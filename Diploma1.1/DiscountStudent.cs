//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Diploma1._1
{
    using System;
    using System.Collections.Generic;
    
    public partial class DiscountStudent
    {
        public int DiscountStudentID { get; set; }
        public int DiscountID { get; set; }
        public int ClientID { get; set; }
        public Nullable<System.DateTime> DateOfDiscountApplication { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual Discount Discount { get; set; }
    }
}
