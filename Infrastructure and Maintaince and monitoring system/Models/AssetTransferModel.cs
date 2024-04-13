using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{ 
public class AssetTransferModel
{
        [Display(Name = "Transfer ID")]
        public int ATid { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime TransferDate { get; set; }

        [Display(Name = "From Room ID")]
        public int From_RoomID { get; set; }

        [Display(Name = "To Room ID")]
        public int To_RoomID { get; set; }

        [Display(Name = "Asset ID")]
        public int AssetID { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
    }
}