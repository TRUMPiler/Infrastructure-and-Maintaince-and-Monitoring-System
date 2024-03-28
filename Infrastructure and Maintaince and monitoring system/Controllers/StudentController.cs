using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class StudentController : Controller
    {
        // GET: Student
        public ActionResult StudentProfile()
        {
            return View();
        }
    }
}