using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Diploma1._1.Model;

namespace Diploma1._1.View.CRUD
{
    /// <summary>
    /// Логика взаимодействия для EditTaskItem.xaml
    /// </summary>
    public partial class EditTaskItem : Window
    {
        private TeacherTask currentTask;
        private int currentTeacherId;

        public EditTaskItem(int teacherId, int taskId = 0)
        {
            InitializeComponent();
            this.currentTeacherId = teacherId;
            InitializeComboBoxes();

            if (taskId > 0)
            {
                using (var context = new Dimploma1Entities())
                {
                    currentTask = context.TeacherTask.Find(taskId);
                    if (currentTask != null)
                    {
                        CreationDatePicker.SelectedDate = currentTask.DataCreate;
                        DueDatePicker.SelectedDate = currentTask.DataTask;
                        TaskTextBox.Text = currentTask.Task;
                        StatusCheckBox.IsChecked = currentTask.StatusTask;
                        
                        if (currentTask.StudentID.HasValue)
                        {
                            StudentComboBox.SelectedValue = currentTask.StudentID.Value;
                        }

                        TeacherComboBox.SelectedValue = currentTask.TeacherID;
                    }
                }
            }
            else
            {
                currentTask = new TeacherTask
                {
                    DataCreate = DateTime.Now,
                    DataTask = DateTime.Now,
                    TeacherID = teacherId,
                    StatusTask = false
                };
                CreationDatePicker.SelectedDate = DateTime.Now;
                DueDatePicker.SelectedDate = DateTime.Now;
                TeacherComboBox.SelectedValue = teacherId;
            }
        }

        private void InitializeComboBoxes()
        {
            using (var context = new Dimploma1Entities())
            {
                // Заполняем комбобокс студентов
                var students = context.Student
                    .Select(s => new
                    {
                        StudentID = s.StudentID,
                        LastName = s.LastName,
                        FirstName = s.FirstName
                    })
                    .ToList() // Выполняем запрос к базе данных
                    .Select(s => new // Теперь работаем с объектами в памяти
                    {
                        StudentID = s.StudentID,
                        FullName = s.LastName + " " + s.FirstName
                    });

                var studentObservableCollection = new ObservableCollection<dynamic>(students);
                StudentComboBox.ItemsSource = studentObservableCollection;
                StudentComboBox.DisplayMemberPath = "FullName";
                StudentComboBox.SelectedValuePath = "StudentID";

                // Заполняем комбобокс преподавателей
                var teachers = context.Employee
                    .Where(e => e.EmployeeRoleID == 2)
                    .Select(t => new
                    {
                        TeacherID = t.EmployeeID,
                        LastName = t.LastName,
                        FirstName = t.FirstName
                    })
                    .ToList() // Выполняем запрос к базе данных
                    .Select(t => new // Теперь работаем с объектами в памяти
                    {
                        TeacherID = t.TeacherID,
                        FullName = t.LastName + " " + t.FirstName
                    });

                var teacherObservableCollection = new ObservableCollection<dynamic>(teachers);
                TeacherComboBox.ItemsSource = teacherObservableCollection;
                TeacherComboBox.DisplayMemberPath = "FullName";
                TeacherComboBox.SelectedValuePath = "TeacherID";
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
                    if (currentTask.TeacherTaskID == 0)
                    {
                        context.TeacherTask.Add(currentTask);
                    }
                    else
                    {
                        var existingTask = context.TeacherTask.Find(currentTask.TeacherTaskID);
                        if (existingTask != null)
                        {
                            existingTask.DataCreate = currentTask.DataCreate;
                            existingTask.DataTask = currentTask.DataTask;
                            existingTask.Task = currentTask.Task;
                            existingTask.StudentID = currentTask.StudentID;
                            existingTask.TeacherID = currentTask.TeacherID;
                            existingTask.StatusTask = currentTask.StatusTask;
                        }
                    }

                    context.SaveChanges();
                    MessageBox.Show("Задача успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (CreationDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату создания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату выполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DueDatePicker.SelectedDate.Value < CreationDatePicker.SelectedDate.Value)
            {
                MessageBox.Show("Дата выполнения не может быть раньше даты создания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TaskTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите текст задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (TeacherComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите преподавателя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            currentTask.DataCreate = CreationDatePicker.SelectedDate.Value;
            currentTask.DataTask = DueDatePicker.SelectedDate.Value;
            currentTask.Task = TaskTextBox.Text;
            currentTask.StatusTask = StatusCheckBox.IsChecked ?? false;
            currentTask.StudentID = StudentComboBox.SelectedValue as int?;
            currentTask.TeacherID = (int)TeacherComboBox.SelectedValue;

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
