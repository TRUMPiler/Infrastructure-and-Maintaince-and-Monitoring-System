using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Configuration;
using System.Data;
using System.Net.Mail;
using System.Net;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class AdminController : Controller
    {
        HttpCookie cookieLogin, cookieName, cookieRole;
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString; 

        [HandleError]
        void ConnectionString()
        {
            con= new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
        }
        public int GetCount()
        {
            int count = 0;
            string query = "SELECT COUNT(ComplainID) AS Count FROM Tbl_Complain;";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(query, con))
            {
                con.Open();
                count = (int)com.ExecuteScalar();
            }

            return count;
        }
        public ActionResult ComplaintType()
        {
            Complaint complaint = new Complaint()
            {
                ComplaintTypes = GetAllComplaints()
            };
            return View(complaint);
        }
        public ActionResult RegisterUser()
        {
            return View();
        }
        public List<ComplaintTypes> GetAllComplaints()
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
                            ComplaintType = reader["ComplaintType"].ToString(),
                            cStatus=reader.GetBoolean(reader.GetOrdinal("Status"))
                        };
                        complaintTypes.Add(ct);
                    }
                }
            }

            return complaintTypes;
        }
        private void SendEmailToComplaintUsers(int complaintId, String Status)
        {
            
            var users = GETCOMPLAINTUSERS(complaintId);
            var userEmails = users.Select(u => u.Email).ToList();

            foreach (var email in userEmails)
            {
                using (var client = new SmtpClient("smtp.gmail.com", 587)) 
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("dashtaxigg@gmail.com", "cgqgsvvqvjwswyyq");

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("dashtaxigg@gmail.com"),
                        Subject = "Complaint Update Notification",
                        Body = $"Dear User, \n\nThis is to notify you that the complaint with ID: {complaintId} has been marked as '{Status}'.\n\nRegards,\nInfra Care",
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(email);

                    try
                    {
                        client.Send(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine("Exception caught in SendEmailToComplaintUsers(): {0}", ex.ToString());
                    }
                }
            }
        }
        private List<Room> GetAllRooms()
        {
            List<Room> rooms = new List<Room>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT RoomID, RoomNo FROM Tbl_Room"; // Adjust based on your actual column names

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Room room = new Room()
                        {
                            RoomID = (int)reader["RoomID"], // Adjust the column name as necessary
                            RoomNo = reader["RoomNo"].ToString()
                        };
                        rooms.Add(room);
                    }
                }
            }

            return rooms;
        }
        public ActionResult FileComplaint()
        {
            Complaint cs = new Complaint()
            {

                ComplaintTypes = GetAllComplaintTypes(),
                Rooms = GetAllRooms(),
                Users = GetAllUsers()
            };
            return View(cs);
        }
        public ActionResult CompleteComplaint(int? complaintid)
        {
            if (complaintid == null)
            {
                string script = "<script>alert('ComplaintID is missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }


            string query = "UPDATE Tbl_Complain SET Status = 'Completed',Complain_Completion_Date=@ComplainCompletionDate WHERE ComplainID = @ComplainID";


            using (SqlConnection con = new SqlConnection(connectionString))
            {

                con.Open();


                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    
                    cmd.Parameters.Add("@ComplainCompletionDate", SqlDbType.Date).Value = DateTime.Today;
                    cmd.Parameters.AddWithValue("@ComplainID", complaintid);


                    int result = cmd.ExecuteNonQuery();


                    if (result > 0)
                    {
                        SendEmailToComplaintUsers((int)complaintid, "Completed");
                        string script = "<script>alert('Complaint status updated to Completed.');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");

                    }
                    else
                    {
                        string script = "<script>alert('Error updating complaint status. Please try again.');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");
                    }
                }
            }



        }
        private bool CheckIfFeedbackExists(int complaintId, SqlConnection con)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Tbl_Feedback WHERE ComplainID = @ComplaintID", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@ComplaintID", complaintId);
                int feedbackCount = (int)cmd.ExecuteScalar();
                return feedbackCount > 0;
            }
        }
        public ActionResult ViewFeedback(int? complaintID)
        {
            if (complaintID.HasValue)
            {
                List<Feedback> feedbackList = new List<Feedback>();

               
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT FeedbackID, Description, Rating FROM Tbl_Feedback WHERE ComplainID = @ComplaintID";

                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            Feedback feedback = new Feedback
                            {
                                FeedbackID = (int)reader["FeedbackID"],
                                Description = reader["Description"].ToString(),
                                Rating = (int)reader["Rating"]
                            };

                            feedbackList.Add(feedback);
                        }
                    }
                }

                return View(feedbackList);
            }
            else
            {
                string script = "<script>alert('ComplaintID is Missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }
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
        [HttpPost]
        public ActionResult RegisterUser(GetData gd)
        {

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                String Query= "INSERT INTO Tbl_Users(Name, Email, PhoneNo, LoginID, Gender, Role, Password, Status) VALUES(@Name, @Email, @PhoneNo, @LoginID, @Gender, @Role, @Password, @Status); SELECT SCOPE_IDENTITY()";
                using (SqlCommand com = new SqlCommand(Query, con))
                {
                    con.Open();
                    if (gd.Role.Contains("Faculty"))
                    {
                        if (IsEmailExists(gd.Email, con))
                        {
                            string scripts = "<script>alert('The Details of this Faculty is already Registered into the System');window.location='/Admin/Users'</script>";
                            return Content(scripts, "text/html");
                        }
                    }
                    else if(gd.Role.Contains("Student"))
                    {
                        if (IsEmailExists(gd.LoginID, con))
                        {
                            string scripts = "<script>alert('The Details of this Student is already Registered into the System');window.location='/Admin/Users'</script>";
                            return Content(scripts, "text/html");
                        }
                    }
                    com.Parameters.AddWithValue("@Name", gd.Name);
                    com.Parameters.AddWithValue("@Email", gd.Email);
                    com.Parameters.AddWithValue("@PhoneNo", gd.PhoneNo);
                    if (gd.Role.Contains("Faculty"))
                    {
                        com.Parameters.AddWithValue("@LoginID", gd.Email);
                    }
                    else
                    {
                        com.Parameters.AddWithValue("@LoginID", gd.LoginID);
                    }
                    com.Parameters.AddWithValue("@Gender", gd.Gender);
                    com.Parameters.AddWithValue("@Role", gd.Role);
                    com.Parameters.AddWithValue("@Password", gd.Password);
                    com.Parameters.AddWithValue("@Status", 1);

                    int userID = Convert.ToInt32(com.ExecuteScalar());


                    if (gd.Role.Contains("Student"))
                    {

                        com.CommandText = "INSERT INTO Tbl_Students (UserId, Semster) VALUES (@UserID, @Sem)";
                        com.Parameters.Clear();
                        com.Parameters.AddWithValue("@UserID", userID);
                        com.Parameters.AddWithValue("@Sem", Convert.ToInt32(gd.Semester));
                        com.ExecuteNonQuery(); 
                    }


                    // Execute query and get the complaint ID
                }
            }
            string script = "<script>alert('Registration is Completed');window.location='/Admin/Users'</script>";
            return Content(script, "text/html");

        }

        private bool IsEmailExists(string email, SqlConnection con)
        {
            string emailCheckQuery = "SELECT COUNT(*) FROM Tbl_Users WHERE LoginID = @Email";
            using (SqlCommand emailCheckCommand = new SqlCommand(emailCheckQuery, con))
            {
                emailCheckCommand.Parameters.AddWithValue("@Email", email);
                int emailCount = (int)emailCheckCommand.ExecuteScalar();
                return emailCount > 0;
            }
        }
        private bool IsLoginIDExists(string loginID, SqlConnection con)
        {
            string loginIDCheckQuery = "SELECT COUNT(*) FROM Tbl_Users WHERE LoginID = @LoginID";
            using (SqlCommand loginIDCheckCommand = new SqlCommand(loginIDCheckQuery, con))
            {
                loginIDCheckCommand.Parameters.AddWithValue("@LoginID", loginID);
                int loginIDCount = (int)loginIDCheckCommand.ExecuteScalar();
                return loginIDCount > 0;
            }
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
        public List<GetData> GetAllUsers()
        {
            List<GetData> ls = new List<GetData>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT u.UserID, " +
"u.Email, " +
"u.Gender, " +
"u.Role, " +
"u.PhoneNo, " +
"u.Name, " +
"u.LoginID, " +
"u.Password, " +
"u.Status " +
"FROM Tbl_Users u " +
" Where u.Role!='Admin' AND u.Status=1" +
"";

                using (SqlCommand com = new SqlCommand(query, con))
                {
                    con.Open();
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
        private void RegisterComplaintUsers(int[] selectedUsers, int complaintId, string connectionString)
        {
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
               
                connection.Open();

             
                string query = "INSERT INTO Tbl_Complaint_User (UserID, ComplainID) VALUES (@UserID, @ComplainID);";

             
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                  
                    command.Parameters.AddWithValue("@ComplainID", complaintId);
                    command.Parameters.AddWithValue("@UserID", Session["UserID"]);
                    command.ExecuteNonQuery();
                    
                    if(selectedUsers!=null)
                    { 
                        foreach (int userId in selectedUsers)
                        {
                            // Clear previous parameters
                            command.Parameters.Clear();
                            // Add parameters for the new command
                            command.CommandText = "INSERT INTO Tbl_Complaint_User (ComplainID, UserID) VALUES (@ComplainID, @UserID)";
                            command.Parameters.AddWithValue("@ComplainID", complaintId);
                            command.Parameters.AddWithValue("@UserID", userId);
                            command.ExecuteNonQuery();
                        }
                    }
                }

            }
        }
        [HttpPost]
        public ActionResult FileComplaint(Complaint cs)
        {
            
            int complaintId = RegisterComplaint(cs, connectionString);
            if (complaintId == 0)
            {
                string script = "<script>alert('Image is Empty');window.location='/Admin/Users'</script>";
                return Content(script, "text/html");
            }
            else if (complaintId == 1)
            {
                string script = "<script>alert('Complaint is Empty');window.location='/Admin/Users'</script>";
                return Content(script, "text/html");
            }
            
            RegisterComplaintUsers(cs.SelectedUser, complaintId, connectionString);

         
            return RedirectToAction("Index");
        }

        private int RegisterComplaint(Complaint cs, string connectionString)
        {
            // Create connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                connection.Open();

                if (cs.ComplaintImage != null && cs.ComplaintImage.ContentLength > 0)
                {
                    try
                    {

                        string fileName = Path.GetFileName(cs.ComplaintImage.FileName);
                        String path = Path.Combine(Server.MapPath("~\\Admins\\dist\\img"), fileName);
                        cs.ComplaintImage.SaveAs(path);
                        cs.Image = fileName;



                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    cs.Image = "No Image";
                }

              
                string query = "INSERT INTO [dbo].[Tbl_Complain]([Description],[ComplaintType],[ClassID],[Image],[Status],[Complain_Registration_Date]) VALUES (@Description, @ComplaintType, @ClassID,@Image, @Status,@ComplainRegisterDate); SELECT SCOPE_IDENTITY();";

               
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                   
                    command.Parameters.Add("@Description", SqlDbType.VarChar).Value = cs.Description;
                    command.Parameters.Add("@ComplaintType", SqlDbType.Int).Value = cs.ComplaintType;
                    command.Parameters.Add("@Image", SqlDbType.VarChar).Value = cs.Image;
                    command.Parameters.Add("@Status", SqlDbType.VarChar).Value = "Pending"; // Default status
                    command.Parameters.Add("@ClassID", SqlDbType.Int).Value = cs.ClassID;
                    command.Parameters.Add("@ComplainRegisterDate", SqlDbType.Date).Value = DateTime.Today;
               
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        private List<ComplaintTypes> GetAllComplaintTypes()
        {
            List<ComplaintTypes> complaintTypes = new List<ComplaintTypes>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Tbl_ComplaintType where Status=1 ";

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
       

        
        public ActionResult Users()
        {
            if(Session["Role"]!=null)
            {
                if (!Session["Role"].Equals("Admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            String Query = "select * from Tbl_Users";
            
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
                if (!Session["Role"].Equals("Admin"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            String Query = "SELECT c.ComplainID, " +
           "ct.ComplaintType, " +
           "c.Description, " +
           "c.Image, " +
           "c.Status, " +
           "c.ClassID,c.Complain_Registration_Date,c.Complain_Completion_Date " +
        "FROM Tbl_Complain  c " +
        "INNER JOIN Tbl_ComplaintType ct ON c.ComplaintType = ct.Complaint_TypeID WHERE c.Status NOT IN ('Pending', 'Rejected')";


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
                            Complain_Completion_Date = reader["Complain_Completion_Date"].ToString(),
                            Complain_Registration_Date= reader["Complain_Registration_Date"].ToString(),
                            Users = GETCOMPLAINTUSERS((int)reader["ComplainID"]),
                            HasFeedback = CheckIfFeedbackExists((int)reader["ComplainID"],con)
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

                    
                    con.Open();
                    com.Connection = con;

                    
                    using (StreamReader reader = new StreamReader(path))
                    {
                        
                        reader.ReadLine();

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] fields = line.Split(',');

                           
                            bool status = Convert.ToBoolean(fields[8]);

                           
                            com.CommandText = "INSERT INTO Tbl_Users (Name, Email, PhoneNo, LoginID, Gender, Role, Password, Status) VALUES (@Name, @Email, @PhoneNo, @LoginID, @Gender, @Role, @Password, @Status); SELECT SCOPE_IDENTITY()";
                            com.Parameters.AddWithValue("@Name", fields[1]);
                            com.Parameters.AddWithValue("@Email", fields[2]);
                            com.Parameters.AddWithValue("@PhoneNo", fields[3]);
                            com.Parameters.AddWithValue("@LoginID", fields[4]);
                            com.Parameters.AddWithValue("@Gender", fields[5]);
                            com.Parameters.AddWithValue("@Role", fields[6]);
                            com.Parameters.AddWithValue("@Password", fields[7]);
                            com.Parameters.AddWithValue("@Status", status);

                            int userID = Convert.ToInt32(com.ExecuteScalar());
                            

                           
                            com.CommandText = "INSERT INTO Tbl_Students (UserId, Semster) VALUES (@UserID, @Sem)";
                                com.Parameters.Clear();
                                com.Parameters.AddWithValue("@UserID", userID);
                                com.Parameters.AddWithValue("@Sem", Convert.ToInt32(fields[9]));

                                com.ExecuteNonQuery();
                            com.CommandText = "DELETE FROM Tbl_Students WHERE Semster = 15";
                            com.ExecuteNonQuery();
                        }
                    }

                    return View("Register", model: "File uploaded");
                }
                catch (Exception ex)
                {
                    return View("Register", model: ex.Message);
                }
                finally
                {
                    
                    con.Close(); 
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
                if (!Session["Role"].ToString().Contains("Admin"))
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
                if (!Session["Role"].ToString().Contains("Admin"))
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
                if(Session["Role"]!=null)
                {
                    if (!Session["Role"].Equals("Admin"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["UserID"] = userId;
                String Query = "select * from Tbl_Users where UserID="+userId;
                
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
        public ActionResult Deactive(String tableName, int id)
        {
            if (string.IsNullOrWhiteSpace(tableName) || !tableName.Equals("Tbl_ComplaintType", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid table name");
            }

            string query = "Update Tbl_ComplaintType set Status=0 where Complaint_TypeID=@id";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    com.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    con.Open();
                    int rowsAffected = com.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        string script = "<script>alert('Type was Deactivated Successfully');window.location='/Admin/ComplaintType'</script>";
                        return Content(script, "text/html");
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.NotFound, "No rows updated");
                    }
                }
            }
            catch (Exception ex)
            {
                
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error during operation");
            }
        }


        public ActionResult Active(String TableName,int id)
        {
            String Query="";
            if (TableName.Contains("Tbl_ComplaintType"))
            {
                Query = "Update Tbl_ComplaintType set Status=1 where Complaint_TypeID=@id";
                
            }
            else
            {
                Query = "Update Tbl_ComplaintType set Status=1 where Complaint_TypeID=0";
            }
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(Query, con))
            {
                       com.Parameters.Add("@id", SqlDbType.Int).Value = id;
                       con.Open();
                       com.ExecuteNonQuery();
                       string script = "<script>alert('Type was Actived Successfully');window.location='/Admin/ComplaintType'</script>";
                       return Content(script, "text/html");
            }
        }

        public ActionResult ManageRoom()
        {


            string query = "SELECT R.RoomID, RT.RoomtType AS RoomType, R.Wing, R.RoomNo, R.Status FROM Tbl_Room R INNER JOIN Tbl_RoomType RT ON R.RoomType = RT.RoomTypeID;";
            ConnectionString();

            List<Room> rooms = new List<Room>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        Room getrooms = new Room
                        {
                            RoomID = (int)reader["RoomID"],
                            RoomType = reader["RoomType"].ToString(),
                            Wing = reader["Wing"].ToString()[0],
                            RoomNo = reader["RoomNo"].ToString(),
                            Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                        };

                        rooms.Add(getrooms);
                    }
                }
            }

            return View(rooms);
        }
        public ActionResult ManageRoomType()
        {


            string query = "select * from Tbl_RoomType;;";
            ConnectionString();

            List<RoomType> roomtype = new List<RoomType>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        RoomType getroomtype = new RoomType
                        {
                            RoomTypeID = (int)reader["RoomTypeId"],
                            Roomtype = reader["RoomtType"].ToString(),
                            Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                        };

                        roomtype.Add(getroomtype);
                    }
                }
            }

            return View(roomtype);
        }

        public ActionResult AddRoom()
        {
            Room room = new Room()
            {
                roomTypes = GetRoomtype()
            };
            return View(room);
        }
        public ActionResult AddRoomType()
        {
            return View();
        }


        [HttpPost]
        public ActionResult AddRoom(Room room)
        {
            int roomtype = int.Parse(room.RoomType);
            string query = "Insert into Tbl_Room (RoomType,Wing,RoomNo,Status) values(" + roomtype + ",'" + room.Wing + "'," + room.RoomNo + ",1);";
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('Room data was inserted Successfully');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Insertion Failed');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error?error=" + e + "", "Error");
                
            }
        }
        [HttpPost]
        public ActionResult AddRoomType(String Roomtype, bool Status)
        {
            string query = "Insert into Tbl_RoomType (RoomtType,Status) values(@RoomType,@Status);";
            ConnectionString();
            con.Open();
            com.Connection = con;
           
            com.CommandText = query;
            com.Parameters.Add("@RoomType", SqlDbType.VarChar).Value = Roomtype;
            com.Parameters.Add("@Status", SqlDbType.Bit).Value = Status;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('RoomType data was inserted Successfully');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Insertion Failed');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
                
                return RedirectToAction("Error?error=" + e + "", "Error");
            }
        }
        public ActionResult DeactivateRoom(int? RoomID)
        {

            String Query = "update Tbl_Room SET Status=0 where RoomID=" + RoomID;
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('Room deactivated Successfully');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Deactivation Failed');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error?error=" + e + "", "Error");
            }
        }
        public ActionResult ActivateRoom(int? RoomID)
        {

            String Query = "update Tbl_Room SET Status=1 where RoomID=" + RoomID;
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('Room Activated Successfully');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Activation Failed');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error?error=" + e + "", "Error");
            }
        }
        public ActionResult DeactivateRoomType(int? RoomID)
        {

            String Query = "update Tbl_RoomType SET Status=0 where RoomTypeID=" + RoomID;
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('RoomType deactivated Successfully');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Deactivation Failed');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
               
                return RedirectToAction("Error?error=" + e + "", "Error");
            }
        }
        public ActionResult ActivateRoomType(int? RoomID)
        {

            String Query = "update Tbl_RoomType SET Status=1 where RoomTypeID=" + RoomID;
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('RoomType Activated Successfully');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Activation Failed');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
               
                return RedirectToAction("Error?error=" + e + "", "Error");
            }
        }

        [HttpPost]
        public ActionResult EditRoom(Room room)
        {
            String Query = "Update Tbl_Room set Wing='" + room.Wing + "', RoomNo=" + room.RoomNo + ", RoomType=" + Convert.ToInt32(room.RoomType) + " where RoomID=" + room.RoomID;
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('Room data was updated Successfully');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Updation Failed');window.location='/Admin/ManageRoom'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error?error=" + e + "", "Error");
            }



        }
        public ActionResult EditRoom(int? RoomID)
        {
            String Query = "SELECT R.RoomID, RT.RoomtType AS RoomType, R.Wing, R.RoomNo FROM Tbl_Room R INNER JOIN Tbl_RoomType RT ON R.RoomType = RT.RoomTypeID where RoomID=" + RoomID;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                com.Connection = con;
                com.CommandText = Query;

                SqlDataReader reader = com.ExecuteReader();
                Room getRoom;
                while (reader.Read())
                {
                    getRoom = new Room
                    {
                        RoomID = (int)reader["RoomID"],
                        RoomType = reader["RoomType"].ToString(),
                        Wing = reader["Wing"].ToString()[0],
                        RoomNo = reader["RoomNo"].ToString(),
                        roomTypes = GetRoomtype()
                    };
                    return View(getRoom);

                }
            }
            return View();
        }
        public ActionResult EditRoomType(int? RoomtypeID)
        {
            String Query = "select * from Tbl_RoomType where RoomTypeID=" + RoomtypeID;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                com.Connection = con;
                com.CommandText = Query;

                SqlDataReader reader = com.ExecuteReader();
                RoomType getRoomtype;
                while (reader.Read())
                {
                    getRoomtype = new RoomType
                    {
                        RoomTypeID = (int)reader["RoomTypeID"],
                        Roomtype = reader["RoomtType"].ToString(),
                        Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                    };
                    return View(getRoomtype);

                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult EditRoomType(int RoomTypeID, String Roomtype)
        {
            String Query = "update Tbl_RoomType set RoomtType='" + Roomtype + "' where RoomTypeID=" + RoomTypeID + ";";
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            try
            {
                int i = com.ExecuteNonQuery();
                if (i >= 1)
                {
                    string script = "<script>alert('RoomType data was updated Successfully');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
                else
                {
                    string script = "<script>alert('Updation Failed');window.location='/Admin/ManageRoomType'</script>";
                    return Content(script, "text/html");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error?error=" + e + "", "Error");
            }



        }











        public List<RoomType> GetRoomtype()
        {
            List<RoomType> lsRoomtype = new List<RoomType>();
            string query = "Select * from Tbl_RoomType;";
            ConnectionString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        RoomType getrooms = new RoomType
                        {
                            RoomTypeID = (int)reader["RoomTypeID"],
                            Roomtype = reader["RoomtType"].ToString()
                        };

                        lsRoomtype.Add(getrooms);
                    }
                }
            }


            return lsRoomtype;
        }
        public List<RoomType> GetRoomtypeDetails()
        {
            List<RoomType> lsRoomtype = new List<RoomType>();
            string query = "Select * from Tbl_RoomType;";
            ConnectionString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand com = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        RoomType getrooms = new RoomType
                        {
                            RoomTypeID = (int)reader["RoomTypeID"],
                            Roomtype = reader["RoomtType"].ToString(),
                            Status = reader.GetBoolean(reader.GetOrdinal("Status"))
                        };

                        lsRoomtype.Add(getrooms);
                    }
                }
            }


            return lsRoomtype;
        }
        public ActionResult ComplaintHistory()
        {
            String Query = "SELECT " +
    "C.ComplainID, " +
    "C.Description,C.Complain_Registration_Date,C.Complain_Completion_Date," +
    "CT.ComplaintType, " +
    "C.Status,C.Image " +
    "FROM " +
    "Tbl_Complain C " +
"INNER JOIN " +
    "Tbl_Complaint_User CU ON C.ComplainID = CU.ComplainID " +
"INNER JOIN " +
    "Tbl_ComplaintType CT ON C.ComplaintType = CT.Complaint_TypeID " +
"WHERE " +

    "CU.UserID =(SELECT UserID FROM Tbl_Users WHERE LoginID ='" + Session["LoginID"] + "');";
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
                            Users = GETCOMPLAINTUSERS((int)reader["ComplainID"]),
                            Complain_Registration_Date = reader["Complain_Registration_Date"].ToString(),
                            Complain_Completion_Date = reader["Complain_Completion_Date"].ToString(),
                            HasFeedback = CheckIfFeedbackExists((int)reader["ComplainID"], con)  // Check feedback status
                        };

                        complaints.Add(complaint);
                    }
                }
            }

            return View(complaints);
        }
        public ActionResult DeleteComplaint(int? complaintID)
        {
            if (complaintID.HasValue)
            {
               
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

               
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                       
                        string deleteUserQuery = "DELETE FROM Tbl_Complaint_User WHERE ComplainID = @ComplaintID";
                        using (SqlCommand deleteUserCmd = new SqlCommand(deleteUserQuery, con, transaction))
                        {
                            deleteUserCmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                            deleteUserCmd.ExecuteNonQuery();
                        }

                        
                        string deleteComplaintQuery = "DELETE FROM Tbl_Complain WHERE ComplainID = @ComplaintID";
                        using (SqlCommand deleteComplaintCmd = new SqlCommand(deleteComplaintQuery, con, transaction))
                        {
                            deleteComplaintCmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                            deleteComplaintCmd.ExecuteNonQuery();
                        }

                       
                        transaction.Commit();
                        string script = "<script>alert('Complaint was Reverted Successfully');window.location='/Student/StudentProfile'</script>";
                        return Content(script, "text/html");
                    }
                    catch (Exception ex)
                    {
                       
                        transaction.Rollback();
                        string script = "<script>alert('Complaint was not Reverted Successfully');window.location='/Admin/Index'</script>";
                        return Content(script, "text/html");
                    }
                }



            }
            else
            {
                string script = "<script>alert('ComplaintID is missing');window.location='/Admin/Index'</script>";
                return Content(script, "text/html");
            }
        }

    }

}