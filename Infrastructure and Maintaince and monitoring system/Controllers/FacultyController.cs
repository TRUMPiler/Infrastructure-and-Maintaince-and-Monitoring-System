using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class FacultyController : Controller
    {

        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string connectionString= ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HandleError]

        void ConnectionString()
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
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
        public ActionResult AcceptComplaint(int? complaintid)
        {
            if (complaintid == null)
            {
                string script = "<script>alert('ComplaintID is missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }


            string query = "UPDATE Tbl_Complain SET Status = 'In-progress' WHERE ComplainID = @ComplainID";


            using (SqlConnection con = new SqlConnection(connectionString))
            {

                con.Open();


                using (SqlCommand cmd = new SqlCommand(query, con))
                {

                    cmd.Parameters.AddWithValue("@ComplainID", complaintid);


                    int result = cmd.ExecuteNonQuery();


                    if (result > 0)
                    {
                        SendEmailToComplaintUsers((int)complaintid, "In-progress");
                        string script = "<script>alert('Complaint status updated to In-progress.');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");

                    }
                    else
                    {
                        string script = "<script>alert('Error updating complaint status. Please try again.');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");
                    }
                }
            }


            return RedirectToAction("Complaints");
        }
        public ActionResult CompleteComplaint(int? complaintid)
        {
            if (complaintid == null)
            {
                string script = "<script>alert('ComplaintID is missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }


            string query = "UPDATE Tbl_Complain SET Status = 'Completed' WHERE ComplainID = @ComplainID";


            using (SqlConnection con = new SqlConnection(connectionString))
            {

                con.Open();


                using (SqlCommand cmd = new SqlCommand(query, con))
                {

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
        public ActionResult AddRoom()
        {
            return View();//GG
        }
        public ActionResult RejectComplaint(int? complaintid)
        {
            if (complaintid == null)
            {
                string script = "<script>alert('ComplaintID is missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }


            string query = "UPDATE Tbl_Complain SET Status = 'Rejected' WHERE ComplainID = @ComplainID";


            using (SqlConnection con = new SqlConnection(connectionString))
            {

                con.Open();


                using (SqlCommand cmd = new SqlCommand(query, con))
                {

                    cmd.Parameters.AddWithValue("@ComplainID", complaintid);


                    int result = cmd.ExecuteNonQuery();


                    if (result > 0)
                    {
                        SendEmailToComplaintUsers((int)complaintid, "Rejected");
                        string script = "<script>alert('Complaint status updated to Rejected.');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");

                    }
                    else
                    {
                        string script = "<script>alert('Error updating complaint status. Please try again.');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");
                    }
                }
            }
            return RedirectToAction("Complaints");
        }
        //
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
        public ActionResult ManageRoom()
        {
            

            string query = "SELECT R.RoomID, RT.RoomtType AS RoomType, R.Wing, R.RoomNo FROM Tbl_Room R INNER JOIN Tbl_RoomType RT ON R.RoomType = RT.RoomTypeID;";
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
                            Wing = reader["Wing"].ToString()[0], // Assuming Wing is a char in the database
                            RoomNo = reader["RoomNo"].ToString()
                        };

                        rooms.Add(getrooms);
                    }
                }
            }
            return View(rooms);
        }
        //To be Edited
        //[HttpPost]
        //public ActionResult EditUser1(Room room)
        //{

        //    if (Session["UserID"] == null)
        //    {
        //        return RedirectToAction("Users");
        //    }
        //    int userId = Convert.ToInt32(Session["UserID"]);
        //    //String Query = "update Tbl_Users set Name='" + gd.Name + "', Email='" + gd.Email + "', PhoneNo='" + gd.PhoneNo + "', Gender='" + gd.Gender + "', LoginID='" + gd.LoginID + "', Role='" + gd.Role + "', Status='" + gd.Status + "' where UserID=" + userId;
        //    String Query="";
        //    ConnectionString();
        //    con.Open();
        //    com.Connection = con;
        //    com.CommandText = Query;
        //    try
        //    {
        //        int i = com.ExecuteNonQuery();
        //        if (i >= 1)
        //        {
        //            string script = "<script>alert('User data was updated Successfully');window.location='/Admin/Users'</script>";
        //            return Content(script, "text/html");
        //        }
        //        else
        //        {
        //            string script = "<script>alert('User data was updated Unsuccessfully');window.location='/Admin/Users'</script>";
        //            return Content(script, "text/html");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return RedirectToAction("Error?error=" + e + "", "Error");
        //    }



        //}
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
        public ActionResult FileComplaint()
        {
            Complaint cs = new Complaint()
            {

                ComplaintTypes = GetAllComplaintTypes(),
                Rooms = GetAllRooms()

            };
            return View(cs);
        }
        private List<ComplaintTypes> GetAllComplaintTypes()
        {
            List<ComplaintTypes> complaintTypes = new List<ComplaintTypes>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Tbl_ComplaintType where Status=1";

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
        private void RegisterComplaintUsers(int[] selectedUsers, int complaintId, string connectionString)
        {
            // Create connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open connection
                connection.Open();

                // Define query
                string query = "INSERT INTO Tbl_Complaint_User (UserID, ComplainID) VALUES (@UserID, @ComplainID);";

                // Create command
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@ComplainID", complaintId);
                    command.Parameters.AddWithValue("@UserID", Session["UserID"]);
                    command.ExecuteNonQuery();
                    // Add each selected user

                }

            }
        }
       
        [HttpPost]
        public ActionResult FileComplaint(Complaint cs)
        {
            // Your connection string


            // Register the complaint and get its ID
            int complaintId = RegisterComplaint(cs, connectionString);
            if (complaintId == 0)
            {
                string script = "<script>alert('Image is Empty');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }
            else if (complaintId == 1)
            {
                string script = "<script>alert('Complaint is Empty');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }
            // Register the associated users
            RegisterComplaintUsers(cs.SelectedUser, complaintId, connectionString);

            // Redirect to the StudentProfile page
            return RedirectToAction("Index");
        }
        private int RegisterComplaint(Complaint cs, string connectionString)
        {
            // Create connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open connection
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
                {
                    cs.Image = "No Image";
                }
                // Define query
                string query = "INSERT INTO [dbo].[Tbl_Complain]([Description],[ComplaintType],[ClassID],[Image],[Status],[Complain_Registration_Date]) VALUES (@Description, @ComplaintType, @ClassID,@Image, @Status,@ComplainRegisterDate); SELECT SCOPE_IDENTITY();";

                // Create command
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.Add("@Description", SqlDbType.VarChar).Value = cs.Description;
                    command.Parameters.Add("@ComplaintType", SqlDbType.Int).Value = cs.ComplaintType;
                    command.Parameters.Add("@Image", SqlDbType.VarChar).Value = cs.Image;
                    command.Parameters.Add("@Status", SqlDbType.VarChar).Value = "Pending"; // Default status
                    command.Parameters.Add("@ClassID", SqlDbType.Int).Value = cs.ClassID;
                    command.Parameters.Add("@ComplainRegisterDate", SqlDbType.Date).Value = DateTime.Today;
                    // Execute query and get the complaint ID
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
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
        public ActionResult ComplaintHistory()
        {
            String Query = "SELECT " +
    "C.ComplainID, " +
    "C.Description, " +
    "CT.ComplaintType, " +
    "C.Status,C.Image,c.Complain_Registration_Date,c.Complain_Completion_Date " +
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
                            Complain_Registration_Date = reader["Complain_Registration_Date"].ToString(),
                            
                            Complain_Completion_Date = reader["Complain_Completion_Date"].ToString(),
                            Users = GETCOMPLAINTUSERS((int)reader["ComplainID"]),
                            HasFeedback = CheckIfFeedbackExists((int)reader["ComplainID"], con)  // Check feedback status
                        };

                        complaints.Add(complaint);
                    }
                }
            }

            return View(complaints);
        }
        public ActionResult ViewFeedback(int? complaintID)
        {
            if(complaintID.HasValue)
            {
                List<Feedback> feedbackList = new List<Feedback>();

                // Assuming you have a Feedback model class
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

        public ActionResult Complaints()
        {
            if (Session["Role"] != null)
            {
                if (!Session["Role"].ToString().Contains("Faculty"))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            String Query;
            if (Session["Role"].ToString().Contains("Head"))
            {
                Query = "SELECT c.ComplainID, " +
          "ct.ComplaintType, " +
          "c.Description, " +
          "c.Image, " +
          "c.Status,c.Complain_Registration_Date,c.Complain_Completion_Date, " +

          "c.ClassID " +
       "FROM Tbl_Complain  c " +
       "INNER JOIN Tbl_ComplaintType ct ON c.ComplaintType = ct.Complaint_TypeID where ct.Complaint_TypeID!=1";
            }
            else if (Session["Role"].ToString().Contains("Lab"))
            {
                Query = "SELECT c.ComplainID, " +
           "ct.ComplaintType, " +
           "c.Description, " +
           "c.Image, " +
           "c.Status,c.Complain_Registration_Date,c.Complain_Completion_Date, " +

           "c.ClassID " +
        "FROM Tbl_Complain  c " +
        "INNER JOIN Tbl_ComplaintType ct ON c.ComplaintType = ct.Complaint_TypeID where ct.Complaint_TypeID=1";
            }
            else
            {
                Query = "SELECT c.ComplainID, " +
           "ct.ComplaintType, " +
           "c.Description, " +
           "c.Image, " +
           "c.Status,c.Complain_Registration_Date,c.Complain_Completion_Date, " +

           "c.ClassID " +
        "FROM Tbl_Complain  c " +
        "INNER JOIN Tbl_ComplaintType ct ON c.ComplaintType = ct.Complaint_TypeID";
            }
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
                         
                            Complain_Completion_Date = reader["Complain_Completion_Date"].ToString(),
                            Complain_Registration_Date = reader["Complain_Registration_Date"].ToString(),
                            HasFeedback =CheckIfFeedbackExists((int)reader["ComplainID"],con)
                        };

                        complaints.Add(complaint);
                    }
                }
            }

            return View(complaints);
        }
        public ActionResult Delete(int? complaintID)
        {
            if (complaintID.HasValue)
            {
                // Add code to delete the complaint with the given ID from the database
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Begin a transaction
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        // First, delete from Tbl_Complaint_User
                        string deleteUserQuery = "DELETE FROM Tbl_Complaint_User WHERE ComplainID = @ComplaintID";
                        using (SqlCommand deleteUserCmd = new SqlCommand(deleteUserQuery, con, transaction))
                        {
                            deleteUserCmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                            deleteUserCmd.ExecuteNonQuery();
                        }

                        // Then, delete from Tbl_Complain
                        string deleteComplaintQuery = "DELETE FROM Tbl_Complain WHERE ComplainID = @ComplaintID";
                        using (SqlCommand deleteComplaintCmd = new SqlCommand(deleteComplaintQuery, con, transaction))
                        {
                            deleteComplaintCmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                            deleteComplaintCmd.ExecuteNonQuery();
                        }

                        // Commit the transaction if both deletions were successful
                        transaction.Commit();
                        string script = "<script>alert('Complaint was Reverted Successfully');window.location='/Faculty/'</script>";
                        return Content(script, "text/html");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any error occurs
                        transaction.Rollback();
                        string script = "<script>alert('Complaint was not Reverted Successfully');window.location='/Faculty'</script>";
                        return Content(script, "text/html");
                    }
                }



            }
            else
            {
                string script = "<script>alert('ComplaintID is missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }
        }

        public ActionResult GiveFeedback(int? Complaintid)
        {
            if (Complaintid.HasValue)
            {
                Session["ComplaintID"] = Complaintid;
                return View();
            }
            else
            {
                string script = "<script>alert('ComplaintID is Missing');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
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
        [HttpPost]
        public ActionResult GiveFeedback(Feedback feed)
        {
            String Query = "Insert into Tbl_Feedback(ComplainID,Description,Rating,Status) Values(@ComplainID,@Description,@Rating,@Status)";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(Query, con))
            {
                con.Open();
                com.Parameters.AddWithValue("@ComplainID", SqlDbType.Int).Value = Session["ComplaintID"];
                com.Parameters.AddWithValue("@Description", SqlDbType.VarChar).Value = feed.Description;
                com.Parameters.AddWithValue("@Rating", SqlDbType.Int).Value = feed.Rating;
                com.Parameters.AddWithValue("@Status", SqlDbType.Bit).Value = true;

                com.ExecuteNonQuery();
                string script = "<script>alert('Feedback Successfull');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }
        }
        private void SendEmailToComplaintUsers(int complaintId, String Status)
        {
            // Fetch user emails for the given complaint ID
            var users = GETCOMPLAINTUSERS(complaintId);
            var userEmails = users.Select(u => u.Email).ToList();

            foreach (var email in userEmails)
            {
                using (var client = new SmtpClient("smtp.gmail.com", 587)) // Replace with your SMTP server details
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
                        // Handle the exception or log it
                        Console.WriteLine("Exception caught in SendEmailToComplaintUsers(): {0}", ex.ToString());
                    }
                }
            }
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
    }
}