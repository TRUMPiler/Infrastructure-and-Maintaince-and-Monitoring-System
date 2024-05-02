using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.IO;
using System.Data;
using System.Configuration;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class StudentController : Controller
    {
        String connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        
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
        [HttpPost]
        public ActionResult GiveFeedback(Feedback feed)
        {
            String Query = "Insert into Tbl_Feedback(ComplainID,Description,Rating,Status) Values(@ComplainID,@Description,@Rating,@Status)";
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand com = new SqlCommand(Query, con))
            {
                con.Open();
                com.Parameters.AddWithValue("@ComplainID",SqlDbType.Int).Value= Session["ComplaintID"];
                com.Parameters.AddWithValue("@Description", SqlDbType.VarChar).Value= feed.Description;
                com.Parameters.AddWithValue("@Rating", SqlDbType.Int).Value=feed.Rating;
                com.Parameters.AddWithValue("@Status", SqlDbType.Bit).Value = true;
                com.ExecuteNonQuery();
                string script = "<script>alert('Feedback Successfull');window.location='/Faculty/'</script>";
                return Content(script, "text/html");
            }
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
        public ActionResult ComplaintHistory()
        {
            String Query = "SELECT "+
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
    
    "CU.UserID =(SELECT UserID FROM Tbl_Users WHERE LoginID ='"+Session["LoginID"]+"');";
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
                            Complain_Registration_Date=reader["Complain_Registration_Date"].ToString(),
                            Complain_Completion_Date = reader["Complain_Completion_Date"].ToString(),
                            HasFeedback = CheckIfFeedbackExists((int)reader["ComplainID"], con)  // Check feedback status
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
                        string script = "<script>alert('Complaint was Reverted Successfully');window.location='/Student/StudentProfile'</script>";
                        return Content(script, "text/html");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any error occurs
                        transaction.Rollback();
                        string script = "<script>alert('Complaint was not Reverted Successfully');window.location='/Student/StudentProfile'</script>";
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
                Rooms=GetAllRooms(),
                Users=GetAllUsers()
            };
            return View(cs);
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
        public List<GetData> GetAllUsers()
        {
            List<GetData> ls=new List<GetData>();
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
"INNER JOIN Tbl_Students s ON u.UserID = s.UserID " +
"WHERE s.Semster = (SELECT Semster FROM Tbl_Students WHERE UserId = (SELECT UserID FROM Tbl_Users WHERE LoginID = '" + Session["LoginID"] + "')) AND u.Status = 1 AND u.UserID != (SELECT UserID FROM Tbl_Users WHERE LoginID = '" + Session["LoginID"] + "')";

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
        public ActionResult GiveFeedback(int? Complaintid)
        {
            if(Complaintid.HasValue)
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
        [HttpPost]
        public ActionResult FileComplaint(Complaint cs)
        {
            // Your connection string
            

            // Register the complaint and get its ID
            int complaintId = RegisterComplaint(cs, connectionString);
            if(complaintId==0)
            {
                string script = "<script>alert('Image is Empty');window.location='/Admin/Users'</script>";
                return Content(script, "text/html");
            }
            else if(complaintId==1)
            {
                string script = "<script>alert('Complaint is Empty');window.location='/Admin/Users'</script>";
                return Content(script, "text/html");
            }
            // Register the associated users
            RegisterComplaintUsers(cs.SelectedUser, complaintId, connectionString);

            // Redirect to the StudentProfile page
            return RedirectToAction("StudentProfile");
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
                else
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
    }
    
}