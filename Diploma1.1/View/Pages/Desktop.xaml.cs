using Diploma1._1.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Desktop.xaml
    /// </summary>
    public partial class Desktop : Page
    {
        private ObservableCollection<TaskItem> todayTasks;
        private ObservableCollection<GroupStatsItem> groupsStats;
        private ObservableCollection<TeacherStatsItem> teachersStats;
        private ObservableCollection<UpcomingClassItem> upcomingClasses;
        private ObservableCollection<NotificationItem> notifications;
        private Dimploma1Entities context;

        public Desktop()
        {
            InitializeComponent();
            context = new Dimploma1Entities();

            // Инициализация коллекций
            todayTasks = new ObservableCollection<TaskItem>();
            groupsStats = new ObservableCollection<GroupStatsItem>();
            teachersStats = new ObservableCollection<TeacherStatsItem>();
            upcomingClasses = new ObservableCollection<UpcomingClassItem>();
            notifications = new ObservableCollection<NotificationItem>();

            // Привязка к элементам управления
            TodayTasksListView.ItemsSource = todayTasks;
            GroupsStatsListView.ItemsSource = groupsStats;
            TeachersStatsListView.ItemsSource = teachersStats;
            UpcomingClassesListView.ItemsSource = upcomingClasses;
            NotificationsListView.ItemsSource = notifications;

            // Установка текущей даты
            CurrentDateText.Text = DateTime.Now.ToString("dd.MM.yyyy");

            // Загрузка данных
            LoadTodayTasks();
            LoadGroupsStats();
            LoadTeachersStats();
            LoadUpcomingClasses();
            LoadNotifications();
        }

        private void LoadTodayTasks()
        {
            var tasks = context.TeacherTask
                .Where(t => t.DataTask == DateTime.Today && t.StatusTask == false)
                .OrderBy(t => t.DataTask)
                .ToList();

            todayTasks.Clear();
            foreach (var task in tasks)
            {
                var student = context.Student.FirstOrDefault(s => s.StudentID == task.StudentID);
                var teacher = context.Employee.FirstOrDefault(e => e.EmployeeID == task.TeacherID);

                todayTasks.Add(new TaskItem
                {
                    TaskID = task.TeacherTaskID,
                    Title = task.Task,
                    // Проверяем, что student и teacher не null перед использованием
                    Description = student != null && teacher != null ?
                        $"Студент: {student.LastName} {student.FirstName}\nПреподаватель: {teacher.LastName} {teacher.FirstName}" :
                        "Информация недоступна",
                    Time = task.DataTask.HasValue ? task.DataTask.Value.ToString("HH:mm") : "--:--"
                });
            }
        }

        private void LoadGroupsStats()
        {
            // Получаем группы и связанные данные
            var groups = context.Group.ToList();
            groupsStats.Clear();

            foreach (var group in groups)
            {
                // Получаем курс для группы
                var course = context.Course.FirstOrDefault(c => c.CourseID == group.CourseID);

                // Считаем количество студентов в группе
                var studentCount = context.GroupStudent.Count(gs => gs.GroupID == group.GroupID);

                // Получаем все записи посещаемости для данной группы
                var attendances = context.Attendance
                    .Where(a => a.Schedule.GroupID == group.GroupID)
                    .ToList();

                // Посещаемость (с проверкой деления на ноль)
                double attendanceRate = 0;
                if (attendances.Count > 0)
                {
                    var presentCount = attendances
                        .Count(a => a.StatusAttendanceID ==
                            context.StatusAttendance
                                .FirstOrDefault(s => s.StatusAttendanceName == "Присутствует")?.StatusAttendanceID);

                    attendanceRate = (double)presentCount * 100 / attendances.Count;
                }

                groupsStats.Add(new GroupStatsItem
                {
                    GroupName = group.GroupName,
                    CourseName = course?.CourseName ?? "Неизвестный курс",
                    StudentCount = $"Студентов: {studentCount}",
                    Attendance = $"Посещаемость: {attendanceRate:F1}%"
                });
            }
        }

        private void LoadTeachersStats()
        {
            // Получаем преподавателей
            var teachers = context.Employee
                .Where(e => e.EmployeeRoleID == 2) // ID роли преподавателя
                .ToList();

            teachersStats.Clear();

            foreach (var teacher in teachers)
            {
                // Получаем первый курс преподавателя
                var teacherCourse = context.TeacherCourse
                    .FirstOrDefault(tc => tc.TeacherID == teacher.EmployeeID);

                string courseName = "Нет курса";
                if (teacherCourse != null)
                {
                    var course = context.Course.FirstOrDefault(c => c.CourseID == teacherCourse.CourseID);
                    courseName = course?.CourseName ?? "Нет курса";
                }

                // Считаем часы (количество занятий * 2)
                var schedules = context.Schedule
                    .Where(s => s.CourseID.HasValue && context.TeacherCourse
                        .Any(tc => tc.TeacherID == teacher.EmployeeID && tc.CourseID == s.CourseID))
                    .ToList();

                int hours = schedules.Count * 2;

                // Считаем уникальные группы
                var groupCount = schedules
                    .Select(s => s.GroupID)
                    .Distinct()
                    .Count();

                teachersStats.Add(new TeacherStatsItem
                {
                    TeacherName = $"{teacher.LastName} {teacher.FirstName} {teacher.MiddleName}",
                    CourseName = courseName,
                    Hours = $"Часов: {hours}",
                    Groups = $"Групп: {groupCount}"
                });
            }
        }

        private void LoadUpcomingClasses()
        {
            var classes = context.Schedule
                .Where(s => s.ClassDate >= DateTime.Today)
                .OrderBy(s => s.ClassDate)
                .ThenBy(s => s.Time.TimeStart)
                .Take(5)
                .ToList();

            upcomingClasses.Clear();
            foreach (var classItem in classes)
            {
                var course = context.Course.FirstOrDefault(c => c.CourseID == classItem.CourseID);
                var group = context.Group.FirstOrDefault(g => g.GroupID == classItem.GroupID);
                var cabinet = context.Cabinet.FirstOrDefault(c => c.CabinetID == classItem.CabinetID);
                var timeInfo = context.Time.FirstOrDefault(t => t.TimeID == classItem.TimeID);

                string timeStr = "--:--";
                if (timeInfo?.TimeStart != null)
                {
                    var time = timeInfo.TimeStart.Value;
                    timeStr = $"{time.Hours}:{time.Minutes:D2}";
                }

                upcomingClasses.Add(new UpcomingClassItem
                {
                    Time = timeStr,
                    CourseName = course?.CourseName ?? "Неизвестный курс",
                    GroupName = group?.GroupName ?? "Неизвестная группа",
                    Cabinet = cabinet?.CabinetName ?? "Неизвестный кабинет"
                });
            }
        }

        private void LoadNotifications()
        {
            notifications.Clear();

            // Добавляем уведомления о предстоящих занятиях
            var currentHour = DateTime.Now.Hour;
            var upcomingSchedules = context.Schedule
                .Where(s => s.ClassDate == DateTime.Today)
                .ToList()
                .Where(s => s.Time != null && s.Time.TimeStart.HasValue &&
                        s.Time.TimeStart.Value.Hours == currentHour + 1)
                .ToList();

            foreach (var schedule in upcomingSchedules)
            {
                var course = context.Course.FirstOrDefault(c => c.CourseID == schedule.CourseID);
                var group = context.Group.FirstOrDefault(g => g.GroupID == schedule.GroupID);

                if (course != null && group != null)
                {
                    notifications.Add(new NotificationItem
                    {
                        Time = DateTime.Now.ToString("HH:mm"),
                        Message = $"Через час занятие: {course.CourseName} - {group.GroupName}"
                    });
                }
            }

            // Получаем все активные контракты
            var contracts = context.Contract
                .Where(c => c.StatusContractID != 2 && c.StatusContractID != 3)
                .ToList();

            foreach (var contract in contracts)
            {
                var student = context.Student.FirstOrDefault(s => s.StudentID == contract.StudentID);
                var price = context.Price.FirstOrDefault(p => p.PriceID == contract.PriceID);

                if (student != null && price != null)
                {
                    // Сумма оплат по этому контракту
                    var paidAmount = context.Payment
                        .Where(p => p.ContractID == contract.ContractID)
                        .Sum(p => (int?)p.Quantity) ?? 0;

                    // Вычисляем задолженность
                    int debt = price.PriceQuantity.HasValue ? (int)(price.PriceQuantity.Value - paidAmount) : 0;

                    if (debt > 0)
                    {
                        notifications.Add(new NotificationItem
                        {
                            Time = DateTime.Now.ToString("HH:mm"),
                            Message = $"Задолженность: {student.LastName} {student.FirstName} - {debt:C}"
                        });
                    }
                }
            }
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskItem task)
            {
                var taskToUpdate = context.TeacherTask.Find(task.TaskID);
                if (taskToUpdate != null)
                {
                    taskToUpdate.StatusTask = true;
                    context.SaveChanges();
                    LoadTodayTasks();
                }
            }
        }

        public class GroupStatsItem
        {
            public string GroupName { get; set; }
            public string CourseName { get; set; }
            public string StudentCount { get; set; }
            public string Attendance { get; set; }
        }

        public class TeacherStatsItem
        {
            public string TeacherName { get; set; }
            public string CourseName { get; set; }
            public string Hours { get; set; }
            public string Groups { get; set; }
        }

        public class UpcomingClassItem
        {
            public string Time { get; set; }
            public string CourseName { get; set; }
            public string GroupName { get; set; }
            public string Cabinet { get; set; }
        }

        public class NotificationItem
        {
            public string Time { get; set; }
            public string Message { get; set; }
        }
    }
}
