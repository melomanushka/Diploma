using Diploma1._1.Model;
using Diploma1._1.View.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Diploma1._1.View.CRUD
{
    /// <summary>
    /// Логика взаимодействия для EditScheduleItem.xaml
    /// </summary>
    public partial class EditScheduleItem : Window
    {
        private ScheduleItem currentSchedule;

        public EditScheduleItem(ScheduleItem scheduleItem)
        {
            InitializeComponent();
            currentSchedule = scheduleItem;
            InitializeComboBoxes();
            DataContext = currentSchedule;
            dpDate.SelectedDate = currentSchedule.Date;

            // Скрываем кнопку удаления при добавлении нового занятия
            if (currentSchedule.ScheduleID == 0)
            {
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeComboBoxes()
        {
            using (var context = new Dimploma1Entities())
            {
                // Заполняем комбобокс временных слотов
                var timeList = context.Time
                    .Select(t => new
                    {
                        TimeID = t.TimeID,
                        TimeStart = t.TimeStart,
                        TimeEnd = t.TimeEnd
                    })
                    .ToList();

                var formattedTimeList = timeList.Select(t => new
                {
                    TimeID = t.TimeID,
                    TimeRange = $"{t.TimeStart.Value:HH:mm} - {t.TimeEnd.Value:HH:mm}"
                }).ToList();

                var timeObservableCollection = new ObservableCollection<dynamic>(formattedTimeList);
                cmbxTime.ItemsSource = timeObservableCollection;
                cmbxTime.DisplayMemberPath = "TimeRange";
                cmbxTime.SelectedValuePath = "TimeID";

                // Устанавливаем текущее время
                var currentTime = timeObservableCollection.FirstOrDefault(t => t.TimeID == currentSchedule.TimeID);
                if (currentTime != null)
                {
                    cmbxTime.SelectedItem = currentTime;
                }

                // Заполняем комбобокс курсов
                var courses = context.Course.ToList();
                var courseObservableCollection = new ObservableCollection<Course>(courses);
                cmbxCourse.ItemsSource = courseObservableCollection;
                cmbxCourse.DisplayMemberPath = "CourseName";
                cmbxCourse.SelectedValuePath = "CourseID";

                // Устанавливаем текущий курс
                var currentCourse = courseObservableCollection.FirstOrDefault(c => c.CourseID == currentSchedule.CourseID);
                if (currentCourse != null)
                {
                    cmbxCourse.SelectedItem = currentCourse;
                }

                // Заполняем комбобокс студентов
                var studentsList = (from user in context.Student
                                   select new UserFullName
                                   {
                                       UserID = user.StudentID,
                                       FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
                                   }).ToList();

                var studentObservableCollection = new ObservableCollection<UserFullName>(studentsList);
                cmbxStudent.ItemsSource = studentObservableCollection;
                cmbxStudent.DisplayMemberPath = "FullName";
                cmbxStudent.SelectedValuePath = "UserID";

                // Заполняем комбобокс групп
                var groups = context.Group.ToList();
                var groupObservableCollection = new ObservableCollection<Group>(groups);
                cmbxGroup.ItemsSource = groupObservableCollection;
                cmbxGroup.DisplayMemberPath = "GroupName";
                cmbxGroup.SelectedValuePath = "GroupID";

                // Заполняем комбобокс кабинетов
                var cabinets = context.Cabinet.ToList();
                var cabinetObservableCollection = new ObservableCollection<Cabinet>(cabinets);
                cmbxCabinet.ItemsSource = cabinetObservableCollection;
                cmbxCabinet.DisplayMemberPath = "CabinetName";
                cmbxCabinet.SelectedValuePath = "CabinetID";

                // Устанавливаем текущий кабинет
                var currentCabinet = cabinetObservableCollection.FirstOrDefault(c => c.CabinetID == currentSchedule.CabinetID);
                if (currentCabinet != null)
                {
                    cmbxCabinet.SelectedItem = currentCabinet;
                }

                // Заполняем комбобокс преподавателей
                var teachersList = (from user in context.Employee
                                  where user.EmployeeRoleID == 2
                                  select new UserFullName
                                  {
                                      UserID = user.EmployeeID,
                                      StatusID = (int)user.EmployeeRoleID,
                                      FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
                                  }).ToList();

                var teacherObservableCollection = new ObservableCollection<UserFullName>(teachersList);
                cmbxTeacher.ItemsSource = teacherObservableCollection;
                cmbxTeacher.DisplayMemberPath = "FullName";
                cmbxTeacher.SelectedValuePath = "UserID";

                // Устанавливаем текущего преподавателя
                var currentTeacher = teacherObservableCollection.FirstOrDefault(t => t.UserID == currentSchedule.TeacherID);
                if (currentTeacher != null)
                {
                    cmbxTeacher.SelectedItem = currentTeacher;
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (rbStudent.IsChecked == true)
            {
                spStudent.Visibility = Visibility.Visible;
                spGroup.Visibility = Visibility.Collapsed;
            }
            else if (rbGroup.IsChecked == true)
            {
                spStudent.Visibility = Visibility.Collapsed;
                spGroup.Visibility = Visibility.Visible;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                using (var context = new Dimploma1Entities())
                {
                    var scheduleToUpdate = context.Schedule.FirstOrDefault(s => s.ScheduleID == currentSchedule.ScheduleID);
                    if (scheduleToUpdate != null)
                    {
                        scheduleToUpdate.ClassDate = dpDate.SelectedDate;
                        scheduleToUpdate.TimeID = (int)cmbxTime.SelectedValue;
                        scheduleToUpdate.CourseID = (int)cmbxCourse.SelectedValue;
                        scheduleToUpdate.CabinetID = (int)cmbxCabinet.SelectedValue;
                        scheduleToUpdate.TeacherID = (int)cmbxTeacher.SelectedValue;

                        // Определяем GroupID в зависимости от выбранного типа
                        if (rbStudent.IsChecked == true)
                        {
                            int studentId = (int)cmbxStudent.SelectedValue;
                            var studentGroup = context.GroupStudent
                                .FirstOrDefault(sg => sg.StudentID == studentId);
                            if (studentGroup != null)
                            {
                                scheduleToUpdate.GroupID = studentGroup.GroupID;
                            }
                        }
                        else
                        {
                            scheduleToUpdate.GroupID = (int)cmbxGroup.SelectedValue;
                        }

                        context.SaveChanges();
                        MessageBox.Show("Расписание успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        var newSchedule = new Schedule
                        {
                            ClassDate = dpDate.SelectedDate,
                            TimeID = (int)cmbxTime.SelectedValue,
                            CourseID = (int)cmbxCourse.SelectedValue,
                            CabinetID = (int)cmbxCabinet.SelectedValue,
                            TeacherID = (int)cmbxTeacher.SelectedValue
                        };

                        // Определяем GroupID в зависимости от выбранного типа
                        if (rbStudent.IsChecked == true)
                        {
                            int studentId = (int)cmbxStudent.SelectedValue;
                            var studentGroup = context.GroupStudent
                                .FirstOrDefault(sg => sg.StudentID == studentId);
                            if (studentGroup != null)
                            {
                                newSchedule.GroupID = studentGroup.GroupID;
                            }
                        }
                        else
                        {
                            newSchedule.GroupID = (int)cmbxGroup.SelectedValue;
                        }

                        context.Schedule.Add(newSchedule);
                        context.SaveChanges();
                        MessageBox.Show("Расписание успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
            }
        }

        private bool ValidateInputs()
        {
            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbxTime.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите время.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbxCourse.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите курс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (rbStudent.IsChecked == true && cmbxStudent.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите обучающегося.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (rbGroup.IsChecked == true && cmbxGroup.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите группу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbxCabinet.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите кабинет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cmbxTeacher.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите преподавателя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSchedule.ScheduleID != 0)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Удаление", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new Dimploma1Entities())
                    {
                        var scheduleToRemove = context.Schedule.FirstOrDefault(s => s.ScheduleID == currentSchedule.ScheduleID);
                        if (scheduleToRemove != null)
                        {
                            context.Schedule.Remove(scheduleToRemove);
                            context.SaveChanges();
                            MessageBox.Show("Запись успешно удалена!", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                            Close();
                        }
                    }
                }
            }
        }
    }
}
