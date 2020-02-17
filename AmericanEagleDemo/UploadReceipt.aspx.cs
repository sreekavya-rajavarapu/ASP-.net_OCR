using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Google.Cloud.Vision.V1;

namespace AmericanEagleDemo
{
    public partial class UploadReceipt : System.Web.UI.Page
    {
        public static DataTable outputDT = new DataTable();
        public static List<string> checkboxs = new List<string>();
        protected void SavePoints_Click(object sender, EventArgs e)
        {
            List<int> custIds = new List<int>();
            // on click of save to profile, get the selected checkboxes and add them
            foreach (GridViewRow objRow in wholesaleDGV.Rows)
            {
                CheckBox chkbkx = objRow.Cells[0].Controls[0] as CheckBox;
                if (chkbkx.Checked)
                {
                    // this item is checked
                    // add it to list of custIds
                    custIds.Add(int.Parse(objRow.Cells[1].Text.ToString())); 
                }
            }
            if (custIds.Count > 0)
            {
                string query = "Insert into points (userid, customerid, points) values";
                // one or more customer id's was checked, add it to database
                foreach (int cusID in custIds)
                {
                    query += "(1," + cusID + ", 5),";
                }
                query = query.Substring(0, (query.Length - 1));
                //UploadStatusLabel.Text = query;
                InsertPointsToTable(query);
            }

        }

        protected void InsertPointsToTable(string query)
        {
            // the query has the insert statement
            try
            {
                SqlConnection conn = new SqlConnection("Data source=.; Database=american_eagle_demo;User Id=sreekavya;Password=sreekavya");
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                UploadStatusLabel.Text = "Updated your points successfully";
                conn.Close();
            }
            catch (Exception e)
            {
                UploadStatusLabel.Text = "Failed to save points to database: " + e.Message.ToString();
            }

        }
        protected void UploadButton_Click(object sender, EventArgs e)
        {
            outputDT.Clear();
            checkboxs.Clear();

            // Specify the path on the server to
            // save the uploaded file to.
            String filePath = Server.MapPath("~/Content/uploads/");
            //filePath = filePath.Replace(@"\", @"\\");
            // Before attempting to perform operations
            // on the file, verify that the FileUpload 
            // control contains a file.
            
            if (FileUpload1.HasFile)
            {
                // Get the name of the file to upload.
                String fileName = FileUpload1.FileName;

                string ext = System.IO.Path.GetExtension(fileName).ToString().Trim();
                UploadStatusLabel.Text += ext;
                if (ext != ".jpeg" && ext != ".png" && ext != ".jpg")
                {
                    UploadStatusLabel.Text += " File extensions should be jpeg or png or jpg";
                    return;
                }

                // Append the name of the file to upload to the path.
                //filePath += "//" fileName;
                DirectoryInfo dir = new DirectoryInfo(Server.MapPath("~/Content/uploads/"));
                if (!dir.Exists)
                {
                    dir.Create();
                }
               
                // Call the SaveAs method to save the 
                // uploaded file to the specified path.
                // This example does not perform all
                // the necessary error checking.               
                // If a file with the same name
                // already exists in the specified path,  
                // the uploaded file overwrites it.
                FileUpload1.SaveAs(filePath + "//" + fileName);
                receiptImage.ImageUrl = "~/Content/uploads/" + Path.GetFileName(FileUpload1.FileName);
                receiptImage.Visible = true;
                // Notify the user of the name of the file
                // was saved under.
                UploadStatusLabel.Text = "Your file was saved as " + fileName;
                

                DataTable dt = GetMedicalMutualCustomers();


                // Call google cloud vision to read the file and detect text


                
                var client = ImageAnnotatorClient.Create();
                var image = Google.Cloud.Vision.V1.Image.FromFile(filePath + "//" + fileName);
                var response = client.DetectText(image);
                
                String allDescription = response[0].Description;
               
                // description = "wholesale\necme\nyour\ncustomer\nagent\nwas\nwendy\n......."
                // "wholesale", "ecme", "your", "customer" .....
                // the all descrpition has \n seperated values
                String[] descriptionArray = allDescription.Split('\n');
                // [.....]
                if (outputDT.Columns.Count <= 0)
                {
                    outputDT.Columns.Add("Select");
                    outputDT.Columns.Add("Customer ID");
                    outputDT.Columns.Add("Item purchased");
                    outputDT.Columns.Add("Confidence");
                }
                

                DataTable receiptDT = new DataTable();
                receiptDT.Columns.Add("Receipt Item");
                receiptDT.Columns.Add("Cost");

                
                for (var i = 0; i < descriptionArray.Length; i++)
                {
                    if (descriptionArray[i] != "" && IsDigitsOnly(descriptionArray[i]) != true)
                    {
                        
                        var ipRow = receiptDT.NewRow();
                        ipRow[0] = descriptionArray[i];
                        ipRow[1] = descriptionArray[i + 1];
                        receiptDT.Rows.Add(ipRow);
                        //Console.WriteLine("Checking for " + description);
                        // for each item in the master whole sale list
                        foreach (DataRow row in dt.Rows)
                        {
                            // remove special characters 
                            Regex reg = new Regex("[*'\",_&#^@]");
                            string receipt_item = reg.Replace(descriptionArray[i].ToLower().Trim(), string.Empty);
                            string db_item = reg.Replace(row[1].ToString().ToLower().Trim(), string.Empty);

                            double similarity = CalculateSimilarity(receipt_item, db_item);

                            // check if this item is already in the output
                            // if it is there then check if the confidence is lower than new
                            bool toSave = true;
                            for (var idx = 0; idx < outputDT.Rows.Count; idx++)
                            {
                                if (outputDT.Rows[idx][1].ToString() == receipt_item)
                                {
                                    if (similarity > float.Parse(outputDT.Rows[idx][2].ToString()))
                                    {
                                        // current confidence is higher than old one
                                        // remove the old row
                                        DataRow dr = outputDT.Rows[idx];
                                        dr.Delete();
                                        break;
                                    }
                                    else
                                    {
                                        // current similarty is not better than old
                                        // keep the old one and discard current
                                        toSave = false;
                                        break;
                                    }
                                }
                            }

                            if (similarity > 0.60 && toSave == true)
                            {
                                var opRow = outputDT.NewRow();
                                opRow[0] = row[0];
                                opRow[1] = receipt_item;
                                opRow[2] = Math.Round(similarity, 2).ToString().ToLower().Trim();
                                outputDT.Rows.Add(opRow);
                            }

                        }
                    }
                }

                wholesaleDGV.DataSource = outputDT;
                wholesaleDGV.DataBind();
                savePointsBtn.Visible = true;

                int index = 0;
                foreach (GridViewRow objRow in wholesaleDGV.Rows)
                {
                    TableCell tcCheckCell = new TableCell();
                    CheckBox chkCheckBox = new CheckBox();
                    chkCheckBox.ID = "chk_bx_" + index;
                    checkboxs.Add(chkCheckBox.ID);
                    tcCheckCell.Controls.Add(chkCheckBox);
                    objRow.Cells.AddAt(0, tcCheckCell);
                    index++;
                }
                loadingSpinner.Attributes.CssStyle.Add("opacity", "0");

            }
            else
            {
                // Notify the user that a file was not uploaded.
                UploadStatusLabel.Text = "You did not specify a file to upload.";
                loadingSpinner.Attributes.CssStyle.Add("opacity", "0");
            }
            

        }


        public static DataTable GetMedicalMutualCustomers()
        {
            string query = "SELECT * FROM  mm_customers";

            using (SqlConnection sqlConn = new SqlConnection("Server=.;Database=american_eagle_demo;User Id=sreekavya;Password=sreekavya;"))
            using (SqlCommand cmd = new SqlCommand(query, sqlConn))
            {
                sqlConn.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
               
                sqlConn.Close();
                return dt;
            }
        }

        static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
       

        /// Returns the number of steps required to transform the source string
        /// into the target string.
        static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        static DataTable CSVToDataTable(string filePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }
            return dt;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
                Response.Redirect("login.aspx");

            if (wholesaleDGV.Rows.Count > 0)
            {
                UploadStatusLabel.Text += wholesaleDGV.Columns.Count.ToString();

                if (wholesaleDGV.Columns.Count > 1)
                {
                    
                    wholesaleDGV.DataSource = outputDT;
                    wholesaleDGV.DataBind();
                    checkboxs.Clear();
                    int index = 0;
                    foreach (GridViewRow objRow in wholesaleDGV.Rows)
                    {
                        TableCell tcCheckCell = new TableCell();
                        CheckBox chkCheckBox = new CheckBox();
                        chkCheckBox.ID = "chk_bx_" + index;
                        checkboxs.Add(chkCheckBox.ID);
                        tcCheckCell.Controls.Add(chkCheckBox);
                        objRow.Cells.AddAt(0, tcCheckCell);
                        index++;
                    }
                }

            }
                
            
            
        }
    }
}