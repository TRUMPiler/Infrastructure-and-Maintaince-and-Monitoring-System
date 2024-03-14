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
        string otp="";
        
        string Result = "";
        [HandleError]

        public string SendMail(String To)
        {
            es.Email = "dashtaxigg@gmail.com";
            es.To = To;
            string randomNum = GenerateRandomNumber(6);
            Session["otp"] = randomNum;
            using (MailMessage mm = new MailMessage(es.Email, es.To))
            {
                

                es.Body = "Your Otp is " + randomNum;
                
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
            return randomNum;


        }
        void ConnectionString()
        {
            con.ConnectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        }
        public ActionResult Register()
        {
            return View();
        }
       

        public ActionResult Index()
        {
            return View();
        }
        

        [HttpPost]
        public ActionResult VerifyOtp(string otps)
        {
            Result = Session["otp"]+" "+otps;

            otp = Session["otp"].ToString();
                if (this.otp.Equals(otps))
                {
                    return RedirectToAction("ChangePass");
                }
                
                
            return View(model: "Coundn't Verify Otp"+ (this.otp.Equals(otps))+ Result);

        }
        [HttpPost]
        public ActionResult Login(GetData gd)
        {
            String Query = "select * from Tbl_Users where LoginID='"+gd.LoginID+"' and Password ='"+gd.Password+"'";
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            dr = com.ExecuteReader();
            if (dr.HasRows)
            {
                Session["LoginID"] = gd.LoginID;
                Session["UserVerified"] = "true";
                return View("Index");
            }
            else
            {
                return View("Login",model:"Invalid Crendiatls");
            }
            
            //}
        }

        
        public ActionResult ChangePass()
        {
            if(Session["LoginID"]==null|| Session["LoginID"].Equals(""))
            {
                return RedirectToAction("Login");
                
            }
            return View();
        }
        [HttpPost]
        public ActionResult ChangePass(string Newpass,string oldPass)
        {
            if (Newpass.Equals(oldPass))
            {
                String Query = "update Tbl_User Password='" + oldPass + "' where LoginID='" + Session["LoginID"] + "'";
                return View("Success");
            }
            else
            {
                return View(model:"Password Does not Match");
            }
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
        public ActionResult ForgetPass()
        {
            if (Session["LoginID"] != null||!Session["LoginID"].Equals(""))
            {
                return RedirectToAction("ChangePass");
            }
            return View();
        }
        [HttpPost]
        public ActionResult VerifyLoginID(GetData gs)
        {
            String Query = "Select Email from Tbl_Users where LoginID='"+gs.LoginID+"'";
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            dr = com.ExecuteReader();
            if(dr.HasRows)
            {
                while (dr.Read())
                {
                    Session["LoginID"] = gs.LoginID;
                    String Email= dr[0].ToString();
                    SendMail(Email); 
                    
                }
                
                return View("VerifyOtp");
                
            }
            else
            {
                return View("ForgetPass", model:Result) ;
            }
        }
        public ActionResult Login()
        {
            if (Session["LoginID"] != null || !Session.Equals(""))
            {
                return RedirectToAction("Index");
            }
            return View();
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