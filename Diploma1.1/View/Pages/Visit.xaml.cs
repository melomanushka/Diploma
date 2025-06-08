using Diploma1._1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
// using System.ComponentModel; // Для INotifyPropertyChanged
// using System.Runtime.CompilerServices; // Для INotifyPropertyChanged

namespace Diploma1._1.View.Pages
{
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
            try
            {
                var groups = context.Group.AsNoTracking().OrderBy(g => g.GroupName).ToList();
                GroupComboBox.ItemsSource = groups;
                var courses = context.Course.AsNoTracking().OrderBy(c => c.CourseName).ToList();
                CourseComboBox.ItemsSource = courses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для фильтров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVisitData()
        {
            if (GroupComboBox.SelectedValue == null || CourseComboBox.SelectedValue == null ||
               DatePicker.SelectedDate == null)
            {
                visitItems.Clear();
                //MessageBox.Show("Пожалуйста, заполните все поля фильтрации.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int groupId = (int)GroupComboBox.SelectedValue;
            int courseId = (int)CourseComboBox.SelectedValue;
            DateTime selectedDate = DatePicker.SelectedDate.Value.Date;

            visitItems.Clear();

            try
            {
                var schedules = context.Schedule
                    .Include(s => s.Time)
                    .Where(s => s.GroupID == groupId &&
                                s.CourseID == courseId &&
                                DbFunctions.TruncateTime(s.ClassDate) == selectedDate)
                    .OrderBy(s => s.Time.TimeStart)
                    .ToList();

                if (!schedules.Any()) return;

                var studentIds = context.GroupStudent
                    .Where(gs => gs.GroupID == groupId)
                    .Select(gs => gs.StudentID)
                    .ToList();

                if (!studentIds.Any()) return;

                var students = context.Student
                    .Where(s => studentIds.Contains(s.StudentID))
                    .Select(s => new { s.StudentID, s.LastName, s.FirstName, s.MiddleName })
                    .ToDictionary(s => s.StudentID);

                var scheduleIds = schedules.Select(s => s.ScheduleID).ToList();

                var attendanceRecords = context.Attendance
                    .Include(a => a.StatusAttendance)
                    .Where(a => scheduleIds.Contains(a.ScheduleID ?? 0) &&
                                studentIds.Contains(a.StudentID ?? 0))
                    .ToList()
                    .GroupBy(a => (ScheduleID: a.ScheduleID ?? 0, StudentID: a.StudentID ?? 0))
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var schedule in schedules)
                {
                    string timeString = "N/A";
                    if (schedule.Time != null && schedule.Time.TimeStart.HasValue && schedule.Time.TimeEnd.HasValue)
                    {
                        timeString = $"{schedule.Time.TimeStart.Value:hh\\:mm} - {schedule.Time.TimeEnd.Value:hh\\:mm}";
                    }
                    else if (schedule.Time != null)
                    {
                        timeString = "Время не указано";
                    }

                    foreach (var studentId in studentIds)
                    {
                        if (!students.TryGetValue((int)studentId, out var student)) continue;

                        attendanceRecords.TryGetValue((schedule.ScheduleID, student.StudentID), out var attendance);

                        visitItems.Add(new VisitItem
                        {
                            ScheduleID = schedule.ScheduleID,
                            StudentID = student.StudentID,
                            StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
                            Time = timeString,
                            Status = attendance?.StatusAttendance?.StatusAttendanceName ?? "Не отмечено",
                            StatusAttendanceID = attendance?.StatusAttendanceID
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о посещаемости: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadVisitData();
        }

        // --- ИСПРАВЛЕННЫЙ МЕТОД ---
        private void UpdateAttendanceStatus(VisitItem item, string statusName)
        {
            if (item == null) return;

            // Объявляем переменную ДО блока try
            Attendance attendance = null;
            bool isNew = false; // Флаг, чтобы знать, была ли запись новой

            try
            {
                var statusAttendance = context.StatusAttendance
                    .FirstOrDefault(sa => sa.StatusAttendanceName == statusName);

                if (statusAttendance == null)
                {
                    MessageBox.Show("Статус посещаемости отмечен");
                    return;
                }

                // Присваиваем значение переменной, объявленной выше (убираем var)
                attendance = context.Attendance
                    .FirstOrDefault(a => a.ScheduleID == item.ScheduleID &&
                                          a.StudentID == item.StudentID);

                if (attendance == null) // Если записи нет - создаем
                {
                    isNew = true; // Отмечаем, что запись новая
                    // Присваиваем значение переменной, объявленной выше
                    attendance = new Attendance
                    {
                        ScheduleID = item.ScheduleID,
                        StudentID = item.StudentID,
                        StatusAttendanceID = statusAttendance.StatusAttendanceID
                    };
                    context.Attendance.Add(attendance);
                    Console.WriteLine($"Добавление записи: StudentID={item.StudentID}, ScheduleID={item.ScheduleID}, StatusID={statusAttendance.StatusAttendanceID}"); // Логирование
                }
                else // Если запись есть - обновляем статус
                {
                    isNew = false;
                    attendance.StatusAttendanceID = statusAttendance.StatusAttendanceID;
                    Console.WriteLine($"Обновление записи: AttendanceID={attendance.AttendanceID}, Новый StatusID={statusAttendance.StatusAttendanceID}"); // Логирование
                }

                int affectedRows = context.SaveChanges();
                Console.WriteLine($"SaveChanges выполнено. Затронуто строк: {affectedRows}");

                LoadVisitData();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Ошибка при обновлении статуса: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.Message}";
                }
                Console.WriteLine($"ИСКЛЮЧЕНИЕ: {errorMessage}");
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Попытка откатить изменения в контексте, если SaveChanges не удалось
                try
                {
                    // Теперь 'attendance' доступна здесь
                    if (attendance != null)
                    {
                        var entry = context.Entry(attendance);
                        if (entry != null)
                        {
                            // Если запись была новой и не сохранилась, отсоединяем ее
                            if (isNew && entry.State == EntityState.Added)
                            {
                                entry.State = EntityState.Detached;
                            }
                            // Если запись существовала и не обновилась, возвращаем в Unchanged
                            else if (!isNew && entry.State == EntityState.Modified)
                            {
                                entry.State = EntityState.Unchanged;
                            }
                            // Можно добавить обработку других состояний при необходимости
                        }
                    }
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"Ошибка при откате изменений контекста: {rollbackEx.Message}");
                }
            }
        }
        // --- КОНЕЦ ИСПРАВЛЕННОГО МЕТОДА ---


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

        // Освобождение контекста при закрытии страницы (рекомендуется)
        // private void Page_Unloaded(object sender, RoutedEventArgs e)
        // {
        //     context?.Dispose();
        // }
    }

    // Класс VisitItem остается без изменений
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