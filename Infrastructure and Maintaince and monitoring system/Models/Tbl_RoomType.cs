//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Infrastructure_and_Maintaince_and_monitoring_system.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tbl_RoomType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_RoomType()
        {
            this.Tbl_Room = new HashSet<Tbl_Room>();
        }
    
        public int RoomTypeID { get; set; }
        public string RoomtType { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_Room> Tbl_Room { get; set; }
    }
}