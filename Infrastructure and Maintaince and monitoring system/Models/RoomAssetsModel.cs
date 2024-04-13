using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class RoomAssetsModel
    {
        public int RAid { get; set; }

        [Display(Name = "Room")]
        public int RoomID { get; set; }

        [Display(Name = "Asset")]
        public int AssetID { get; set; }

        [Display(Name = "Working")]
        public int Working { get; set; }

        [Display(Name = "NonWorking")]
        public int NonWorking { get; set; }
    }
}