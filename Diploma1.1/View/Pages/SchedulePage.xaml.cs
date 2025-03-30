using Diploma1._1.Model;
using Diploma1._1.View.CRUD;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для SchedulePage.xaml
    /// </summary>
    public partial class SchedulePage : Page
    {
        private DateTime currentMonth;
        private List<ScheduleItem> allScheduleItems;
        private List<ScheduleItem> filteredScheduleItems;
        private List<TimeTable> timesList;

        public SchedulePage()
        {
            InitializeComponent();
            currentMonth = DateTime.Now;
            InitializeComboBoxes();
            LoadScheduleData();
            UpdateCalendarView();
        }

        private void InitializeComboBoxes()
        {
            using (Dimploma1Entities context = new Dimploma1Entities())
            {
                // Заполняем комбобоксы месяцами
                var months = Enumerable.Range(1, 12)
                    .Select(m => new { Value = m, Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) })
                    .ToList();
                MonthComboBox.ItemsSource = months;
                MonthComboBox.DisplayMemberPath = "Name";
                MonthComboBox.SelectedValuePath = "Value";
                MonthComboBox.SelectedValue = DateTime.Now.Month;

                // Заполняем комбобоксы годами
                var currentYear = DateTime.Now.Year;
                var years = Enumerable.Range(currentYear - 1, 3).ToList();
                YearComboBox.ItemsSource = years;
                YearComboBox.SelectedValue = currentYear;

                // Заполняем комбобоксы курсами
                var courses = context.Course.ToList();
                CourseComboBox.ItemsSource = courses;
                CourseComboBox.DisplayMemberPath = "CourseName";
                CourseComboBox.SelectedValuePath = "CourseID";

                // Заполняем комбобоксы кабинетами
                var cabinets = context.Cabinet.ToList();
                CabinetComboBox.ItemsSource = cabinets;
                CabinetComboBox.DisplayMemberPath = "CabinetName";
                CabinetComboBox.SelectedValuePath = "CabinetID";

                // Заполняем комбобоксы преподавателями
                var usersListTeacher = (from user in context.Employee
                                        where user.EmployeeRoleID == 2
                                        select new UserFullName
                                        {
                                            UserID = user.EmployeeID,
                                            StatusID = (int)user.EmployeeRoleID,
                                            FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
                                        }).ToList();

                var usersTeacher = new ObservableCollection<UserFullName>(usersListTeacher);
                TeacherComboBox.ItemsSource = usersTeacher;
                TeacherComboBox.DisplayMemberPath = "FullName";
                TeacherComboBox.SelectedValuePath = "UserID";

                // Заполняем комбобоксы студентами
                var studentsList = (from user in context.Student
                                   select new UserFullName
                                   {
                                       UserID = user.StudentID,
                                       FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
                                   }).ToList();

                var students = new ObservableCollection<UserFullName>(studentsList);
                StudentComboBox.ItemsSource = students;
                StudentComboBox.DisplayMemberPath = "FullName";
                StudentComboBox.SelectedValuePath = "UserID";

                // Заполняем комбобоксы группами
                var groups = context.Group.ToList();
                GroupComboBox.ItemsSource = groups;
                GroupComboBox.DisplayMemberPath = "GroupName";
                GroupComboBox.SelectedValuePath = "GroupID";

                // Загружаем доступные временные слоты из базы данных
                timesList = context.Time.Select(t => new TimeTable
                {
                    TimeID = t.TimeID,
                    TimeStart = t.TimeStart.Value,
                    TimeEnd = t.TimeEnd.Value
                }).OrderBy(t => t.TimeStart).ToList();
            }
        }

        public void LoadScheduleData()
        {
            // Загружаем реальные данные из базы данных
            allScheduleItems = LoadRealScheduleData();
            filteredScheduleItems = new List<ScheduleItem>(allScheduleItems);
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
                Grid.SetColumn(dayHeader, i + 1); // +1 потому что первый столбец для времени
                ScheduleGrid.Children.Add(dayHeader);
            }

            // Добавляем строки для временных интервалов из базы данных
            for (int i = 0; i < timesList.Count; i++)
            {
                // Добавляем строку
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });

                // Создаем ячейку с временем
                var timeSlot = new TextBlock
                {
                    Text = $"{timesList[i].TimeStart.Hours:D2}:{timesList[i].TimeStart.Minutes:D2} - {timesList[i].TimeEnd.Hours:D2}:{timesList[i].TimeEnd.Minutes:D2}",
                    Style = (Style)FindResource("TimeSlotStyle")
                };

                Grid.SetRow(timeSlot, i + 1); // +1 потому что первая строка для заголовков
                Grid.SetColumn(timeSlot, 0);
                ScheduleGrid.Children.Add(timeSlot);
            }
        }

        private void PopulateScheduleWithItems()
        {
            // Фильтруем занятия по текущему месяцу
            var monthItems = filteredScheduleItems.Where(item =>
                item.Date.Month == currentMonth.Month &&
                item.Date.Year == currentMonth.Year).ToList();

            foreach (var item in monthItems)
            {
                // Определяем позицию занятия в сетке
                int dayColumn = item.Date.Day; // День месяца соответствует столбцу

                // Находим индекс временного слота
                int timeIndex = -1;
                for (int i = 0; i < timesList.Count; i++)
                {
                    if (timesList[i].TimeStart == item.StartTime)
                    {
                        timeIndex = i;
                        break;
                    }
                }

                if (timeIndex == -1) continue; // Если не нашли слот - пропускаем

                // Создаем элемент расписания
                var scheduleItemElement = CreateScheduleItemElement(item);

                // Устанавливаем позицию в сетке
                Grid.SetRow(scheduleItemElement, timeIndex + 1); // +1 потому что первая строка для заголовков
                Grid.SetColumn(scheduleItemElement, dayColumn); // День месяца (начиная с 1)

                // Добавляем элемент в сетку
                ScheduleGrid.Children.Add(scheduleItemElement);
            }
        }


        private void ScheduleItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ScheduleItem item)
            {
                // Открываем окно редактирования с данными выбранного элемента
                var editWindow = new EditScheduleItem(item.ScheduleID);
                editWindow.ShowDialog();
                LoadScheduleData();
                UpdateCalendarView();
            }
        }

        private SolidColorBrush GetCourseColor(int courseId)
        {
            // Генерируем цвет на основе ID курса
            // Это гарантирует, что один и тот же курс всегда будет иметь один и тот же цвет
            switch (courseId % 6)
            {
                case 0: return new SolidColorBrush(Color.FromRgb(230, 243, 255)); // Светло-голубой
                case 1: return new SolidColorBrush(Color.FromRgb(230, 255, 230)); // Светло-зеленый
                case 2: return new SolidColorBrush(Color.FromRgb(255, 240, 230)); // Светло-оранжевый
                case 3: return new SolidColorBrush(Color.FromRgb(255, 230, 245)); // Светло-розовый
                case 4: return new SolidColorBrush(Color.FromRgb(245, 255, 230)); // Светло-лаймовый
                case 5: return new SolidColorBrush(Color.FromRgb(230, 245, 255)); // Светло-небесный
                default: return new SolidColorBrush(Color.FromRgb(240, 240, 240)); // Светло-серый
            }
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Применяем фильтры к данным
            filteredScheduleItems = new List<ScheduleItem>(allScheduleItems);

            // Если выбран месяц и год, изменяем текущий месяц
            if (MonthComboBox.SelectedValue != null && YearComboBox.SelectedValue != null)
            {
                int month = (int)MonthComboBox.SelectedValue;
                int year = (int)YearComboBox.SelectedValue;
                currentMonth = new DateTime(year, month, 1);
            }

            // Применяем фильтр по курсу
            if (CourseComboBox.SelectedValue != null)
            {
                int courseId = (int)CourseComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.CourseID == courseId).ToList();
            }

            // Применяем фильтр по кабинету
            if (CabinetComboBox.SelectedValue != null)
            {
                int cabinetId = (int)CabinetComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.CabinetID == cabinetId).ToList();
            }

            // Применяем фильтр по преподавателю
            if (TeacherComboBox.SelectedValue != null)
            {
                int teacherId = (int)TeacherComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.TeacherID == teacherId).ToList();
            }

            // Применяем фильтр по студенту
            if (StudentComboBox.SelectedValue != null)
            {
                int studentId = (int)StudentComboBox.SelectedValue;
                using (var context = new Dimploma1Entities())
                {
                    var studentGroups = context.GroupStudent
                        .Where(sg => sg.StudentID == studentId)
                        .Select(sg => sg.GroupID)
                        .ToList();

                    filteredScheduleItems = filteredScheduleItems
                        .Where(item => studentGroups.Contains(item.GroupID))
                        .ToList();
                }
            }

            // Применяем фильтр по группе
            if (GroupComboBox.SelectedValue != null)
            {
                int groupId = (int)GroupComboBox.SelectedValue;
                filteredScheduleItems = filteredScheduleItems.Where(item => item.GroupID == groupId).ToList();
            }

            // Обновляем отображение
            UpdateCalendarView();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем фильтры
            MonthComboBox.SelectedValue = DateTime.Now.Month;
            YearComboBox.SelectedValue = DateTime.Now.Year;
            CourseComboBox.SelectedValue = null;
            CabinetComboBox.SelectedValue = null;
            TeacherComboBox.SelectedValue = null;
            StudentComboBox.SelectedValue = null;
            GroupComboBox.SelectedValue = null;

            // Сбрасываем текущий месяц на текущий
            currentMonth = DateTime.Now;

            // Сбрасываем фильтрованный список
            filteredScheduleItems = new List<ScheduleItem>(allScheduleItems);

            // Обновляем отображение
            UpdateCalendarView();
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            // Переходим к предыдущему месяцу
            currentMonth = currentMonth.AddMonths(-1);

            // Обновляем комбобоксы месяца и года
            MonthComboBox.SelectedValue = currentMonth.Month;
            YearComboBox.SelectedValue = currentMonth.Year;

            // Обновляем отображение
            UpdateCalendarView();
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            // Переходим к следующему месяцу
            currentMonth = currentMonth.AddMonths(1);

            // Обновляем комбобоксы месяца и года
            MonthComboBox.SelectedValue = currentMonth.Month;
            YearComboBox.SelectedValue = currentMonth.Year;

            // Обновляем отображение
            UpdateCalendarView();
        }

        // Метод для загрузки реальных данных расписания из базы данных
        private List<ScheduleItem> LoadRealScheduleData()
        {
            List<ScheduleItem> scheduleItems = new List<ScheduleItem>();

            using (var context = new Dimploma1Entities())
            {
                // Загружаем данные из базы
                var scheduleEntries = (from s in context.Schedule
                                       join c in context.Course on s.CourseID equals c.CourseID
                                       join cab in context.Cabinet on s.CabinetID equals cab.CabinetID
                                       join emp in context.Employee on s.TeacherID equals emp.EmployeeID
                                       join t in context.Time on s.TimeID equals t.TimeID
                                       join g in context.Group on s.GroupID equals g.GroupID
                                       select new
                                       {
                                           ScheduleID = s.ScheduleID,
                                           ClassDate = s.ClassDate,
                                           TimeID = t.TimeID,
                                           TimeStart = t.TimeStart,
                                           TimeEnd = t.TimeEnd,
                                           CourseID = c.CourseID,
                                           CourseName = c.CourseName,
                                           GroupName = g.GroupName,
                                           GroupID = g.GroupID,
                                           CabinetID = cab.CabinetID,
                                           CabinetName = cab.CabinetName,
                                           TeacherID = emp.EmployeeID,
                                           TeacherName = emp.LastName + " " + emp.FirstName + " " + emp.MiddleName
                                       }).ToList();

                // Преобразуем данные в нашу модель ScheduleItem
                foreach (var entry in scheduleEntries)
                {
                    var scheduleItem = new ScheduleItem
                    {
                        ScheduleID = entry.ScheduleID,
                        Date = entry.ClassDate.Value,
                        TimeID = entry.TimeID,
                        StartTime = entry.TimeStart.Value,
                        EndTime = entry.TimeEnd.Value,
                        CourseID = entry.CourseID,
                        CourseName = entry.CourseName,
                        GroupID = entry.GroupID,
                        GroupName = entry.GroupName,
                        CabinetID = entry.CabinetID,
                        CabinetName = entry.CabinetName,
                        TeacherID = entry.TeacherID,
                        TeacherName = entry.TeacherName
                    };

                    scheduleItems.Add(scheduleItem);
                }
            }

            return scheduleItems;
        }

        private void SaveScheduleItem(ScheduleItem item)
        {
            using (var context = new Dimploma1Entities())
            {
                // Проверяем, существует ли уже такая запись
                var existingItem = context.Schedule.FirstOrDefault(s => s.ScheduleID == item.ScheduleID);

                if (existingItem != null)
                {
                    // Обновляем существующую запись
                    existingItem.ClassDate = item.Date;
                    existingItem.TimeID = item.TimeID;
                    existingItem.CourseID = item.CourseID;
                    existingItem.CabinetID = item.CabinetID;
                    existingItem.TeacherID = item.TeacherID;
                    existingItem.GroupID = item.GroupID;
                }
                else
                {
                    // Создаем новую запись
                    var newSchedule = new Diploma1._1.Schedule
                    {
                        ClassDate = item.Date,
                        TimeID = item.TimeID,
                        CourseID = item.CourseID,
                        CabinetID = item.CabinetID,
                        TeacherID = item.TeacherID,
                        GroupID = item.GroupID
                    };

                    context.Schedule.Add(newSchedule);
                }

                context.SaveChanges();
            }
        }

        // Метод для удаления записи расписания
        private void DeleteScheduleItem(int scheduleId)
        {
            using (var context = new Dimploma1Entities())
            {
                var itemToDelete = context.Schedule.FirstOrDefault(s => s.ScheduleID == scheduleId);
                if (itemToDelete != null)
                {
                    context.Schedule.Remove(itemToDelete);
                    context.SaveChanges();
                }
            }
        }
        // Добавление нового занятия
        private void AddScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно для добавления нового расписания
            var editWindow = new EditScheduleItem();
            editWindow.ShowDialog();
            LoadScheduleData();
            UpdateCalendarView();
        }

        private UIElement CreateScheduleItemElement(ScheduleItem item)
        {
            // Создаем контейнер для элемента расписания
            var border = new Border
            {
                Style = (Style)FindResource("ScheduleItemStyle"),
                Background = GetCourseColor(item.CourseID),
                Tag = item, // Сохраняем данные элемента в Tag для последующего редактирования
                Cursor = Cursors.Hand // Указываем, что элемент можно кликнуть
            };

            // Добавляем обработчик клика для редактирования
            border.MouseLeftButtonDown += (sender, e) => {
                var editWindow = new EditScheduleItem(item.ScheduleID);
                editWindow.ShowDialog();
                LoadScheduleData();
                UpdateCalendarView();
            };

            // Создаем контейнер для содержимого
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(5)
            };

            // Добавляем название курса
            var courseText = new TextBlock
            {
                Text = item.CourseName,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(courseText);

            // Добавляем название группы
            var groupText = new TextBlock
            {
                Text = $"Группа: {item.GroupName}",
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(groupText);

            // Добавляем кабинет
            var cabinetText = new TextBlock
            {
                Text = $"Кабинет: {item.CabinetName}",
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(cabinetText);

            // Добавляем преподавателя
            var teacherText = new TextBlock
            {
                Text = $"Преподаватель: {item.TeacherName}",
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(teacherText);

            // Устанавливаем содержимое для border
            border.Child = stackPanel;

            return border;
        }

        //private void EditScheduleItem(ScheduleItem item)
        //{
        //    // Создаем диалоговое окно для редактирования
        //    var editWindow = new Window
        //    {
        //        Title = item.ScheduleID == 0 ? "Добавление занятия" : "Редактирование занятия",
        //        Width = 450,
        //        Height = 600,
        //        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        //        ResizeMode = ResizeMode.NoResize
        //    };

        //    var grid = new Grid
        //    {
        //        Margin = new Thickness(15)
        //    };

        //    // Определяем строки в сетке
        //    for (int i = 0; i < 16; i++)
        //    {
        //        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        //    }
        //    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        //    // Заголовок
        //    var titleText = new TextBlock
        //    {
        //        Text = item.ScheduleID == 0 ? "Добавление занятия" : "Редактирование занятия",
        //        FontSize = 18,
        //        FontWeight = FontWeights.Bold,
        //        Margin = new Thickness(0, 0, 0, 15)
        //    };
        //    Grid.SetRow(titleText, 0);
        //    grid.Children.Add(titleText);

        //    // Дата
        //    var dateLabel = new TextBlock { Text = "Дата:", Margin = new Thickness(0, 5, 0, 5) };
        //    Grid.SetRow(dateLabel, 1);
        //    grid.Children.Add(dateLabel);

        //    var datePicker = new DatePicker { SelectedDate = item.Date, Margin = new Thickness(0, 0, 0, 10) };
        //    Grid.SetRow(datePicker, 2);
        //    grid.Children.Add(datePicker);

        //    // Время
        //    var timeLabel = new TextBlock { Text = "Время:", Margin = new Thickness(0, 5, 0, 5) };
        //    Grid.SetRow(timeLabel, 3);
        //    grid.Children.Add(timeLabel);

        //    var timeComboBox = new ComboBox
        //    {
        //        Margin = new Thickness(0, 0, 0, 10),
        //        DisplayMemberPath = "DisplayText",
        //        SelectedValuePath = "TimeID"
        //    };

        // Заполняем комбобокс временными слотами из базы
        //var timeItems = timesList.Select(t => new
        //{
        //    TimeID = t.TimeID,
        //    DisplayText = $"{t.TimeStart.Hours:D2}:{t.TimeStart.Minutes:D2} - {t.TimeEnd.Hours:D2}:{t.TimeEnd.Minutes:D2}"
        //}).ToList();

        //timeComboBox.ItemsSource = timeItems;

        //    // Выбираем текущее время занятия
        //    var currentTimeId = timesList.FirstOrDefault(t => t.TimeStart == item.StartTime)?.TimeID;
        //    if (currentTimeId.HasValue)
        //    {
        //        timeComboBox.SelectedValue = currentTimeId.Value;
        //    }
        //    else if (timeItems.Any())
        //    {
        //        // Если это новое занятие, выбираем первый доступный слот
        //        timeComboBox.SelectedIndex = 0;
        //    }

        //    Grid.SetRow(timeComboBox, 4);
        //    grid.Children.Add(timeComboBox);

        //    // Курс
        //    var courseLabel = new TextBlock { Text = "Курс:", Margin = new Thickness(0, 5, 0, 5) };
        //    Grid.SetRow(courseLabel, 5);
        //    grid.Children.Add(courseLabel);

        //    var courseComboBox = new ComboBox
        //    {
        //        Margin = new Thickness(0, 0, 0, 10),
        //        DisplayMemberPath = "CourseName",
        //        SelectedValuePath = "CourseID"
        //    };

        //    // Группа
        //    var groupLabel = new TextBlock { Text = "Группа:", Margin = new Thickness(0, 5, 0, 5) };
        //    Grid.SetRow(groupLabel, 7);
        //    grid.Children.Add(groupLabel);

        //    var groupComboBox = new ComboBox
        //    {
        //        Margin = new Thickness(0, 0, 0, 10),
        //        DisplayMemberPath = "GroupName",
        //        SelectedValuePath = "GroupID"
        //    };

        //    // Кабинет
        //    var cabinetLabel = new TextBlock { Text = "Кабинет:", Margin = new Thickness(0, 5, 0, 5) };
        //    Grid.SetRow(cabinetLabel, 9);
        //    grid.Children.Add(cabinetLabel);

        //    var cabinetComboBox = new ComboBox
        //    {
        //        Margin = new Thickness(0, 0, 0, 10),
        //        DisplayMemberPath = "CabinetName",
        //        SelectedValuePath = "CabinetID"
        //    };

        //    // Преподаватель
        //    var teacherLabel = new TextBlock { Text = "Преподаватель:", Margin = new Thickness(0, 5, 0, 5) };
        //    Grid.SetRow(teacherLabel, 11);
        //    grid.Children.Add(teacherLabel);

        //    var teacherComboBox = new ComboBox
        //    {
        //        Margin = new Thickness(0, 0, 0, 10),
        //        DisplayMemberPath = "FullName",
        //        SelectedValuePath = "UserID"
        //    };

        //    // Заполняем данные
        //    using (var context = new Dimploma1Entities())
        //    {
        //        courseComboBox.ItemsSource = context.Course.ToList();
        //        courseComboBox.SelectedValue = item.CourseID;

        //        groupComboBox.ItemsSource = context.Group.ToList();
        //        groupComboBox.SelectedValue = item.GroupID;

        //        cabinetComboBox.ItemsSource = context.Cabinet.ToList();
        //        cabinetComboBox.SelectedValue = item.CabinetID;

        //        // Преподаватели (сотрудники с ролью 2)
        //        var usersListTeacher = (from user in context.Employee
        //                                where user.EmployeeRoleID == 2
        //                                select new UserFullName
        //                                {
        //                                    UserID = user.EmployeeID,
        //                                    StatusID = (int)user.EmployeeRoleID,
        //                                    FullName = user.LastName + " " + user.FirstName + " " + user.MiddleName
        //                                }).ToList();

        //        teacherComboBox.ItemsSource = usersListTeacher;
        //        teacherComboBox.SelectedValue = item.TeacherID;
        //    }

        //    Grid.SetRow(courseComboBox, 6);
        //    grid.Children.Add(courseComboBox);

        //    Grid.SetRow(groupComboBox, 8);
        //    grid.Children.Add(groupComboBox);

        //    Grid.SetRow(cabinetComboBox, 10);
        //    grid.Children.Add(cabinetComboBox);

        //    Grid.SetRow(teacherComboBox, 12);
        //    grid.Children.Add(teacherComboBox);

        //    // Кнопки
        //    var buttonPanel = new StackPanel
        //    {
        //        Orientation = Orientation.Horizontal,
        //        HorizontalAlignment = HorizontalAlignment.Right,
        //        Margin = new Thickness(0, 15, 0, 0)
        //    };

        //    var saveButton = new Button
        //    {
        //        Content = "Сохранить",
        //        Padding = new Thickness(15, 5, 15, 5),
        //        Margin = new Thickness(0, 0, 10, 0)
        //    };

        //    var cancelButton = new Button
        //    {
        //        Content = "Отмена",
        //        Padding = new Thickness(15, 5, 15, 5)
        //    };

        //    buttonPanel.Children.Add(saveButton);
        //    buttonPanel.Children.Add(cancelButton);

        //    // Если это существующее занятие, добавляем кнопку удаления
        //    if (item.ScheduleID != 0)
        //    {
        //        var deleteButton = new Button
        //        {
        //            Content = "Удалить",
        //            Padding = new Thickness(15, 5, 15, 5),
        //            Margin = new Thickness(10, 0, 0, 0),
        //            Background = new SolidColorBrush(Colors.LightCoral)
        //        };

        //        buttonPanel.Children.Add(deleteButton);

        //        // Обработчик для кнопки удаления
        //        deleteButton.Click += (s, e) =>
        //        {
        //            var result = MessageBox.Show("Вы уверены, что хотите удалить это занятие?", "Подтверждение удаления",
        //                MessageBoxButton.YesNo, MessageBoxImage.Question);

        //            if (result == MessageBoxResult.Yes)
        //            {
        //                // Удаляем из базы
        //                DeleteScheduleItem(item.ScheduleID);

        //                // Обновляем отображение
        //                LoadScheduleData();
        //                UpdateCalendarView();

        //                // Закрываем окно
        //                editWindow.Close();
        //            }
        //        };
        //    }

        //    Grid.SetRow(buttonPanel, 15);
        //    grid.Children.Add(buttonPanel);

        //    // Обработчики событий для кнопок
        //    saveButton.Click += (s, e) =>
        //    {
        //        // Проверяем заполнение всех обязательных полей
        //        if (datePicker.SelectedDate.HasValue &&
        //            timeComboBox.SelectedValue != null &&
        //            courseComboBox.SelectedValue != null &&
        //            groupComboBox.SelectedValue != null &&
        //            cabinetComboBox.SelectedValue != null &&
        //            teacherComboBox.SelectedValue != null)
        //        {
        //            // Обновляем данные элемента
        //            item.Date = datePicker.SelectedDate.Value;

        //            // Получаем выбранное время
        //            int selectedTimeId = (int)timeComboBox.SelectedValue;
        //            var selectedTime = timesList.FirstOrDefault(t => t.TimeID == selectedTimeId);

        //            if (selectedTime != null)
        //            {
        //                item.TimeID = selectedTimeId;
        //                item.StartTime = selectedTime.TimeStart;
        //                item.EndTime = selectedTime.TimeEnd;
        //            }

        //            item.CourseID = (int)courseComboBox.SelectedValue;
        //            item.CourseName = (courseComboBox.SelectedItem as Course)?.CourseName;

        //            item.GroupID = (int)groupComboBox.SelectedValue;
        //            item.GroupName = (groupComboBox.SelectedItem as Group)?.GroupName;

        //            item.CabinetID = (int)cabinetComboBox.SelectedValue;
        //            item.CabinetName = (cabinetComboBox.SelectedItem as Cabinet)?.CabinetName;

        //            item.TeacherID = (int)teacherComboBox.SelectedValue;
        //            item.TeacherName = (teacherComboBox.SelectedItem as UserFullName)?.FullName;

        //            // Сохраняем изменения в базу
        //            SaveScheduleItem(item);

        //            // Обновляем отображение
        //            LoadScheduleData();
        //            UpdateCalendarView();

        //            // Закрываем окно
        //            editWindow.Close();
        //        }
        //        else
        //        {
        //            MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    };

        //    cancelButton.Click += (s, e) => editWindow.Close();

        //    editWindow.Content = grid;
        //    editWindow.ShowDialog();
        //}
        
    }

    // Вспомогательные классы для работы с данными
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
}