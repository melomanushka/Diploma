using Diploma1._1.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Visit.xaml
    /// </summary>
    public partial class Visit : Page
    {
        private ObservableCollection<VisitItem> visitItems;
        private Dimploma1Entities context;

        public Visit()
        {
            InitializeComponent();
            context = new Dimploma1Entities();
            visitItems = new ObservableCollection<VisitItem>();
            VisitDataGrid.ItemsSource = visitItems;

            InitializeComboBoxes();
            DatePicker.SelectedDate = DateTime.Today;
        }

        private void InitializeComboBoxes()
        {
            // Заполняем комбобокс групп
            var groups = context.Group.ToList();
            GroupComboBox.ItemsSource = groups;

            // Заполняем комбобокс курсов
            var courses = context.Course.ToList();
            CourseComboBox.ItemsSource = courses;
        }

        private void LoadVisitData()
        {
            if (GroupComboBox.SelectedValue == null || CourseComboBox.SelectedValue == null ||
                DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля фильтрации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int groupId = (int)GroupComboBox.SelectedValue;
            int courseId = (int)CourseComboBox.SelectedValue;
            DateTime selectedDate = DatePicker.SelectedDate.Value;

            // Получаем расписание для выбранной группы, курса и даты
            var schedules = context.Schedule
                .Where(s => s.GroupID == groupId && 
                           s.CourseID == courseId &&
                           s.ClassDate.Value.Date == selectedDate.Date)
                .OrderBy(s => s.Time.TimeStart)
                .ToList();

            // Получаем студентов группы
            var students = context.GroupStudent
                .Where(gs => gs.GroupID == groupId)
                .Select(gs => gs.Student)
                .ToList();

            visitItems.Clear();

            foreach (var schedule in schedules)
            {
                foreach (var student in students)
                {
                    // Проверяем, есть ли уже запись о посещаемости
                    var attendance = context.Attendance
                        .FirstOrDefault(a => a.ScheduleID == schedule.ScheduleID && 
                                           a.StudentID == student.StudentID);

                    var timeSlot = context.Time.FirstOrDefault(t => t.TimeID == schedule.TimeID);
                    string timeString = timeSlot != null ? 
                        $"{timeSlot.TimeStart.Value.Hours:D2}:{timeSlot.TimeStart.Value.Minutes:D2} - {timeSlot.TimeEnd.Value.Hours:D2}:{timeSlot.TimeEnd.Value.Minutes:D2}" : 
                        "";

                    visitItems.Add(new VisitItem
                    {
                        ScheduleID = schedule.ScheduleID,
                        StudentID = student.StudentID,
                        StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                        Time = timeString,
                        Status = attendance?.StatusAttendance?.StatusAttendanceName ?? "Не отмечено",
                        StatusAttendanceID = attendance?.StatusAttendanceID
                    });
                }
            }
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadVisitData();
        }

        private void UpdateAttendanceStatus(VisitItem item, string statusName)
        {
            // Получаем ID статуса посещаемости по его названию
            var statusAttendance = context.StatusAttendance
                .FirstOrDefault(sa => sa.StatusAttendanceName == statusName);

            if (statusAttendance == null)
            {
                MessageBox.Show("Статус посещаемости не найден в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var attendance = context.Attendance
                .FirstOrDefault(a => a.ScheduleID == item.ScheduleID &&
                                   a.StudentID == item.StudentID);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    ScheduleID = item.ScheduleID,
                    StudentID = item.StudentID,
                    StatusAttendanceID = statusAttendance.StatusAttendanceID
                };
                context.Attendance.Add(attendance);
            }
            else
            {
                attendance.StatusAttendanceID = statusAttendance.StatusAttendanceID;
            }

            context.SaveChanges();
            LoadVisitData();
        }

        private void MarkPresent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is VisitItem item)
            {
                UpdateAttendanceStatus(item, "Присутствует");
            }
        }

        private void MarkAbsent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is VisitItem item)
            {
                UpdateAttendanceStatus(item, "Отсутствует");
            }
        }
    }

    public class VisitItem
    {
        public int ScheduleID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public int? StatusAttendanceID { get; set; }
    }
}
