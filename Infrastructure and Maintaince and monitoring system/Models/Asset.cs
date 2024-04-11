
using System.ComponentModel.DataAnnotations;

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    public class Asset
    {
        [Key]
        public int AssetID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        public bool Status { get; set; } = true;
    }
}