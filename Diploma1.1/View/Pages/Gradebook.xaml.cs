using Diploma1._1.Model;
using Diploma1._1.View.CRUD;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Gradebook.xaml
    /// </summary>
    public partial class Gradebook : Page
    {
        private ObservableCollection<AttendanceItem> attendanceItems;
        private Dimploma1Entities context;

        public Gradebook()
        {
            InitializeComponent();
            context = new Dimploma1Entities();
            attendanceItems = new ObservableCollection<AttendanceItem>();
            AttendanceDataGrid.ItemsSource = attendanceItems;

            InitializeComboBoxes();
            SetDefaultDates();
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

        private void SetDefaultDates()
        {
            // Устанавливаем текущую дату в качестве начальной
            StartDatePicker.SelectedDate = DateTime.Today;
            // Устанавливаем дату через неделю в качестве конечной
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void LoadAttendanceData()
        {
            if (GroupComboBox.SelectedValue == null || CourseComboBox.SelectedValue == null ||
                StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                return;

            int groupId = (int)GroupComboBox.SelectedValue;
            int courseId = (int)CourseComboBox.SelectedValue;
            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;

            // Загружаем расписание для выбранной группы и курса
            var schedule = context.Schedule
                .Where(s => s.GroupID == groupId && s.CourseID == courseId &&
                           s.ClassDate >= startDate && s.ClassDate <= endDate)
                .OrderBy(s => s.ClassDate)
                .ToList();

            attendanceItems.Clear();

            foreach (var scheduleItem in schedule)
            {
                // Получаем студентов группы
                var students = context.GroupStudent
                    .Where(gs => gs.GroupID == groupId)
                    .Select(gs => gs.Student)
                    .ToList();

                foreach (var student in students)
                {
                    // Проверяем посещаемость
                    var attendance = context.Attendance
                        .FirstOrDefault(a => a.ScheduleID == scheduleItem.ScheduleID &&
                                           a.StudentID == student.StudentID);

                    // Получаем статус посещаемости из связанной таблицы
                    string status = "Не отмечено";
                    if (attendance != null && attendance.StatusAttendanceID.HasValue)
                    {
                        var statusAttendance = context.StatusAttendance
                            .FirstOrDefault(sa => sa.StatusAttendanceID == attendance.StatusAttendanceID);
                        if (statusAttendance != null)
                        {
                            status = statusAttendance.StatusAttendanceName;
                        }
                    }

                    attendanceItems.Add(new AttendanceItem
                    {
                        ScheduleID = scheduleItem.ScheduleID,
                        StudentID = student.StudentID,
                        StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                        Date = scheduleItem.ClassDate.Value,
                        Time = $"{scheduleItem.Time.TimeStart.Value.Hours:D2}:{scheduleItem.Time.TimeStart.Value.Minutes:D2}",
                        Status = status,
                        StatusAttendanceID = attendance?.StatusAttendanceID
                    });
                }
            }
        }

        private void UpdateAttendanceStatus(AttendanceItem item, string statusName)
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
            LoadAttendanceData();
        }

        private void MarkPresent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AttendanceItem item)
            {
                UpdateAttendanceStatus(item, "Присутствует");
            }
        }

        private void MarkAbsent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AttendanceItem item)
            {
                UpdateAttendanceStatus(item, "Отсутствует");
            }
        }
        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAttendanceData();
        }

        private void EditAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is AttendanceItem item)
            {
                // Открываем окно редактирования посещаемости
                var editWindow = new EditAttendanceWindow(item);
                if (editWindow.ShowDialog() == true)
                {
                    LoadAttendanceData();
                }
            }
        }
    }

    public class AttendanceItem
    {
        public int ScheduleID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public int? StatusAttendanceID { get; set; }
    }

}
