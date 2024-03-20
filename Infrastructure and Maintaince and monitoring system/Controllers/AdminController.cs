using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class AdminController : Controller
    {
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        // GET: Admin
        [HandleError]
        public ActionResult Index()
        {
            return View();
        }
        void ConnectionString()
        {
            con.ConnectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        }
        public ActionResult Users()
        {

            String Query = "select * from Tbl_Users";
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            List<GetData> getDataList = new List<GetData>();
            SqlDataReader reader = com.ExecuteReader();

            while (reader.Read())
            {
                GetData getData = new GetData
                {
                    UserID = (int)reader["UserID"],
                    Email = reader["Email"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Role = reader["Role"].ToString(),
                    PhoneNo = reader["PhoneNo"].ToString(),
                    Name = reader["Name"].ToString(),
                    LoginID = reader["LoginID"].ToString(),
                    Password = reader["Password"].ToString()
                };

                getDataList.Add(getData);
            }
            return View(getDataList);
        }
        public ActionResult Complaints()
        {

            String Query = "SELECT "
    +"C.ComplainID, "
    +"C.Description, "
    +"CT.ComplaintType, "
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