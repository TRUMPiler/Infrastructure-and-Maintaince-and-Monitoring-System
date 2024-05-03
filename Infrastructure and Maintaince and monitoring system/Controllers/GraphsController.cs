using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class GraphsController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult Graphs()
        {
            var complaintTypeData = new List<GraphData>();
            var completedComplaintsData = new List<GraphData>();
            var pendingInProgressComplaintsData = new List<GraphData>();

            var allMonths = GetDistinctMonths();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                string complaintTypeQuery = "SELECT Tbl_ComplaintType.ComplaintType, COUNT(*) AS TotalComplaints " +
                                            "FROM Tbl_Complain " +
                                            "INNER JOIN Tbl_ComplaintType ON Tbl_Complain.ComplaintType = Tbl_ComplaintType.Complaint_TypeID " +
                                            "GROUP BY Tbl_ComplaintType.ComplaintType";

                string completedQuery = "SELECT CONVERT(VARCHAR(7), Complain_Registration_Date, 120) AS Month, " +
                                        "COUNT(*) AS TotalComplaints " +
                                        "FROM Tbl_Complain " +
                                        "WHERE Status = 'Completed' " +
                                        "GROUP BY CONVERT(VARCHAR(7), Complain_Registration_Date, 120)";

                string pendingInProgressQuery = "SELECT CONVERT(VARCHAR(7), Complain_Registration_Date, 120) AS Month, " +
                                                 "COUNT(*) AS TotalComplaints " +
                                                 "FROM Tbl_Complain " +
                                                 "WHERE Status = 'Pending' OR Status = 'In Progress' " +
                                                 "GROUP BY CONVERT(VARCHAR(7), Complain_Registration_Date, 120)";

                SqlCommand complaintTypeCommand = new SqlCommand(complaintTypeQuery, connection);
                SqlCommand completedCommand = new SqlCommand(completedQuery, connection);
                SqlCommand pendingInProgressCommand = new SqlCommand(pendingInProgressQuery, connection);

                connection.Open();

                using (SqlDataReader complaintTypeReader = complaintTypeCommand.ExecuteReader())
                {
                    while (complaintTypeReader.Read())
                    {
                        string label = complaintTypeReader["ComplaintType"].ToString();
                        int value = Convert.ToInt32(complaintTypeReader["TotalComplaints"]);
                        complaintTypeData.Add(new GraphData { Label = label, Value = value });
                    }
                }

                using (SqlDataReader completedReader = completedCommand.ExecuteReader())
                {
                    while (completedReader.Read())
                    {
                        string month = completedReader["Month"].ToString();
                        int count = Convert.ToInt32(completedReader["TotalComplaints"]);
                        completedComplaintsData.Add(new GraphData { Label = month, Value = count });
                    }
                }

                using (SqlDataReader pendingInProgressReader = pendingInProgressCommand.ExecuteReader())
                {
                    while (pendingInProgressReader.Read())
                    {
                        string month = pendingInProgressReader["Month"].ToString();
                        int count = Convert.ToInt32(pendingInProgressReader["TotalComplaints"]);
                        pendingInProgressComplaintsData.Add(new GraphData { Label = month, Value = count });
                    }
                }
            }

            completedComplaintsData = FillMissingMonths(completedComplaintsData, allMonths);
            pendingInProgressComplaintsData = FillMissingMonths(pendingInProgressComplaintsData, allMonths);

            ViewBag.ComplaintTypeData = complaintTypeData;
            ViewBag.CompletedComplaintsData = completedComplaintsData;
            ViewBag.PendingInProgressComplaintsData = pendingInProgressComplaintsData;

            return View();
        }

        private List<string> GetDistinctMonths()
        {
            List<string> months = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT CONVERT(VARCHAR(7), Complain_Registration_Date, 120) AS Month " +
                               "FROM Tbl_Complain";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string month = reader["Month"].ToString();
                        months.Add(month);
                    }
                }
            }
            return months;
        }

        private List<GraphData> FillMissingMonths(List<GraphData> data, List<string> allMonths)
        {
            foreach (string month in allMonths)
            {
                if (!data.Any(d => d.Label == month))
                {
                    data.Add(new GraphData { Label = month, Value = 0 });
                }
            }
            return data.OrderBy(d => d.Label).ToList();
        }
    }
}
