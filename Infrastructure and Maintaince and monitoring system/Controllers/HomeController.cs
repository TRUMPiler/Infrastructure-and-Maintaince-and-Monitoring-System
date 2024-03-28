using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Infrastructure_and_Maintaince_and_monitoring_system.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web.Helpers;
namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class HomeController : Controller
    {

        HttpCookie cookieLogin, cookieName, cookieRole;
        EmailSending es = new EmailSending();
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        string otp="";
        
        string Result = "";
        [HandleError]

        public string SendMail(String To,String subject,bool verification)
        {
            es.Email = "dashtaxigg@gmail.com";
            es.To = To;
            string randomNum = GenerateRandomNumber(6);
            Session["otp"] = randomNum;
            using (MailMessage mm = new MailMessage(es.Email, es.To))
            {
                

                if(verification)
                {
                    es.Body = "Your Otp is " + randomNum;
                    es.Subject = "verification Email";
                }
                else
                {
                    es.Body = "Your Password for User ID:"+Session["LoginID"]+" is "+subject;
                    es.Subject = "Your Password ";
                }
                    es.Password = "cgqgsvvqvjwswyyq";


                
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
        void ConnectionString()
        {
            con.ConnectionString = "data source=ASUSTUFGAMING\\SQLEXPRESS; database=IMMS; integrated security=SSPI";
        }
            
    
   

        public ActionResult Index()
        {
            cookieLogin = Request.Cookies["Login"];
            cookieName = Request.Cookies["Name"];
            cookieRole = Request.Cookies["Role"];
            if (cookieLogin!=null)
            {
                Session["LoginID"] = cookieLogin.Value;
                Session["Name"] = cookieName.Value;
                Session["Role"] = cookieRole.Value;
            }
            if (Session["Role"]!=null)
            {
                if (Session["Role"].ToString().Equals("admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                if (Session["Role"].ToString().Equals("Student"))
                {
                    return RedirectToAction("StudentProfile", "Student");
                }
                else
                {
                    if (Session["Role"].ToString().Equals("Faculty"))
                    {
                        return RedirectToAction("Profile", "Faculty");
                    }
                    
                }

            }
            return RedirectToAction("Login");
        }
        

        [HttpPost]
        public ActionResult VerifyOtp(string otps)
        {
            Result = Session["otp"]+" "+otps;

            otp = Session["otp"].ToString();
                if (this.otp.Equals(otps))
                {
                SendMail(Session["Email"].ToString(), Session["Password"].ToString(), false);
                Session["Email"] = null;
                Session["Password"] = null;
                Session["LoginID"] = null;
                return View("Success",model:"Your password was sent to your email");
                }
                
                
            return View(model: "Coundn't Verify Otp"+ (this.otp.Equals(otps))+ Result);

        }
        [HttpPost]
        public ActionResult Login(GetData gd)
        {
            
            
            if(gd.LoginID.Equals("admin"))
            {
                if(gd.Password.Equals("admin"))
                {
                    
                    
                    cookieRole = new HttpCookie("Role", "admin");
                    cookieLogin = new HttpCookie("Login", gd.LoginID);
                    cookieName = new HttpCookie("Name", "Varsha Patel");
                    cookieRole.Expires = DateTime.Now.AddDays(2);
                    cookieLogin.Expires = DateTime.Now.AddDays(2);
                    cookieName.Expires = DateTime.Now.AddDays(2);
                    Response.Cookies.Add(cookieLogin);
                    Response.Cookies.Add(cookieName);
                    Session["Role"] = "admin";
                    Session["LoginID"] = gd.LoginID;
                    Session["Name"] = "Varsha Parel";
                    Response.Cookies.Add(cookieRole);
                    return RedirectToAction("Index", "Admin");
                }    
            }
            String Query = "select LoginID,Email,Role,Name from Tbl_Users where LoginID='"+gd.LoginID+"' and Password ='"+gd.Password+"'";
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = Query;
            dr = com.ExecuteReader();
            if (dr.HasRows)
            {
               while(dr.Read())
                {
                  
                    gd.Email = dr[1].ToString();
                    gd.Role = dr[2].ToString();
                    gd.Name = dr[3].ToString();
                    Session["Role"] = gd.Role;
                    Session["Email"] = gd.Email;
                    Session["LoginID"] = gd.LoginID;
                    Session["Name"] = gd.Name;
                    cookieRole = new HttpCookie("Role", gd.Role);
                    cookieLogin = new HttpCookie("Login", gd.LoginID);
                    cookieName = new HttpCookie("Name", gd.Name);
                    cookieRole.Expires = DateTime.Now.AddDays(2);
                    cookieLogin.Expires = DateTime.Now.AddDays(2);
                    cookieName.Expires = DateTime.Now.AddDays(2);
                    Response.Cookies.Add(cookieLogin);
                    Response.Cookies.Add(cookieName);
                    Response.Cookies.Add(cookieRole);

                }
                Session["UserVerified"] = "true";
                if(gd.Role.Contains("Student"))
                {
                    
                    return RedirectToAction("StudentProfile","Student");

                }
                else if (gd.Role.Contains("Faculty"))
                {

                    return RedirectToAction("Profile", "Faculty");

                }

                return View();
            }
            else
            {
                return View("Login",model:"Invalid Crendiatls");
            }
        }
        public ActionResult ForgetPass()
        {
            if (Session["LoginID"] != null)
            {
                return RedirectToAction("ChangePass");
            }
            return View();
        }
        public ActionResult Success()
        {
            return View(model:"nothing to show");
        }
        [HttpPost]
        public ActionResult VerifyLoginID(GetData gs)
        {
            String Query = "Select Email,Password from Tbl_Users where LoginID='"+gs.LoginID+"'";
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
                    Session["Password"] = dr[1].ToString();
                    Session["Email"] = dr[0].ToString();
                    String Email= dr[0].ToString();
                    
                    SendMail(Email,"",true); 
                    
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
            if (Session["LoginID"] != null)
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