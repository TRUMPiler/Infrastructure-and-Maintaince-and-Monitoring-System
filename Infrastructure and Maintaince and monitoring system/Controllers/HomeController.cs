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
        private readonly string connectionString = "Data Source=DESKTOP-T7RS5U7\\SQLEXPRESS;Initial Catalog=IMMS;Integrated Security=True";

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (IsValidUser(username, password))
            {
                
                return RedirectToAction("Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }
        }

        
        public ActionResult ChangePass()
        {
            return View();
        }

        private bool IsValidUser(string username, string password)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password", con))
                {           
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }

}