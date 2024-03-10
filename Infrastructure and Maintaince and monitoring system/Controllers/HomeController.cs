using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.Helpers;
namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class HomeController : Controller
    {

        EmailSending es = new EmailSending();
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        String otp;
        [HandleError]

        public void SendMail(String To)
        {
            es.Email = "dashtaxigg@gmail.com";
            es.To = To;
            using (MailMessage mm = new MailMessage(es.Email, es.To))
            {
                string randomNum = GenerateRandomNumber(6);
                es.Body = "Your Otp is " + randomNum;
                otp = randomNum;
                es.Password = "cgqgsvvqvjwswyyq";

                es.Subject = "Your One Time Password";
                mm.Subject = es.Subject;
                mm.Body = es.Body;
                mm.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential cred = new NetworkCredential(es.Email, es.Password);
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = cred;
                    smtp.Port = 587;
                    smtp.Send(mm);
                   
                }
            }


        }
        void ConnectionString()
        {
            con.ConnectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        }
        [HttpPost]
        public ActionResult VerifyUser(GetData gs)
        {
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = "select Email from Tbl_Users where LoginID='" + gs.LoginID + "'";
            dr = com.ExecuteReader();
            if(!dr.HasRows)
            {
                return View("ChangePass",model:"Couldn't find your id");
            }
            
                if (dr.Read())
                {


                    SendMail(dr[0].ToString());
                    con.Close();
                    
                }
                return View("VerifyOtp");
            
            
        }
        public ActionResult Register()
        {
            return View();
        }
       

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyOtp(int otp)
        {
            if (Convert.ToInt32(this.otp)==otp)
            {
                return View("Login");
            }
            else
            {
                return View(model:"Coundn't Verify Otp");
            }
        }
        public ActionResult Login(GetData gd)
        {
            //if (IsValidUser(username, password))
            //{

            //    return RedirectToAction("Dashboard");
            //}
            //else
            //{
            //    ModelState.AddModelError("", "Invalid username or password.");
            //    return View();
            Session["LoginID"] = gd.LoginID;
            return View();
            //}
        }

        
        public ActionResult ChangePass()
        {
            if(Session["LoginID"]==null|| Session["LoginID"] == "")
            {
                return RedirectToAction("Login");
                
            }
            return View("");
        }
        [HttpPost]
        public ActionResult ChangePass(String newPassword,String oldPassword)
        {
            return View();

        }

        private bool IsValidUser(string username, string password)
        {
            //using (var con = new SqlConnection(connectionString))
            //{
            //    con.Open();
            //    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password", con))
            //    {           
            //        cmd.Parameters.AddWithValue("@Username", username);
            //        cmd.Parameters.AddWithValue("@Password", password);
            //        int count = (int)cmd.ExecuteScalar();
            //        return count > 0;
            //    }
            //}
            return
                 true;
        }
        

        public string GenerateRandomNumber(int numberOfDigits)
        {
            Random rand = new Random();
            int minValue = (int)Math.Pow(10, numberOfDigits - 1);
            int maxValue = (int)Math.Pow(10, numberOfDigits) - 1;
            return rand.Next(minValue, maxValue).ToString();
        }
    }

}