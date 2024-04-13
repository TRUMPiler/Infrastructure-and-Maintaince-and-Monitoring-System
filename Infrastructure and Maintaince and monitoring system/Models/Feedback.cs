using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int ComplainID { get; set; }
        public String Description { get; set; }
        public int Rating { get; set; }
        public bool Status { get; set; }
    }
}