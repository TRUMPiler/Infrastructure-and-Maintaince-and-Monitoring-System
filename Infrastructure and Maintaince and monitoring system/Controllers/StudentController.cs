using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class StudentController : Controller
    {
        String connectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        // GET: Student
        public ActionResult StudentProfile()
        {
            if (Session["Role"] != null)
            {
                if (!Session["Role"].ToString().Contains("Student"))
                {
                    return RedirectToAction("Logout","Home");
                }
            }   
                int[] count = { GetPendingComplaints(), GetTotalComplaints() };
            List<String> complaints = GetComplaints();
            StudentPanel p = new StudentPanel()
            {
                count = count,
                PendingComplaints = complaints
            };
            return View(p);
        }
        public int GetTotalComplaints()
        {
            int count = 0;
            string query = "SELECT COUNT(C.ComplainID) AS Count FROM Tbl_Complain C INNER JOIN Tbl_Complaint_User CU ON C.ComplainID = CU.ComplainID INNER JOIN Tbl_Users U ON CU.UserID = U.UserID WHERE U.LoginID ='" + Session["LoginID"] + "'";
            ;


            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                count = (int)com.ExecuteScalar();
            }

            return count;
        }
        public int GetPendingComplaints()
        {
            int count = 0;
            string query = "SELECT COUNT(C.ComplainID) AS Count FROM Tbl_Complain C INNER JOIN Tbl_Complaint_User CU ON C.ComplainID = CU.ComplainID INNER JOIN Tbl_Users U ON CU.UserID = U.UserID WHERE C.Status = 'Pending' AND U.LoginID ='" + Session["LoginID"] + "'";


            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                count = (int)com.ExecuteScalar();
            }

            return count;
        }
        void Connection()
        {
            con.ConnectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        }
        public List<string> GetComplaints()
        {
            List<string> complaints = new List<string>();
            string query = "SELECT C.Description FROM Tbl_Complain C INNER JOIN Tbl_Complaint_User CU ON C.ComplainID = CU.ComplainID INNER JOIN Tbl_Users U ON CU.UserID = U.UserID WHERE C.Status = 'Pending' AND U.LoginID = '" + Session["LoginID"] + "'";


            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                using (SqlDataReader dr = com.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string complaint = dr["Description"].ToString();
                        complaints.Add(complaint);
                    }
                }
            }

            return complaints;
        }
        public ActionResult FileComplaint()
        {
            Complaint cs = new Complaint()
            {
                
                ComplaintTypes = GetAllComplaintTypes()
            };
            return View(cs);
        }
        private List<ComplaintTypes> GetAllComplaintTypes()
        {
            List<ComplaintTypes> complaintTypes = new List<ComplaintTypes>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Tbl_ComplaintType";

                using (SqlCommand com = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        ComplaintTypes ct = new ComplaintTypes()
                        {
                            ComplaintType_ID = (int)reader["Complaint_TypeID"],
                            ComplaintType = reader["ComplaintType"].ToString()
                        };
                        complaintTypes.Add(ct);
                    }
                }
            }

            return complaintTypes;
        }
    }
    
}