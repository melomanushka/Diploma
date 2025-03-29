using Diploma1._1.View.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma1._1.Model
{
    internal class ScheduleTable
    {
        int ScheduleID { get; set; }
        DateTime ClassDate { get; set; }
        public List<StudentTable> Students { get; set; }
        int GroupID {  get; set; }
        string GroupName { get; set; }
        int TimeID { get; set; }
        int CabinetID { get; set; }
        string CabinetName { get; set; }
        int CourseID { get; set; }
        string CourseName { get; set; }
    }
}
