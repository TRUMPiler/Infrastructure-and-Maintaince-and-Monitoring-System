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
    
    public partial class Tbl_Feedback
    {
        public int FeedbackID { get; set; }
        public int ComplainID { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
    
        public virtual Tbl_Complain Tbl_Complain { get; set; }
    }
}
