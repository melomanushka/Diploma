using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diploma1._1.Model;
using Diploma1._1.View.CRUD;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Planner.xaml
    /// </summary>
    public partial class Planner : Page
    {
        private ObservableCollection<TaskItem> tasks;
        private DateTime selectedDate;
        private int currentTeacherId;

        public Planner()
        {
            InitializeComponent();
            tasks = new ObservableCollection<TaskItem>();
            TasksListView.ItemsSource = tasks;
            selectedDate = DateTime.Today;
            MainCalendar.SelectedDate = selectedDate;
            
            // Получаем ID текущего преподавателя (в реальном приложении это должно быть из системы авторизации)
            using (var context = new Dimploma1Entities())
            {
                var teacher = context.Employee.FirstOrDefault(e => e.EmployeeRoleID == 2);
                if (teacher != null)
                {
                    currentTeacherId = teacher.EmployeeID;
                }
            }

            LoadTasks();
        }

        private void LoadTasks()
        {
            tasks.Clear();
            using (var context = new Dimploma1Entities())
            {
                var dbTasks = context.TeacherTask
                    .Where(t => t.DataTask == selectedDate.Date && t.TeacherID == currentTeacherId)
                    .OrderBy(t => t.DataTask)
                    .ToList();

                foreach (var task in dbTasks)
                {
                    var studentName = "";
                    if (task.StudentID.HasValue)
                    {
                        var student = context.Student.Find(task.StudentID.Value);
                        if (student != null)
                        {
                            studentName = $"{student.LastName} {student.FirstName}";
                        }
                    }

                    tasks.Add(new TaskItem
                    {
                        TaskID = task.TeacherTaskID,
                        Time = task.DataTask.HasValue ? task.DataTask.Value.ToString("dd.MM.yyyy") : "Не указано",
                        Title = task.Task,
                        StudentName = studentName,
                        Status = (bool)task.StatusTask ? "Выполнено" : "Не выполнено"
                    });

                }
            }

            TasksHeader.Text = $"Задачи на {selectedDate:dd.MM.yyyy}";
        }

        private void MainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainCalendar.SelectedDate.HasValue)
            {
                selectedDate = MainCalendar.SelectedDate.Value;
                LoadTasks();
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var editTaskWindow = new EditTaskItem(currentTeacherId);
            if (editTaskWindow.ShowDialog() == true)
            {
                LoadTasks();
            }
        }

        private void TasksListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TasksListView.SelectedItem is TaskItem selectedTask)
            {
                var editTaskWindow = new EditTaskItem(currentTeacherId, selectedTask.TaskID);
                if (editTaskWindow.ShowDialog() == true)
                {
                    LoadTasks();
                }
            }
        }
    }

    public class TaskItem
    {
        public int TaskID { get; set; }
        public string Time { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StudentName { get; set; }
        public string Status { get; set; }
    }
}
