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
        private bool isIndividualLesson = false;

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

            // Загрузка групп
            var groups = context.Group.ToList();
            GroupComboBox.ItemsSource = groups;
            GroupComboBox.DisplayMemberPath = "GroupName";
            GroupComboBox.SelectedValuePath = "GroupID";

            // Загрузка всех студентов для индивидуальных занятий
            var students = context.Student.ToList();
            StudentComboBox.ItemsSource = students;
            StudentComboBox.DisplayMemberPath = "LastName"; // Можно настроить более полное отображение ФИО
            StudentComboBox.SelectedValuePath = "StudentID";

            // Загрузка предметов
            var subjects = context.Course.ToList();
            SubjectComboBox.ItemsSource = subjects;
            IndividualSubjectComboBox.ItemsSource = subjects;
            SubjectComboBox.DisplayMemberPath = "CourseName";
            IndividualSubjectComboBox.DisplayMemberPath = "CourseName";
            SubjectComboBox.SelectedValuePath = "CourseID";
            IndividualSubjectComboBox.SelectedValuePath = "CourseID";

            // Установка текущей даты
            LessonDatePicker.SelectedDate = DateTime.Today;
        }

        private void LessonType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                isIndividualLesson = radioButton == IndividualLessonRadio;
                //GroupSelectionGrid.Visibility = isIndividualLesson ? Visibility.Collapsed : Visibility.Visible;
                //StudentSelectionGrid.Visibility = isIndividualLesson ? Visibility.Visible : Visibility.Collapsed;
                
                // Очищаем таблицу при смене типа занятия
                //gradeRecords.Clear();
            }
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
            if (LessonDatePicker.SelectedDate == null) return;

            DateTime lessonDate = LessonDatePicker.SelectedDate.Value;
            gradeRecords.Clear();

            if (isIndividualLesson)
            {
                LoadIndividualLessonData(lessonDate);
            }
            else
            {
                LoadGroupLessonData(lessonDate);
            }
        }

        private void LoadIndividualLessonData(DateTime lessonDate)
        {
            if (StudentComboBox.SelectedValue == null || IndividualSubjectComboBox.SelectedValue == null)
                return;

            int studentId = (int)StudentComboBox.SelectedValue;
            int subjectId = (int)IndividualSubjectComboBox.SelectedValue;

            // Загружаем данные индивидуального занятия
            //var student = context.Student.FirstOrDefault(s => s.StudentID == studentId);
            //if (student != null)
            //{
            //    // Проверяем существующую оценку
            //    var individualLesson = context.IndividualLesson
            //        .FirstOrDefault(il => il.StudentID == studentId &&
            //                            il.CourseID == subjectId &&
            //                            il.LessonDate == lessonDate);

            //    string grade = individualLesson?.Grade ?? "";
            //    string note = individualLesson?.Note ?? "";

            //    gradeRecords.Add(new GradeRecord
            //    {
            //        Number = 1,
            //        StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
            //        Grade = grade,
            //        Note = note
            //    });
            //}
        }

        private void LoadGroupLessonData(DateTime lessonDate)
        {
            if (GroupComboBox.SelectedValue == null || SubjectComboBox.SelectedValue == null)
                return;

            int groupId = (int)GroupComboBox.SelectedValue;
            int subjectId = (int)SubjectComboBox.SelectedValue;

            // Загружаем расписание для выбранной группы и предмета
            var schedule = context.Schedule
                .Where(s => s.GroupID == groupId && 
                           s.CourseID == subjectId &&
                           s.ClassDate == lessonDate)
                .OrderBy(s => s.ClassDate)
                .ToList();

            foreach (var scheduleItem in schedule)
            {
                var students = context.GroupStudent
                    .Where(gs => gs.GroupID == groupId)
                    .Select(gs => gs.Student)
                    .ToList();

                int counter = 1;
                //foreach (var student in students)
                //{
                //    var attendance = context.Attendance
                //        .FirstOrDefault(a => a.ScheduleID == scheduleItem.ScheduleID &&
                //                           a.StudentID == student.StudentID);

                //    string grade = attendance?.Grade ?? "";
                //    string note = attendance?.Note ?? "";

                //    gradeRecords.Add(new GradeRecord
                //    {
                //        Number = counter++,
                //        StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}",
                //        Grade = grade,
                //        Note = note
                //    });
                //}
            }
        }

        private void StudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentComboBox.SelectedItem != null)
            {
                LoadAttendanceData();
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
