﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public string RoomNo { get; set; }
        public string RoomType { get; set; } 
        public char Wing { get; set; }
        public bool Status { get; set; }
        public List<RoomType> roomTypes { get; set; }
    }

}