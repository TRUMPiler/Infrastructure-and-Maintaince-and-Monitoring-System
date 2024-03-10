using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class EmailSending
    {
        public String To { get; set; }
        public String Body { get; set; }
     
        public String Email { get; set; }
        public String Subject { get; set; }
        public String Password { get; set; }
    }
}