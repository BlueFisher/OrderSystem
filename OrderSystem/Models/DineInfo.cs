//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class DineInfo
    {
        public int AutoID { get; set; }
        public string CheckID { get; set; }
        public string ClerkID { get; set; }
        public string WaiterID { get; set; }
        public string ClientID { get; set; }
        public Nullable<int> ClientNum { get; set; }
        public string DeskID { get; set; }
        public string Roomid { get; set; }
        public Nullable<System.DateTime> BeginTime { get; set; }
        public Nullable<bool> Status { get; set; }
        public string PayKind { get; set; }
        public Nullable<decimal> Subtotal { get; set; }
        public string PaidAccount { get; set; }
        public Nullable<bool> IsRefund { get; set; }
    }
}
