using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class Complaint
    {
        public int ComplaintID { get; set; }
        public String ComplaintType { get; set; }
        public String Description { get; set; }
        public String Image { get; set; }
        public String Status { get; set; }
        public String User { get; set; }
        public String ClassID { get; set; }

    }
}