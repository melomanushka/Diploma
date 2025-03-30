using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Diploma1._1.Model;

namespace Diploma1._1.View.CRUD
{
    public partial class EditTaskItem : Window
    {
        private TeacherTask currentTask;
        private int teacherId;

        public EditTaskItem(int teacherId, int taskId = 0)
        {
            InitializeComponent();
            this.teacherId = teacherId;
            InitializeComboBoxes();

            if (taskId > 0)
            {
                using (var context = new Dimploma1Entities())
                {
                    currentTask = context.TeacherTask.Find(taskId);
                    if (currentTask != null)
                    {
                        DatePicker.SelectedDate = currentTask.DataTask;
                        TaskTextBox.Text = currentTask.Task;
                        StatusCheckBox.IsChecked = currentTask.StatusTask;
                        
                        if (currentTask.StudentID.HasValue)
                        {
                            StudentComboBox.SelectedValue = currentTask.StudentID.Value;
                        }
                    }
                }
            }
            else
            {
                currentTask = new TeacherTask
                {
                    DataTask = DateTime.Now,
                    TeacherID = teacherId,
                    StatusTask = false
                };
                DatePicker.SelectedDate = currentTask.DataTask;
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
                        FullName = $"{s.LastName} {s.FirstName}"
                    })
                    .ToList();

                var studentObservableCollection = new ObservableCollection<dynamic>(students);
                StudentComboBox.ItemsSource = studentObservableCollection;
                StudentComboBox.DisplayMemberPath = "FullName";
                StudentComboBox.SelectedValuePath = "StudentID";
            }
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
                            existingTask.DataTask = currentTask.DataTask;
                            existingTask.Task = currentTask.Task;
                            existingTask.StudentID = currentTask.StudentID;
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
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TaskTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите текст задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            currentTask.DataTask = DatePicker.SelectedDate.Value;
            currentTask.Task = TaskTextBox.Text;
            currentTask.StatusTask = StatusCheckBox.IsChecked ?? false;
            currentTask.StudentID = StudentComboBox.SelectedValue as int?;

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 