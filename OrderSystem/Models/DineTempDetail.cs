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
    
    public partial class DineTempDetail
    {
        public int AutoID { get; set; }
        public string DisherID { get; set; }
        public Nullable<int> DisherNum { get; set; }
        public Nullable<decimal> DisherPrice { get; set; }
        public Nullable<double> SalesDiscount { get; set; }
        public string Note { get; set; }
    }
}
