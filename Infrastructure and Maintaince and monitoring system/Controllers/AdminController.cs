using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.IO;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class AdminController : Controller
    {
        HttpCookie cookieLogin, cookieName, cookieRole;
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string connectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";

        [HandleError]

        public int GetCount()
        {
            int count = 0;
            string query = "SELECT COUNT(ComplainID) AS Count FROM Tbl_Complain WHERE Status='Pending';";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                count = (int)com.ExecuteScalar();
            }

            return count;
        }
        public int GetDoneCount()
        {
            int count = 0;
            string query = "SELECT COUNT(ComplainID) AS Count FROM Tbl_Complain WHERE Status='Completed';";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                count = (int)com.ExecuteScalar();
            }

            return count;
        }
        public ActionResult Logout()
        {
            Session.RemoveAll();
            cookieLogin = Request.Cookies["Login"];
            cookieName = Request.Cookies["Name"];
            cookieRole = Request.Cookies["Role"];
            cookieLogin.Expires = DateTime.Now.AddSeconds(0);
            cookieName.Expires = DateTime.Now.AddSeconds(0);
            cookieRole.Expires = DateTime.Now.AddSeconds(0);
            Response.Cookies.Add(cookieLogin);
            Response.Cookies.Add(cookieName);
            Response.Cookies.Add(cookieRole);
            return RedirectToAction("Index", "Home");
        }
        public List<string> GetComplaints()
        {
            List<string> complaints = new List<string>();
            string query = "SELECT Description FROM Tbl_Complain WHERE Status='Pending';";

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
        void ConnectionString()
        {
            con.ConnectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        }
        public ActionResult Users()
        {
            if (!Session["Role"].Equals("admin"))
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (!Session["Role"].Equals("admin"))
            {
                return RedirectToAction("Index", "Home");
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
        public ActionResult Delete(int? userID)
        {
            

                if (!userID.HasValue)
                {
                    return RedirectToAction("Users");
                }
                
                String Query = "delete from Tbl_Users where UserID=" + userID;
                ConnectionString();
                con.Open();
                com.Connection = con;
                com.CommandText = Query;
                try
                {
                    int i = com.ExecuteNonQuery();
                    if (i >= 1)
                    {
                        string script = "<script>alert('User deleted Successfully');window.location='/Admin/Users'</script>";
                        return Content(script, "text/html");
                    }
                    else
                    {
                        string script = "<script>alert('User deleted Unsuccessfully');window.location='/Admin/Users'</script>";
                        return Content(script, "text/html");
                    }
                }
                catch (Exception e)
                {
                    if(e.ToString().Contains("REFERENCE constraint"))
                    {
                        string script = "<script>alert('User cannot be deleted as it has some records assoicated with it ');window.location='/Admin/Users'</script>";
                        return Content(script, "text/html");
                    }
                    return RedirectToAction("Error?error=" + e + "", "Error");
                }


                return RedirectToAction("Users");

            }
        [HttpPost]
        public ActionResult Register1(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {

                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~\\App_Data"), fileName);
                    file.SaveAs(path);
                    String Query = "BULK insert Tbl_Users " +
                    " from '" + path + "'"
                    + " WITH("
                    + " FORMAT = 'CSV',"
                    + " FIRSTROW = 2,"
                    + " FIELDTERMINATOR = ',',"
                    + " ROWTERMINATOR = '\\n'"
                    + " )";

                    ConnectionString();
                    con.Open();
                    com.Connection = con;
                    com.CommandText = Query;
                    com.ExecuteNonQuery();
                    return View("Register", model: "File uploaded");
                }
                catch (Exception ex)
                {
                    return View("Register", model: ex.Message);
                }
            }
            else
            {
                return View("Register", model: "please select a file");
            }
        }
        public ActionResult Index()
        {
            if (!Session["Role"].Equals("admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            AdminPanel ap = new AdminPanel
            { completed = GetDoneCount(),
                count = GetCount(),
                todo = GetComplaints(),
                avg=(GetDoneCount()+GetCount())/2
            };
            return View(ap);
        }

        [HttpPost]
        public ActionResult EditUser1(GetData gd)
        {

            if(Session["UserID"]==null)
            {
                return RedirectToAction("Users");
            }
            int userId = Convert.ToInt32(Session["UserID"]);
            String Query = "update Tbl_Users set Name='"+gd.Name+ "', Email='" + gd.Email + "', PhoneNo='" + gd.PhoneNo + "', Gender='" + gd.Gender + "', LoginID='" + gd.LoginID + "' where UserID=" + userId;
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if(i>=1)
                {
                    string script = "<script>alert('User data was updated Successfully');window.location='/Admin/Users'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('User data was updated Unsuccessfully');window.location='/Admin/Users'</script>";
                    return Content(script, "text/html");
                }
            }catch(Exception e)
            {
                return RedirectToAction("Error?error="+e+"", "Error");
            }
            
            
            return RedirectToAction("Users");
        }
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult EditUser(int? userId)
        {
            if(userId.HasValue)
            {
                if (!Session["Role"].Equals("admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["UserID"] = userId;
                String Query = "select * from Tbl_Users where UserID="+userId;
                ConnectionString();
                con.Open();
                com.Connection = con;
                com.CommandText = Query;

                SqlDataReader reader = com.ExecuteReader();
                GetData getData;
                while (reader.Read())
                {
                    getData = new GetData
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
                    return View(getData);

                }
                
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View();
        }       
    
    }
}