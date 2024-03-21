using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Error(String e)
        {

            return View(model:e); 
        }
    }
}