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
        string connectionString = "Data Source=DESKTOP-T7RS5U7\\SQLEXPRESS;Initial Catalog=IMMS;Integrated Security=True";

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
            if (Session["Role"] != null)
            {
                if (!Session["Role"].ToString().Contains("admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            cookieLogin = Request.Cookies["Login"];
            cookieName = Request.Cookies["Name"];
            cookieRole = Request.Cookies["Role"];
            if(cookieLogin!=null)
            {
                cookieLogin.Expires = DateTime.Now.AddSeconds(0);
                cookieName.Expires = DateTime.Now.AddSeconds(0);
                cookieRole.Expires = DateTime.Now.AddSeconds(0);
                Response.Cookies.Add(cookieLogin);
                Response.Cookies.Add(cookieName);
                Response.Cookies.Add(cookieRole);
            }
            
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
            
            con.ConnectionString = "Data Source=ASUSTUFGAMING\\SQLEXPRESS;Initial Catalog=IMMS;Integrated Security=True";
            
        }

        
        public ActionResult Users()
        {
            if(Session["Role"]!=null)
            {
                if (!Session["Role"].Equals("admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
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
                    Password = reader["Password"].ToString(),
                    Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                };

                getDataList.Add(getData);
            }
            return View(getDataList);
        }
        public ActionResult Complaints()
        {
            if (Session["Role"] != null)
            {
                if (!Session["Role"].Equals("admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            String Query = "SELECT c.ComplainID, " +
           "ct.ComplaintType, " +
           "c.Description, " +
           "c.Image, " +
           "c.Status, " +
           "c.ClassID " +
        "FROM Tbl_Complain  c " +
        "INNER JOIN Tbl_ComplaintType ct ON c.ComplaintType = ct.Complaint_TypeID where c.Status!='Pending'";


            List<Complaint> complaints = new List<Complaint>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand(Query, con))
                {
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
                            Users = GETCOMPLAINTUSERS((int)reader["ComplainID"])
                        };

                        complaints.Add(complaint);
                    }
                }
            }

            return View(complaints);
        }

        public List<GetData> GETCOMPLAINTUSERS(int COMPLAINTID)
        {

            List<GetData> ls = new List<GetData>();
            String Query = "SELECT u.UserID, " +
           "u.Email, " +
           "u.Gender, " +
           "u.Role, " +
           "u.PhoneNo, " +
           "u.Name, " +
           "u.LoginID, " +
           "u.Password, " +
           "u.Status " +
        "FROM Tbl_Users u " +
        "INNER JOIN Tbl_Complaint_User cu ON u.UserID = cu.UserID " +
        "WHERE cu.ComplainID = " + COMPLAINTID + "";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand(Query, con))
                {
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        GetData gd = new GetData()
                        {
                            UserID = (int)reader["UserID"],
                            Email = reader["Email"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Role = reader["Role"].ToString(),
                            PhoneNo = reader["PhoneNo"].ToString(),
                            Name = reader["Name"].ToString(),
                            LoginID = reader["LoginID"].ToString(),
                            Password = reader["Password"].ToString(),
                            Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                        };

                        ls.Add(gd);
                    }
                }
            }

            return ls;
        }

        public ActionResult Delete(int? userID)
        {
            

                if (!userID.HasValue)
                {
                    return RedirectToAction("Users");
                }
                
                String Query = "update Tbl_Users SET Status=0 where UserID=" + userID;
                ConnectionString();
                con.Open();
                com.Connection = con;
                com.CommandText = Query;
                try
                {
                    int i = com.ExecuteNonQuery();
                    if (i >= 1)
                    {
                        string script = "<script>alert('User deactivated Successfully');window.location='/Admin/Users'</script>";
                        return Content(script, "text/html");
                    }
                    else
                    {
                        string script = "<script>alert('User deactivated Unsuccessfully');window.location='/Admin/Users'</script>";
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

            if (Session["Role"] != null)
            {
                if (!Session["Role"].Equals("admin"))
                {
                    return RedirectToAction("Logout");
                }
                AdminPanel ap = new AdminPanel
                { completed = GetDoneCount(),
                    count = GetCount(),
                    todo = GetComplaints(),
                    avg=(GetDoneCount()+GetCount())/2,
                    TotalStudents=CountAllUsers()
                };
                return View(ap);
            }
            else
            {
                return RedirectToAction("Logout");
            }    
            
        }

        [HttpPost]
        public ActionResult EditUser1(GetData gd)
        {

            if(Session["UserID"]==null)
            {
                return RedirectToAction("Users");
            }
            int userId = Convert.ToInt32(Session["UserID"]);
            String Query = "update Tbl_Users set Name='"+gd.Name+ "', Email='" + gd.Email + "', PhoneNo='" + gd.PhoneNo + "', Gender='" + gd.Gender + "', LoginID='" + gd.LoginID + "', Role='"+gd.Role+"', Status='"+gd.Status+"' where UserID=" + userId;
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
            
            
            
        }
        public ActionResult Register()
        {
            if (Session["Role"] != null)
            {
                if (!Session["Role"].ToString().Contains("admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }
        public int CountAllUsers()
        {
            int count = 0;
            string query = "SELECT COUNT(LoginID) AS Count FROM Tbl_Users Where Status=1";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                count = (int)com.ExecuteScalar();
            }

            return count;
        }
        public ActionResult EditComplaint(int? cid)
        {
            if (cid.HasValue)
            {
                Complaint cs=new Complaint();
                String Query = "select * from Tbl_Complain where ComplainID=" + cid;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand com = new SqlCommand(Query, con))
                    {
                       

                        con.Open();
                        SqlDataReader reader = com.ExecuteReader();

                        if (reader.Read())
                        {
                            cs = new Complaint()
                            {
                                ComplaintID = (int)reader["ComplainID"],
                                Description = reader["Description"].ToString(),
                                ComplaintType = reader["ComplaintType"].ToString(),
                                ClassID = reader["ClassID"].ToString(),
                                Status = reader["Status"].ToString(),
                                ComplaintTypes = GetAllComplaintTypes()
                            };
                        }
                    }
                }
                return View(cs);
            }
            else
            {
                return RedirectToAction("Complaints");
            }
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
                        Password = reader["Password"].ToString(),
                        Status= reader.GetBoolean(reader.GetOrdinal("Status"))
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