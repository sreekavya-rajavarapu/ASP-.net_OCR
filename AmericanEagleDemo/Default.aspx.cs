using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AmericanEagleDemo
{
    public partial class _Default : Page
    {

        protected void OnPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            pointsDGV.PageIndex = e.NewPageIndex;
            DataTable dt = new DataTable();
            dt = GetUserPoints();
            pointsDGV.DataSource = dt;
            pointsDGV.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
                Response.Redirect("login.aspx");
            // on load of page get all points and add to data grid view
            DataTable dt = new DataTable();
            dt = GetUserPoints();
            pointsDGV.DataSource = dt;
            pointsDGV.DataBind();
        }

        [WebMethod]
        public static List<object> GetChartData()
        {
            string query = "SELECT months.name as Month, SUM(points) as TotalPoints";
            query += " FROM points RIGHT OUTER JOIN months ON months.id = DATEPART(month, date) GROUP BY months.name, months.id ORDER BY months.id";
            string constr = "Server=.;Database=american_eagle_demo;User Id=sreekavya;Password=sreekavya;";
            List<object> chartData = new List<object>();
            chartData.Add(new object[]
            {
                "Month", "TotalPoints"
            });
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            chartData.Add(new object[]
                            {
                                sdr["Month"], sdr["TotalPoints"]
                            });
                        }
                    }
                    con.Close();
                    return chartData;
                }
            }
        }

            protected static DataTable GetUserPoints()
        {
            string query = "SELECT mm_customers.name as  \"Customer Name\", points.date as Date, points.points as Points FROM  points join mm_customers on points.customerid = mm_customers.id order by date desc";

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
    }
}