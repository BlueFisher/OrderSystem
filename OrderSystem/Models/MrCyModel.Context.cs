﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace OrderSystem.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MrCyContext : DbContext
    {
        public MrCyContext()
            : base("name=MrCyContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<BaseInfo> BaseInfo { get; set; }
        public virtual DbSet<BatchNo> BatchNo { get; set; }
        public virtual DbSet<ClerkGroup> ClerkGroup { get; set; }
        public virtual DbSet<ClerkInfo> ClerkInfo { get; set; }
        public virtual DbSet<ClientInfo> ClientInfo { get; set; }
        public virtual DbSet<DepartmentClass> DepartmentClass { get; set; }
        public virtual DbSet<DepartmentInfo> DepartmentInfo { get; set; }
        public virtual DbSet<DepartmentPrinter> DepartmentPrinter { get; set; }
        public virtual DbSet<DeskInfo> DeskInfo { get; set; }
        public virtual DbSet<DineDetail> DineDetail { get; set; }
        public virtual DbSet<DineDetailHistory> DineDetailHistory { get; set; }
        public virtual DbSet<DineInfo> DineInfo { get; set; }
        public virtual DbSet<DineInfoHistory> DineInfoHistory { get; set; }
        public virtual DbSet<DineTempDetail> DineTempDetail { get; set; }
        public virtual DbSet<DineTempInfo> DineTempInfo { get; set; }
        public virtual DbSet<MenuClass> MenuClass { get; set; }
        public virtual DbSet<MenuDetail> MenuDetail { get; set; }
        public virtual DbSet<MenuSubClass> MenuSubClass { get; set; }
        public virtual DbSet<PayKind> PayKind { get; set; }
        public virtual DbSet<PrinterInfo> PrinterInfo { get; set; }
        public virtual DbSet<RoomInfo> RoomInfo { get; set; }
        public virtual DbSet<AreaInfo> AreaInfo { get; set; }
        public virtual DbSet<Note> Note { get; set; }
        public virtual DbSet<PosInfo> PosInfo { get; set; }
        public virtual DbSet<Waiter> Waiter { get; set; }
    }
}