using Diploma1._1.Model;
using Diploma1._1.View.CRUD;
using System;
using System.Collections.Generic;
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
        private ObservableCollection<GradeRecord> gradeRecords;
        private List<string> grades = new List<string> { "5", "4", "3", "2", "Н" }; // Возможные оценки
        private Dimploma1Entities context;

        public Gradebook()
        {
            InitializeComponent();
            context = new Dimploma1Entities();
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Инициализация коллекции для хранения оценок
            gradeRecords = new ObservableCollection<GradeRecord>();
            GradesDataGrid.ItemsSource = gradeRecords;

            // Установка возможных оценок в комбобокс
            ((DataGridComboBoxColumn)GradesDataGrid.Columns[2]).ItemsSource = grades;

            // Здесь должна быть загрузка групп из базы данных
            GroupComboBox.ItemsSource = new List<string> { "Группа 1", "Группа 2", "Группа 3" };

            // Здесь должна быть загрузка предметов из базы данных
            SubjectComboBox.ItemsSource = new List<string> { "Математика", "Физика", "Информатика" };

            // Установка текущей даты
            LessonDatePicker.SelectedDate = DateTime.Today;
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.SelectedItem != null)
            {
                LoadStudents();
            }
        }

        private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubjectComboBox.SelectedItem != null && GroupComboBox.SelectedItem != null)
            {
                LoadGrades();
            }
        }

        private void LessonDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LessonDatePicker.SelectedDate.HasValue && GroupComboBox.SelectedItem != null && SubjectComboBox.SelectedItem != null)
            {
                LoadGrades();
            }
        }

        private void LoadStudents()
        {
            // Здесь должна быть загрузка студентов из базы данных
            gradeRecords.Clear();
            // Пример данных
            for (int i = 1; i <= 5; i++)
            {
                gradeRecords.Add(new GradeRecord
                {
                    Number = i,
                    StudentName = $"Студент {i}",
                    Grade = null,
                    Note = ""
                });
            }
        }

        private void LoadGrades()
        {
            // Здесь должна быть загрузка оценок из базы данных
            // на основе выбранной группы, предмета и даты
        }

        private void LoadAttendanceData()
        {
            if (GroupComboBox.SelectedValue == null || SubjectComboBox.SelectedValue == null ||
                LessonDatePicker.SelectedDate == null)
                return;

            int groupId = (int)GroupComboBox.SelectedValue;
            int subjectId = (int)SubjectComboBox.SelectedValue;
            DateTime lessonDate = LessonDatePicker.SelectedDate.Value;

            // Загружаем расписание для выбранной группы и предмета
            var schedule = context.Schedule
                .Where(s => s.GroupID == groupId && s.CourseID == subjectId &&
                           s.ClassDate == lessonDate)
                .OrderBy(s => s.ClassDate)
                .ToList();

            gradeRecords.Clear();

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

                    gradeRecords.Add(new GradeRecord
                    {
                        Number = 1, // Assuming a single grade per student
                        StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                        Grade = status,
                        Note = ""
                    });
                }
            }
        }

        private void UpdateGrade(GradeRecord item, string grade)
        {
            item.Grade = grade;
            // Save changes to database
            // This is a placeholder and should be replaced with actual database saving logic
        }

        private void EditGrade_Click(object sender, RoutedEventArgs e)
        {
            //if (sender is Button button && button.DataContext is GradeRecord item)
            //{
            //    // Open grade editing window
            //    var editWindow = new EditGradeWindow(item);
            //    if (editWindow.ShowDialog() == true)
            //    {
            //        LoadGrades();
            //    }
            //}
        }
    }

    public class GradeRecord
    {
        public int Number { get; set; }
        public string StudentName { get; set; }
        public string Grade { get; set; }
        public string Note { get; set; }
    }
}
