using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class AdminPanel
    {
        public int count { get; set; }
        public int completed { get; set; }
        public List<String> todo { get; set; }
        public float avg { get; set; }
        public int TotalStudents { get; set; }
    }
}