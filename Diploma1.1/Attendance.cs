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
    
    public partial class Attendance
    {
        public int AttendanceID { get; set; }
        public Nullable<int> StudentID { get; set; }
        public Nullable<int> ScheduleID { get; set; }
        public Nullable<int> StatusAttendanceID { get; set; }
    
        public virtual Schedule Schedule { get; set; }
        public virtual StatusAttendance StatusAttendance { get; set; }
        public virtual Student Student { get; set; }
    }
}
