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
    
    public partial class MenuSubClass
    {
        public int AutoId { get; set; }
        public string SubClassId { get; set; }
        public string SubClassName { get; set; }
        public string ClassId { get; set; }
        public string SubClassRemark { get; set; }
        public Nullable<bool> Usable { get; set; }
        public string Creator { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string Updator { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public string Deletor { get; set; }
        public Nullable<System.DateTime> DeleteDate { get; set; }
    }
}
