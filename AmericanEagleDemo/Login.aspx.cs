using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AmericanEagleDemo
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblErrorMessage.Visible = false;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            using (SqlConnection sqlCon = new SqlConnection(@"Data Source=.;initial Catalog=american_eagle_demo;User Id=sreekavya;Password=sreekavya;"))
            {
                sqlCon.Open();
                string query = "SELECT COUNT(1) FROM [user] WHERE uname=@uname AND password=@password";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@uname", txtUserName.Value.Trim());
                sqlCmd.Parameters.AddWithValue("@password", txtPassword.Value.Trim());
                int count = Convert.ToInt32(sqlCmd.ExecuteScalar());
                if (count == 1)
                {
                    Session["username"] = txtUserName.Value.Trim();
                    Response.Redirect("Default.aspx");
                }
                else { lblErrorMessage.Visible = true; }
            }
        }
    }
}