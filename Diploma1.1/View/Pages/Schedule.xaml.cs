using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using System.Globalization;

namespace Diploma1._1.View.Pages
{
    public partial class Schedule : Page
    {
        // Строка подключения к базе данных
        private string connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=Dimploma1;Integrated Security=True";
        private DateTime currentMonth;
        private List<TimeTable> timesList;
        private List<ScheduleItem> allScheduleItems;
        private List<ScheduleItem> filteredScheduleItems;

        // Класс для хранения данных расписания
        public class TimeTable
        {
            public int TimeID { get; set; }
            public TimeSpan TimeStart { get; set; }
            public TimeSpan TimeEnd { get; set; }
        }

        public class ScheduleItem
        {
            public int ScheduleID { get; set; }
            public DateTime Date { get; set; }
            public int TimeID { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int CourseID { get; set; }
            public string CourseName { get; set; }
            public int GroupID { get; set; }
            public string GroupName { get; set; }
            public int CabinetID { get; set; }
            public string CabinetName { get; set; }
            public int TeacherID { get; set; }
            public string TeacherName { get; set; }
        }

        // Фильтры
        private int? selectedMonth;
        private int? selectedYear;
        private int? selectedCourseId;
        private int? selectedCabinetId;
        private int? selectedTeacherId;

        public DateTime ClassDate { get; internal set; }

        public Schedule()
        {
            InitializeComponent();
            currentMonth = DateTime.Now;
            LoadTimes();
            InitializeComboBoxes();
            LoadScheduleData();
            UpdateCalendarView();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация комбобоксов
            InitializeComboBoxes();

            // Загрузка данных расписания
            LoadScheduleData();
        }

        private void InitializeComboBoxes()
        {
            // Заполнение комбобокса месяцев
            var months = new List<KeyValuePair<int, string>>();
            for (int i = 1; i <= 12; i++)
            {
                months.Add(new KeyValuePair<int, string>(i, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)));
            }
            MonthComboBox.ItemsSource = months;
            MonthComboBox.DisplayMemberPath = "Value";
            MonthComboBox.SelectedValuePath = "Key";
            MonthComboBox.SelectedValue = currentMonth.Month;

            // Заполнение комбобокса годов
            var currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 1, 3).ToList();
            YearComboBox.ItemsSource = years;
            YearComboBox.SelectedItem = currentYear;

            // Загрузка курсов из БД
            LoadCourses();

            // Загрузка кабинетов из БД
            LoadCabinets();

            // Загрузка преподавателей из БД
            LoadTeachers();
        }

        private void LoadTimes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT TimeID, TimeStart, TimeEnd FROM Time ORDER BY TimeStart";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    timesList = new List<TimeTable>();
                    while (reader.Read())
                    {
                        timesList.Add(new TimeTable
                        {
                            TimeID = reader.GetInt32(0),
                            TimeStart = reader.GetTimeSpan(1),
                            TimeEnd = reader.GetTimeSpan(2)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке временных слотов: " + ex.Message);
            }
        }

        private void LoadCourses()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT CourseID, CourseName FROM Course WHERE IsActiveCourse = 1";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable coursesTable = new DataTable();
                    adapter.Fill(coursesTable);

                    CourseComboBox.ItemsSource = coursesTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке курсов: " + ex.Message);
            }
        }

        private void LoadCabinets()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT CabinetID, CabinetName FROM Cabinet";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable cabinetsTable = new DataTable();
                    adapter.Fill(cabinetsTable);

                    CabinetComboBox.ItemsSource = cabinetsTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке кабинетов: " + ex.Message);
            }
        }

        private void LoadTeachers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT EmployeeID, 
                                    LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName 
                                    FROM Employee 
                                    WHERE EmployeeRoleID = 2 AND IsActive = 1";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable teachersTable = new DataTable();
                    adapter.Fill(teachersTable);

                    TeacherComboBox.ItemsSource = teachersTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке преподавателей: " + ex.Message);
            }
        }

        private void LoadScheduleData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT 
                        s.ScheduleID,
                        s.ClassDate,
                        t.TimeID,
                        t.TimeStart,
                        t.TimeEnd,
                        c.CourseID,
                        c.CourseName,
                        g.GroupID,
                        g.GroupName,
                        cab.CabinetID,
                        cab.CabinetName,
                        e.EmployeeID,
                        e.LastName + ' ' + e.FirstName + ' ' + ISNULL(e.MiddleName, '') AS TeacherName
                    FROM 
                        Schedule s
                    JOIN 
                        Time t ON s.TimeID = t.TimeID
                    JOIN 
                        Course c ON s.CourseID = c.CourseID
                    LEFT JOIN 
                        [Group] g ON s.GroupID = g.GroupID
                    JOIN 
                        Cabinet cab ON s.CabinetID = cab.CabinetID
                    LEFT JOIN 
                        TeacherCourse tc ON c.CourseID = tc.CourseID
                    LEFT JOIN 
                        Employee e ON tc.TeacherID = e.EmployeeID
                    WHERE 
                        MONTH(s.ClassDate) = @Month AND YEAR(s.ClassDate) = @Year";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Month", currentMonth.Month);
                    command.Parameters.AddWithValue("@Year", currentMonth.Year);

                    SqlDataReader reader = command.ExecuteReader();

                    allScheduleItems = new List<ScheduleItem>();
                    while (reader.Read())
                    {
                        allScheduleItems.Add(new ScheduleItem
                        {
                            ScheduleID = reader.GetInt32(0),
                            Date = reader.GetDateTime(1),
                            TimeID = reader.GetInt32(2),
                            StartTime = reader.GetTimeSpan(3),
                            EndTime = reader.GetTimeSpan(4),
                            CourseID = reader.GetInt32(5),
                            CourseName = reader.GetString(6),
                            GroupID = reader.GetInt32(7),
                            GroupName = reader.GetString(8),
                            CabinetID = reader.GetInt32(9),
                            CabinetName = reader.GetString(10),
                            TeacherID = reader.GetInt32(11),
                            TeacherName = reader.GetString(12)
                        });
                    }

                    filteredScheduleItems = new List<ScheduleItem>(allScheduleItems);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке расписания: " + ex.Message);
            }
        }

        private void UpdateCalendarView()
        {
            // Обновляем текст с текущим месяцем
            CurrentMonthText.Text = $"Расписание на {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth.Month)} {currentMonth.Year}";

            // Очищаем существующую сетку
            ScheduleGrid.Children.Clear();
            ScheduleGrid.RowDefinitions.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();

            // Добавляем начальные определения строк и столбцов
            ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Создаем сетку с днями недели и временными интервалами
            CreateScheduleGrid();

            // Заполняем сетку данными занятий
            PopulateScheduleWithItems();
        }

        private void CreateScheduleGrid()
        {
            // Получаем первый день месяца
            var firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);

            // Получаем последний день месяца
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Создаем заголовки для дней недели
            var daysOfWeek = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

            // Пустая ячейка в верхнем левом углу
            var emptyHeader = new TextBlock
            {
                Text = "Время/День",
                Style = (Style)FindResource("ScheduleHeaderStyle")
            };
            Grid.SetRow(emptyHeader, 0);
            Grid.SetColumn(emptyHeader, 0);
            ScheduleGrid.Children.Add(emptyHeader);

            // Количество дней в месяце
            int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);

            // Добавляем колонки для каждого дня месяца
            for (int i = 0; i < daysInMonth; i++)
            {
                // Текущая дата
                DateTime currentDate = firstDay.AddDays(i);

                // Добавляем колонку
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                // Создаем заголовок
                var dayHeader = new TextBlock
                {
                    Text = $"{currentDate.Day} ({daysOfWeek[(int)currentDate.DayOfWeek == 0 ? 6 : (int)currentDate.DayOfWeek - 1]})",
                    Style = (Style)FindResource("ScheduleHeaderStyle")
                };

                // Если это выходной - меняем стиль
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    dayHeader.Background = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                }

                // Если это сегодня - выделяем
                if (currentDate.Date == DateTime.Today)
                {
                    dayHeader.Background = new SolidColorBrush(Color.FromRgb(70, 130, 180));
                    dayHeader.FontWeight = FontWeights.ExtraBold;
                }

                Grid.SetRow(dayHeader, 0);
                Grid.SetColumn(dayHeader, i + 1);
                ScheduleGrid.Children.Add(dayHeader);
            }

            // Добавляем строки для временных интервалов
            for (int i = 0; i < timesList.Count; i++)
            {
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

                var timeSlot = new TextBlock
                {
                    Text = $"{timesList[i].TimeStart.Hours:D2}:{timesList[i].TimeStart.Minutes:D2} - {timesList[i].TimeEnd.Hours:D2}:{timesList[i].TimeEnd.Minutes:D2}",
                    Style = (Style)FindResource("TimeSlotStyle")
                };

                Grid.SetRow(timeSlot, i + 1);
                Grid.SetColumn(timeSlot, 0);
                ScheduleGrid.Children.Add(timeSlot);
            }
        }

        private void PopulateScheduleWithItems()
        {
            foreach (var item in filteredScheduleItems)
            {
                int dayColumn = item.Date.Day;
                int timeIndex = timesList.FindIndex(t => t.TimeID == item.TimeID);

                if (timeIndex != -1)
                {
                    var scheduleItemElement = CreateScheduleItemElement(item);
                    Grid.SetRow(scheduleItemElement, timeIndex + 1);
                    Grid.SetColumn(scheduleItemElement, dayColumn);
                    ScheduleGrid.Children.Add(scheduleItemElement);
                }
            }
        }

        private UIElement CreateScheduleItemElement(ScheduleItem item)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ScheduleItemStyle"),
                Background = GetCourseColor(item.CourseID),
                Tag = item,
                Cursor = Cursors.Hand,
                Margin = new Thickness(2)
            };

            border.HorizontalAlignment = HorizontalAlignment.Stretch;
            border.VerticalAlignment = VerticalAlignment.Stretch;

            var content = new StackPanel { Margin = new Thickness(2) };

            content.Children.Add(new TextBlock
            {
                Text = item.CourseName,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            content.Children.Add(new TextBlock
            {
                Text = $"Группа: {item.GroupName}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 3, 0, 0)
            });

            content.Children.Add(new TextBlock
            {
                Text = $"Каб: {item.CabinetName}",
                TextWrapping = TextWrapping.Wrap
            });

            content.Children.Add(new TextBlock
            {
                Text = item.TeacherName,
                TextWrapping = TextWrapping.Wrap,
                FontStyle = FontStyles.Italic
            });

            border.Child = content;
            border.MouseLeftButtonDown += ScheduleItem_Click;

            return border;
        }

        private SolidColorBrush GetCourseColor(int courseId)
        {
            switch (courseId % 6)
            {
                case 0: return new SolidColorBrush(Color.FromRgb(230, 243, 255));
                case 1: return new SolidColorBrush(Color.FromRgb(230, 255, 230));
                case 2: return new SolidColorBrush(Color.FromRgb(255, 240, 230));
                case 3: return new SolidColorBrush(Color.FromRgb(255, 230, 245));
                case 4: return new SolidColorBrush(Color.FromRgb(245, 255, 230));
                case 5: return new SolidColorBrush(Color.FromRgb(230, 245, 255));
                default: return new SolidColorBrush(Color.FromRgb(240, 240, 240));
            }
        }

        private void ScheduleItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ScheduleItem item)
            {
                EditScheduleItem(item);
            }
        }

        private void EditScheduleItem(ScheduleItem item)
        {
            var editWindow = new Window
            {
                Title = item.ScheduleID == 0 ? "Добавление занятия" : "Редактирование занятия",
                Width = 450,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid
            {
                Margin = new Thickness(15)
            };

            // Определяем строки в сетке
            for (int i = 0; i < 16; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Заголовок
            var titleText = new TextBlock
            {
                Text = item.ScheduleID == 0 ? "Добавление занятия" : "Редактирование занятия",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(titleText, 0);
            grid.Children.Add(titleText);

            // Дата
            var dateLabel = new TextBlock { Text = "Дата:", Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(dateLabel, 1);
            grid.Children.Add(dateLabel);

            var datePicker = new DatePicker { SelectedDate = item.Date, Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(datePicker, 2);
            grid.Children.Add(datePicker);

            // Время
            var timeLabel = new TextBlock { Text = "Время:", Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(timeLabel, 3);
            grid.Children.Add(timeLabel);

            var timeComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                DisplayMemberPath = "DisplayText",
                SelectedValuePath = "TimeID"
            };

            var timeItems = timesList.Select(t => new
            {
                TimeID = t.TimeID,
                DisplayText = $"{t.TimeStart.Hours:D2}:{t.TimeStart.Minutes:D2} - {t.TimeEnd.Hours:D2}:{t.TimeEnd.Minutes:D2}"
            }).ToList();

            timeComboBox.ItemsSource = timeItems;
            timeComboBox.SelectedValue = item.TimeID;

            Grid.SetRow(timeComboBox, 4);
            grid.Children.Add(timeComboBox);

            // Курс
            var courseLabel = new TextBlock { Text = "Курс:", Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(courseLabel, 5);
            grid.Children.Add(courseLabel);

            var courseComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                DisplayMemberPath = "CourseName",
                SelectedValuePath = "CourseID"
            };

            // Группа
            var groupLabel = new TextBlock { Text = "Группа:", Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(groupLabel, 7);
            grid.Children.Add(groupLabel);

            var groupComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                DisplayMemberPath = "GroupName",
                SelectedValuePath = "GroupID"
            };

            // Кабинет
            var cabinetLabel = new TextBlock { Text = "Кабинет:", Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(cabinetLabel, 9);
            grid.Children.Add(cabinetLabel);

            var cabinetComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                DisplayMemberPath = "CabinetName",
                SelectedValuePath = "CabinetID"
            };

            // Преподаватель
            var teacherLabel = new TextBlock { Text = "Преподаватель:", Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(teacherLabel, 11);
            grid.Children.Add(teacherLabel);

            var teacherComboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                DisplayMemberPath = "FullName",
                SelectedValuePath = "UserID"
            };

            // Заполняем данные
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Курсы
                string courseQuery = "SELECT CourseID, CourseName FROM Course WHERE IsActiveCourse = 1";
                var courseCommand = new SqlCommand(courseQuery, connection);
                var courseAdapter = new SqlDataAdapter(courseCommand);
                var courseTable = new DataTable();
                courseAdapter.Fill(courseTable);
                courseComboBox.ItemsSource = courseTable.DefaultView;
                courseComboBox.SelectedValue = item.CourseID;

                // Группы
                string groupQuery = "SELECT GroupID, GroupName FROM [Group]";
                var groupCommand = new SqlCommand(groupQuery, connection);
                var groupAdapter = new SqlDataAdapter(groupCommand);
                var groupTable = new DataTable();
                groupAdapter.Fill(groupTable);
                groupComboBox.ItemsSource = groupTable.DefaultView;
                groupComboBox.SelectedValue = item.GroupID;

                // Кабинеты
                string cabinetQuery = "SELECT CabinetID, CabinetName FROM Cabinet";
                var cabinetCommand = new SqlCommand(cabinetQuery, connection);
                var cabinetAdapter = new SqlDataAdapter(cabinetCommand);
                var cabinetTable = new DataTable();
                cabinetAdapter.Fill(cabinetTable);
                cabinetComboBox.ItemsSource = cabinetTable.DefaultView;
                cabinetComboBox.SelectedValue = item.CabinetID;

                // Преподаватели
                string teacherQuery = @"SELECT EmployeeID, 
                                    LastName + ' ' + FirstName + ' ' + ISNULL(MiddleName, '') AS FullName 
                                    FROM Employee 
                                    WHERE EmployeeRoleID = 2 AND IsActive = 1";
                var teacherCommand = new SqlCommand(teacherQuery, connection);
                var teacherAdapter = new SqlDataAdapter(teacherCommand);
                var teacherTable = new DataTable();
                teacherAdapter.Fill(teacherTable);
                teacherComboBox.ItemsSource = teacherTable.DefaultView;
                teacherComboBox.SelectedValue = item.TeacherID;
            }

            Grid.SetRow(courseComboBox, 6);
            grid.Children.Add(courseComboBox);

            Grid.SetRow(groupComboBox, 8);
            grid.Children.Add(groupComboBox);

            Grid.SetRow(cabinetComboBox, 10);
            grid.Children.Add(cabinetComboBox);

            Grid.SetRow(teacherComboBox, 12);
            grid.Children.Add(teacherComboBox);

            // Кнопки
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            var saveButton = new Button
            {
                Content = "Сохранить",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(0, 0, 10, 0)
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Padding = new Thickness(15, 5, 15, 5)
            };

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);

            if (item.ScheduleID != 0)
            {
                var deleteButton = new Button
                {
                    Content = "Удалить",
                    Padding = new Thickness(15, 5, 15, 5),
                    Margin = new Thickness(10, 0, 0, 0),
                    Background = new SolidColorBrush(Colors.LightCoral)
                };

                buttonPanel.Children.Add(deleteButton);

                deleteButton.Click += (s, e) =>
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить это занятие?", "Подтверждение удаления",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DeleteScheduleItem(item.ScheduleID);
                        LoadScheduleData();
                        UpdateCalendarView();
                        editWindow.Close();
                    }
                };
            }

            Grid.SetRow(buttonPanel, 15);
            grid.Children.Add(buttonPanel);

            saveButton.Click += (s, e) =>
            {
                if (datePicker.SelectedDate.HasValue &&
                    timeComboBox.SelectedValue != null &&
                    courseComboBox.SelectedValue != null &&
                    groupComboBox.SelectedValue != null &&
                    cabinetComboBox.SelectedValue != null &&
                    teacherComboBox.SelectedValue != null)
                {
                    item.Date = datePicker.SelectedDate.Value;
                    item.TimeID = (int)timeComboBox.SelectedValue;
                    item.CourseID = (int)courseComboBox.SelectedValue;
                    item.GroupID = (int)groupComboBox.SelectedValue;
                    item.CabinetID = (int)cabinetComboBox.SelectedValue;
                    item.TeacherID = (int)teacherComboBox.SelectedValue;

                    SaveScheduleItem(item);
                    LoadScheduleData();
                    UpdateCalendarView();
                    editWindow.Close();
                }
                else
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            cancelButton.Click += (s, e) => editWindow.Close();

            editWindow.Content = grid;
            editWindow.ShowDialog();
        }

        private void SaveScheduleItem(ScheduleItem item)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = item.ScheduleID == 0 ?
                        @"INSERT INTO Schedule (ClassDate, TimeID, CourseID, GroupID, CabinetID, TeacherID)
                          VALUES (@ClassDate, @TimeID, @CourseID, @GroupID, @CabinetID, @TeacherID)" :
                        @"UPDATE Schedule 
                          SET ClassDate = @ClassDate,
                              TimeID = @TimeID,
                              CourseID = @CourseID,
                              GroupID = @GroupID,
                              CabinetID = @CabinetID,
                              TeacherID = @TeacherID
                          WHERE ScheduleID = @ScheduleID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClassDate", item.Date);
                    command.Parameters.AddWithValue("@TimeID", item.TimeID);
                    command.Parameters.AddWithValue("@CourseID", item.CourseID);
                    command.Parameters.AddWithValue("@GroupID", item.GroupID);
                    command.Parameters.AddWithValue("@CabinetID", item.CabinetID);
                    command.Parameters.AddWithValue("@TeacherID", item.TeacherID);

                    if (item.ScheduleID != 0)
                    {
                        command.Parameters.AddWithValue("@ScheduleID", item.ScheduleID);
                    }

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении занятия: " + ex.Message);
            }
        }

        private void DeleteScheduleItem(int scheduleId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Schedule WHERE ScheduleID = @ScheduleID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ScheduleID", scheduleId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении занятия: " + ex.Message);
            }
        }

        private void AddScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new ScheduleItem
            {
                Date = DateTime.Today,
                TimeID = timesList.FirstOrDefault()?.TimeID ?? 0
            };
            EditScheduleItem(newItem);
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            MonthComboBox.SelectedValue = currentMonth.Month;
            YearComboBox.SelectedItem = currentMonth.Year;
            LoadScheduleData();
            UpdateCalendarView();
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            MonthComboBox.SelectedValue = currentMonth.Month;
            YearComboBox.SelectedItem = currentMonth.Year;
            LoadScheduleData();
            UpdateCalendarView();
        }

        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && MonthComboBox.SelectedValue != null && YearComboBox.SelectedItem != null)
            {
                currentMonth = new DateTime((int)YearComboBox.SelectedItem, (int)MonthComboBox.SelectedValue, 1);
                LoadScheduleData();
                UpdateCalendarView();
            }
        }

        private void YearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && MonthComboBox.SelectedValue != null && YearComboBox.SelectedItem != null)
            {
                currentMonth = new DateTime((int)YearComboBox.SelectedItem, (int)MonthComboBox.SelectedValue, 1);
                LoadScheduleData();
                UpdateCalendarView();
            }
        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void CabinetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void TeacherComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            CourseComboBox.SelectedValue = null;
            CabinetComboBox.SelectedValue = null;
            TeacherComboBox.SelectedValue = null;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            filteredScheduleItems = new List<ScheduleItem>(allScheduleItems);

            if (CourseComboBox.SelectedValue != null)
            {
                int courseId = (int)CourseComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.CourseID == courseId).ToList();
            }

            if (CabinetComboBox.SelectedValue != null)
            {
                int cabinetId = (int)CabinetComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.CabinetID == cabinetId).ToList();
            }

            if (TeacherComboBox.SelectedValue != null)
            {
                int teacherId = (int)TeacherComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.TeacherID == teacherId).ToList();
            }

            UpdateCalendarView();
        }
    }
}
