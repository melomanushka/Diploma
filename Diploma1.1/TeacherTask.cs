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
    
    public partial class TeacherTask
    {
        public int TeacherTaskID { get; set; }
        public string Task { get; set; }
        public Nullable<System.DateTime> DataTask { get; set; }
        public Nullable<int> StudentID { get; set; }
        public Nullable<int> TeacherID { get; set; }
        public Nullable<bool> StatusTask { get; set; }
    
        public virtual Employee Employee { get; set; }
        public virtual Student Student { get; set; }
    }
}
