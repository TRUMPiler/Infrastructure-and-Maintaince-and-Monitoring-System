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
        public ActionResult VerifyUser(GetData gs)
        {
            return View("Naishal");
        }
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
    
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(String GG)
        {
            //if (IsValidUser(username, password))
            //{

            //    return RedirectToAction("Dashboard");
            //}
            //else
            //{
            //    ModelState.AddModelError("", "Invalid username or password.");
            //    return View();
            return View();
            //}
        }

        
        public ActionResult ChangePass()
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
    }

}