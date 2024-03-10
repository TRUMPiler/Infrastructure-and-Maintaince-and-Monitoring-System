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
        // GET: Home
        SmtpClient smtp = new SmtpClient();
        
        
        MailMessage mm = new MailMessage();
        SqlConnection con = new SqlConnection();
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        void ConnectionString()
        {
            con.ConnectionString = "Data Source=DESKTOP-T7RS5U7/SQLEXPRESS;Initial Catalog=IMMS;Integrated Security=True";
        }
        public ActionResult Login()
        {
            return View();
           
        }
        public ActionResult Verify(GetData gd)
        {
            ConnectionString();
            con.Open();
            com.Connection = con;
            com.CommandText = "select * from Users where Username='" + gd.Username + "' and Password='" + gd.Password + "'";
            dr = com.ExecuteReader();
            if (dr.Read())
            {

                con.Close();

                return View("Success");
            }
            else
            {
                con.Close();

                return View("Error");
            }
        }
    }
}