﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class IMMSEntities : DbContext
    {
        public IMMSEntities()
            : base("name=IMMSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Tbl_Asset> Tbl_Asset { get; set; }
        public virtual DbSet<Tbl_Asset_Transfer> Tbl_Asset_Transfer { get; set; }
        public virtual DbSet<Tbl_Complain> Tbl_Complain { get; set; }
        public virtual DbSet<Tbl_Complaint_User> Tbl_Complaint_User { get; set; }
        public virtual DbSet<Tbl_ComplaintType> Tbl_ComplaintType { get; set; }
        public virtual DbSet<Tbl_Feedback> Tbl_Feedback { get; set; }
        public virtual DbSet<Tbl_Room> Tbl_Room { get; set; }
        public virtual DbSet<Tbl_Room_Asset> Tbl_Room_Asset { get; set; }
        public virtual DbSet<Tbl_RoomType> Tbl_RoomType { get; set; }
        public virtual DbSet<Tbl_Students> Tbl_Students { get; set; }
        public virtual DbSet<Tbl_Users> Tbl_Users { get; set; }
    }
}
