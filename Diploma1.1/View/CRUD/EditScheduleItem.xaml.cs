using Diploma1._1.Model;
using Diploma1._1.View.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Diploma1._1.View.CRUD
{
    /// <summary>
    /// Логика взаимодействия для EditScheduleItem.xaml
    /// </summary>
    public partial class EditScheduleItem : Window
    {
        private Schedule currentSchedule;
        private ObservableCollection<Time> timeSlots;
        private ObservableCollection<Course> courses;
        private ObservableCollection<Group> groups;
        private ObservableCollection<Cabinet> cabinets;
        private ObservableCollection<Employee> teachers;
        private ObservableCollection<Student> students;

        public EditScheduleItem(int scheduleId = 0)
        {
            InitializeComponent();
            InitializeComboBoxes();

            if (scheduleId > 0)
            {
                using (var context = new Dimploma1Entities())
                {
                    currentSchedule = context.Schedule.Find(scheduleId);
                    if (currentSchedule != null)
                    {
                        DatePicker.SelectedDate = currentSchedule.ClassDate;
                        TimeComboBox.SelectedValue = currentSchedule.TimeID;
                        CourseComboBox.SelectedValue = currentSchedule.CourseID;
                        GroupComboBox.SelectedValue = currentSchedule.GroupID;
                        CabinetComboBox.SelectedValue = currentSchedule.CabinetID;
                        TeacherComboBox.SelectedValue = currentSchedule.TeacherID;

                        // Устанавливаем значение RadioButton в зависимости от наличия StudentID или GroupID
                        if (currentSchedule.StudentID.HasValue)
                        {
                            StudentRadioButton.IsChecked = true;
                            GroupRadioButton.IsChecked = false;
                            StudentComboBox.IsEnabled = true;
                            GroupComboBox.IsEnabled = false;
                            StudentComboBox.SelectedValue = currentSchedule.StudentID.Value;
                        }
                        else if (currentSchedule.GroupID.HasValue)
                        {
                            StudentRadioButton.IsChecked = false;
                            GroupRadioButton.IsChecked = true;
                            StudentComboBox.IsEnabled = false;
                            GroupComboBox.IsEnabled = true;
                        }
                    }
                }
            }
            else
            {
                currentSchedule = new Schedule
                {
                    ClassDate = DateTime.Today
                };
                DatePicker.SelectedDate = DateTime.Today;
                GroupRadioButton.IsChecked = true;
            }
        }

        private void InitializeComboBoxes()
        {
            using (var context = new Dimploma1Entities())
            {
                // Заполняем комбобокс временных слотов
                timeSlots = new ObservableCollection<Time>(context.Time.ToList());
                TimeComboBox.ItemsSource = timeSlots;
                TimeComboBox.DisplayMemberPath = "TimeStart";
                TimeComboBox.SelectedValuePath = "TimeID";

                // Заполняем комбобокс курсов
                courses = new ObservableCollection<Course>(context.Course.ToList());
                CourseComboBox.ItemsSource = courses;
                CourseComboBox.DisplayMemberPath = "CourseName";
                CourseComboBox.SelectedValuePath = "CourseID";

                // Заполняем комбобокс групп
                groups = new ObservableCollection<Group>(context.Group.ToList());
                GroupComboBox.ItemsSource = groups;
                GroupComboBox.DisplayMemberPath = "GroupName";
                GroupComboBox.SelectedValuePath = "GroupID";

                // Заполняем комбобокс кабинетов
                cabinets = new ObservableCollection<Cabinet>(context.Cabinet.ToList());
                CabinetComboBox.ItemsSource = cabinets;
                CabinetComboBox.DisplayMemberPath = "CabinetName";
                CabinetComboBox.SelectedValuePath = "CabinetID";

                // Получение информации о преподавателях
                var teachersList = (from user in context.Employee
                                    where user.EmployeeRoleID == 2
                                    select new UserFullName
                                    {
                                        UserID = user.EmployeeID,
                                        StatusID = (int)user.EmployeeRoleID,
                                        FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
                                    }).ToList();

                var teachers = new ObservableCollection<UserFullName>(teachersList);

                // Заполнение ComboBox для преподавателей
                TeacherComboBox.ItemsSource = teachers;
                TeacherComboBox.DisplayMemberPath = "FullName";
                TeacherComboBox.SelectedValuePath = "UserID";

                // Установка текущего преподавателя (если необходимо)
                var currentTeacherId = currentSchedule?.TeacherID;
                if (currentTeacherId.HasValue)
                {
                    var currentTeacher = teachers.FirstOrDefault(t => t.UserID == currentTeacherId.Value);
                    if (currentTeacher != null)
                    {
                        TeacherComboBox.SelectedItem = currentTeacher;
                    }
                }

                // Получение информации о студентах
                var studentsList = (from user in context.Student
                                    select new UserFullName
                                    {
                                        UserID = user.StudentID,
                                        StatusID = 3, // Статус студента
                                        FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
                                    }).ToList();

                var students = new ObservableCollection<UserFullName>(studentsList);

                // Заполнение ComboBox для студентов
                StudentComboBox.ItemsSource = students;
                StudentComboBox.DisplayMemberPath = "FullName";
                StudentComboBox.SelectedValuePath = "UserID";

                // Установка текущего студента (если необходимо)
                var currentStudentId = currentSchedule?.StudentID;
                if (currentStudentId.HasValue)
                {
                    var currentStudent = students.FirstOrDefault(s => s.UserID == currentStudentId.Value);
                    if (currentStudent != null)
                    {
                        StudentComboBox.SelectedItem = currentStudent;
                    }
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (currentSchedule.ScheduleID == 0)
                    {
                        context.Schedule.Add(currentSchedule);
                    }
                    else
                    {
                        var existingSchedule = context.Schedule.Find(currentSchedule.ScheduleID);
                        if (existingSchedule != null)
                        {
                            existingSchedule.ClassDate = currentSchedule.ClassDate;
                            existingSchedule.TimeID = currentSchedule.TimeID;
                            existingSchedule.CourseID = currentSchedule.CourseID;
                            existingSchedule.GroupID = currentSchedule.GroupID;
                            existingSchedule.CabinetID = currentSchedule.CabinetID;
                            existingSchedule.TeacherID = currentSchedule.TeacherID;
                            existingSchedule.StudentID = currentSchedule.StudentID;
                        }
                    }

                    context.SaveChanges();
                    MessageBox.Show("Расписание успешно сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TimeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CourseComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CabinetComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите кабинет", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TeacherComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите преподавателя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (StudentRadioButton.IsChecked == true && StudentComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите ученика", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (GroupRadioButton.IsChecked == true && GroupComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            currentSchedule.ClassDate = DatePicker.SelectedDate.Value;
            currentSchedule.TimeID = (int)TimeComboBox.SelectedValue;
            currentSchedule.CourseID = (int)CourseComboBox.SelectedValue;
            currentSchedule.CabinetID = (int)CabinetComboBox.SelectedValue;
            currentSchedule.TeacherID = (int)TeacherComboBox.SelectedValue;

            if (StudentRadioButton.IsChecked == true)
            {
                currentSchedule.StudentID = (int?)StudentComboBox.SelectedValue;
                currentSchedule.GroupID = null;
            }
            else
            {
                currentSchedule.StudentID = null;
                currentSchedule.GroupID = (int?)GroupComboBox.SelectedValue;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (spStudent != null && spGroup != null)
            {
                if (StudentRadioButton.IsChecked == true)
                {
                    spStudent.Visibility = Visibility.Visible;
                    spGroup.Visibility = Visibility.Collapsed;
                }
                else if (GroupRadioButton.IsChecked == true)
                {
                    spStudent.Visibility = Visibility.Collapsed;
                    spGroup.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
