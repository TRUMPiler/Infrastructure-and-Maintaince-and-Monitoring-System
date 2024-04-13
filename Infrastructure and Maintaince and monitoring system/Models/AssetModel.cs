using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class AssetModel
    {
        public int AssetID { get; set; }
        public string AssetName { get; set; }
        public int Working { get; set; }
        public int NonWorking { get; set; }
    }

    public class RoomModel
    {
        public int RoomID { get; set; }
        public string RoomNo { get; set; }
    }

    public class RoomAssetModel
    {
        public int AssetID { get; set; }
        public int RoomID { get; set; }
        public int Working { get; set; }
        public int NonWorking { get; set; }
        public bool Status { get; set; } = true;
    }

}
