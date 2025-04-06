using Diploma1._1.View.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Diploma1._1.View.CRUD
{
    public partial class EditAttendanceWindow : Window
    {
        //private AttendanceItem attendanceItem;

        public EditAttendanceWindow()
        {
            InitializeComponent();
            //attendanceItem = item;

            //// Заполнение полей окна данными из item
            //StudentNameTextBlock.Text = item.StudentName;
            //DateTextBlock.Text = item.Date.ToString("dd.MM.yyyy");
            //TimeTextBlock.Text = item.Time;

            //// Заполнение комбобокса статусов
            //using (var context = new Dimploma1Entities())
            //{
            //    StatusComboBox.ItemsSource = context.StatusAttendance.ToList();
            //    StatusComboBox.DisplayMemberPath = "StatusAttendanceName";
            //    StatusComboBox.SelectedValuePath = "StatusAttendanceID";
            //    StatusComboBox.SelectedValue = item.StatusAttendanceID;
            //}
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //if (StatusComboBox.SelectedValue == null)
            //{
            //    MessageBox.Show("Выберите статус посещаемости", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            //using (var context = new Dimploma1Entities())
            //{
            //    var attendance = context.Attendance
            //        .FirstOrDefault(a => a.ScheduleID == attendanceItem.ScheduleID &&
            //                          a.StudentID == attendanceItem.StudentID);

            //    if (attendance == null)
            //    {
            //        attendance = new Attendance
            //        {
            //            ScheduleID = attendanceItem.ScheduleID,
            //            StudentID = attendanceItem.StudentID,
            //            StatusAttendanceID = (int)StatusComboBox.SelectedValue
            //        };
            //        context.Attendance.Add(attendance);
            //    }
            //    else
            //    {
            //        attendance.StatusAttendanceID = (int)StatusComboBox.SelectedValue;
            //    }

            //    context.SaveChanges();
            //    DialogResult = true;
            //    Close();
            //}
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

