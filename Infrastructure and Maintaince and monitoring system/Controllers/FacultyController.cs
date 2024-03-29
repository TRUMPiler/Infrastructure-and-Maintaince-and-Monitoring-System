using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class FacultyController : Controller
    {
        HttpCookie cookieLogin, cookieName, cookieRole;
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string connectionString = "Data Source=ASUSTUFGAMING\\SQLEXPRESS;Initial Catalog=IMMS;Integrated Security=True";

        [HandleError]

        void ConnectionString()
        {
            con.ConnectionString = "Data Source=ASUSTUFGAMING\\SQLEXPRESS;Initial Catalog=IMMS;Integrated Security=True";
        }

        // GET: Faculty
        public ActionResult Index()
        {
            if (Session["Role"] != null)
            {
                if (!Session["Role"].ToString().Contains("Faculty"))
                {
                    return RedirectToAction("Index", "Home");
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
        public ActionResult Profile()
        {
            if (Session["Role"] != null)
            {
                if (!Session["Role"].ToString().Contains("Faculty"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Complaints()
        {
            if(Session["Role"]!=null)
            {
                if (!Session["Role"].ToString().Contains("Faculty"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            
            String Query = "SELECT "
    + "C.ComplainID, "
    + "C.Description, "
    + "CT.ComplaintType, "
    + "C.Image, "
    + "C.Status, "
    + "U.Name AS UserWhoComplained "
+ "FROM "
+ "Tbl_Complain C "
+ "JOIN "
+ "Tbl_ComplaintType CT ON C.ComplaintType = CT.Complaint_TypeID "
+ "JOIN "
    + "Tbl_Complaint_User CU ON C.ComplainID = CU.ComplainID "
+ "JOIN "
    + "Tbl_Users U ON CU.UserID = U.UserID; ";

            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;

            List<Complaint> complaints = new List<Complaint>();
            SqlDataReader reader = com.ExecuteReader();

            while (reader.Read())
            {
                Complaint complaint = new Complaint
                {
                    ComplaintID = (int)reader["ComplainID"],
                    Description = reader["Description"].ToString(),
                    ComplaintType = reader["ComplaintType"].ToString(),
                    Image = reader["Image"].ToString(),
                    Status = reader["Status"].ToString(),
                    User = reader["UserWhoComplained"].ToString()
                };

                complaints.Add(complaint);
            }

            con.Close(); // Close the connection

            return View(complaints);
        }
    }
}