using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO; // Для StreamWriter
using System.Linq;
using System.Text; // Для Encoding
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // Для Binding
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32; // Для SaveFileDialog
// Замените на ваше актуальное пространство имен модели EF
//using YourAppNamespace.Data; // <--- ЗАМЕНИТЕ НА ВАШ NAMESPACE МОДЕЛИ EF
// Возможно, понадобится using для вашей библиотеки графиков, например:
// using LiveCharts;
// using LiveCharts.Wpf;

namespace Diploma1._1.View.Pages // Пространство имен из XAML
{
    /// <summary>
    /// Логика взаимодействия для Reports.xaml
    /// </summary>
    public partial class Reports : Page // Имя класса и тип из XAML
    {
        // Замените Dimploma1Entities на актуальное имя вашего класса контекста EF
        private readonly Dimploma1Entities context; // <--- Убедитесь, что имя класса контекста верное

        public Reports()
        {
            InitializeComponent(); // Обязательно первым делом
            // Замените Dimploma1Entities на актуальное имя вашего класса контекста EF
            context = new Dimploma1Entities(); // <--- Убедитесь, что имя класса контекста верное
            InitializeDatePickers();
        }

        private void InitializeDatePickers()
        {
            // Инициализация дат для обоих наборов DatePicker
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Today;
            StatStartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            StatEndDatePicker.SelectedDate = DateTime.Today;
        }

        // --- Обработчики событий ---

        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Генерируем отчет при выборе типа, если включена автогенерация (или можно убрать автогенерацию)
            // if (ReportTypeComboBox.SelectedItem is ComboBoxItem)
            // {
            //     GenerateReport();
            // }
        }

        private void StatisticsTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Аналогично для статистики
            // if (StatisticsTypeComboBox.SelectedItem is ComboBoxItem)
            // {
            //     GenerateStatistics();
            // }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void GenerateStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateStatistics();
        }

        private void ExportReportButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем экспорт в CSV для таблицы отчетов
            ExportToCsv(ReportDataGrid, "report");
        }

        private void ExportStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем экспорт в CSV для таблицы статистики
            ExportToCsv(StatisticsDataGrid, "statistics");
        }

        // --- Логика генерации отчетов ---

        private void GenerateReport()
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите период для отчета.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!(ReportTypeComboBox.SelectedItem is ComboBoxItem selectedItem))
            {
                MessageBox.Show("Пожалуйста, выберите тип отчета.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value.Date;
            var endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1); // Конец дня

            try // Обернем получение данных в try-catch
            {
                Mouse.OverrideCursor = Cursors.Wait; // Показываем курсор ожидания

                switch (selectedItem.Content.ToString())
                {
                    case "Отчет по расписанию занятий":
                        GenerateScheduleReport(startDate, endDate);
                        break;
                    case "Отчет по посещаемости":
                        GenerateAttendanceReport(startDate, endDate);
                        break;
                    case "Отчет по договорам":
                        GenerateContractsReport(startDate, endDate);
                        break;
                    // --- Добавлены заглушки для новых типов отчетов из XAML ---
                    case "Отчет по оплатам":
                        GeneratePaymentsReport(startDate, endDate);
                        break;
                    case "Отчет по преподавателям":
                        GenerateTeachersReport(startDate, endDate);
                        break;
                    case "Отчет по учебным группам":
                        GenerateGroupsReport(startDate, endDate);
                        break;
                    case "Отчет по использованию аудиторий":
                        GenerateClassroomUsageReport(startDate, endDate);
                        break;
                    case "Итоговый отчет за учебный период":
                        GenerateOverallPeriodReport(startDate, endDate);
                        break;
                    default:
                        MessageBox.Show($"Тип отчета '{selectedItem.Content}' не распознан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        ReportDataGrid.ItemsSource = null; // Очищаем таблицу
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при формировании отчета:\n{ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                ReportDataGrid.ItemsSource = null; // Очищаем таблицу в случае ошибки
            }
            finally
            {
                Mouse.OverrideCursor = null; // Возвращаем обычный курсор
            }
        }

        // --- Логика генерации статистики ---

        private void GenerateStatistics()
        {
            if (StatStartDatePicker.SelectedDate == null || StatEndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите период для статистики.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!(StatisticsTypeComboBox.SelectedItem is ComboBoxItem selectedItem))
            {
                MessageBox.Show("Пожалуйста, выберите тип статистики.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = StatStartDatePicker.SelectedDate.Value.Date;
            var endDate = StatEndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try // Обернем получение данных в try-catch
            {
                Mouse.OverrideCursor = Cursors.Wait;

                switch (selectedItem.Content.ToString())
                {
                    case "Статистика по количеству обучающихся":
                        GenerateStudentCountStatistics(startDate, endDate);
                        break;
                    case "Статистика по популярным курсам":
                        GeneratePopularCoursesStatistics(startDate, endDate);
                        break;
                    // --- Добавлены заглушки для новых типов статистики из XAML ---
                    case "Статистика по завершению курсов":
                        GenerateCourseCompletionStatistics(startDate, endDate);
                        break;
                    case "Статистика по возрасту":
                        GenerateAgeStatistics(startDate, endDate);
                        break;
                    case "Статистика по нагрузке преподавателей":
                        GenerateTeacherLoadStatistics(startDate, endDate);
                        break;
                    case "Статистика по эффективности рекламы":
                        GenerateAdEffectivenessStatistics(startDate, endDate);
                        break;
                    case "Статистика по дистанционному и очному обучению":
                        GenerateStudyFormatStatistics(startDate, endDate);
                        break;
                    default:
                        MessageBox.Show($"Тип статистики '{selectedItem.Content}' не распознан.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        StatisticsDataGrid.ItemsSource = null; // Очищаем таблицу
                        ChartContainer.Content = null; // Очищаем график
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при формировании статистики:\n{ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                StatisticsDataGrid.ItemsSource = null; // Очищаем таблицу
                ChartContainer.Content = null; // Очищаем график
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // --- Реализации методов генерации (существующие + заглушки) ---

        // Отчет по расписанию (исправленный)
        private void GenerateScheduleReport(DateTime startDate, DateTime endDate)
        {
            // Используем .Date для сравнения только дат, без времени
            var exactStartDate = startDate.Date;
            var exactEndDate = endDate.Date; // Конечная дата включительно

            var scheduleData = context.Schedule
                // Фильтруем по ClassDate в выбранном диапазоне дат
                .Where(s => s.ClassDate >= exactStartDate && s.ClassDate <= exactEndDate)
                // Включаем связанные сущности, которые ЕСТЬ в схеме Schedule
                .Include(s => s.Time)     // Используем лямбда-выражения для Include (рекомендуется)
                .Include(s => s.Group)
                .Include(s => s.Course)
                .Include(s => s.Cabinet)
                // НЕ включаем Teacher и DayOfWeek, т.к. их нет в Schedule
                .Select(s => new // Анонимный тип для DataGrid
                {
                    // Используем ClassDate
                    Дата = s.ClassDate,

                    // Исправляем форматирование времени TimeSpan
                    // Используем безопасный доступ и стандартный/пользовательский формат
                    Время = s.Time != null
                               ? $"{s.Time.TimeStart:hh\\:mm} - {s.Time.TimeEnd:hh\\:mm}" // Формат ЧЧ:ММ - ЧЧ:ММ
                               : "Не указано",

                    Группа = s.Group != null ? s.Group.GroupName : "Не указана",
                    Курс = s.Course != null ? s.Course.CourseName : "Не указан",

                    // Поле Преподаватель удалено, так как TeacherID отсутствует в таблице Schedule.
                    // Если нужно добавить, его надо получать из другого места (например, через Группу, если там есть связь)
                    // Преподаватель = s.Group != null && s.Group.Teacher != null ? (s.Group.Teacher.LastName + " " + s.Group.Teacher.FirstName) : "Не указан", // Пример, если учитель связан с группой

                    Аудитория = s.Cabinet != null ? s.Cabinet.CabinetName : "Не указана", // Проверьте, что поле называется CabinetName

                    // Получаем название дня недели из ClassDate
                    ДеньНедели = s.ClassDate != null
                                    ? CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(s.ClassDate.Value.DayOfWeek)
                                    : "Не указан"
                })
                .OrderBy(s => s.Дата) // Сортируем по дате
                .ThenBy(s => s.Время) // Затем по времени (строковое представление)
                .ToList(); // Материализуем запрос

            ReportDataGrid.ItemsSource = scheduleData;
        }

        // Отчет по посещаемости
        private void GenerateAttendanceReport(DateTime startDate, DateTime endDate)
        {
            // Предполагаем, что нужна посещаемость занятий, которые *начались* в этот период
            var attendanceData = context.Attendance
                .Include("Schedule") // Включаем связанные данные
                .Include("Student.Client") // Student и связанный Client
                .Include("Schedule.Group")
                .Include("Schedule.Course")
                .Include("StatusAttendance")
               .Where(a => a.Schedule != null && a.Schedule.ClassDate >= startDate && a.Schedule.ClassDate <= endDate)
               .Select(a => new
               {
                   ДатаЗанятия = a.Schedule.ClassDate, // Дата начала занятия из расписания
                   Студент = (a.Student != null && a.Student.Client != null) ? (a.Student.Client.LastName + " " + a.Student.Client.FirstName) : "Н/Д",
                   Группа = (a.Schedule != null && a.Schedule.Group != null) ? a.Schedule.Group.GroupName : "Н/Д",
                   Курс = (a.Schedule != null && a.Schedule.Course != null) ? a.Schedule.Course.CourseName : "Н/Д",
                   Статус = a.StatusAttendance != null ? a.StatusAttendance.StatusAttendanceName : "Н/Д" // StatusAttendanceName? Проверьте поле
               })
               .OrderBy(a => a.ДатаЗанятия)
               .ThenBy(a => a.Студент)
               .ToList();

            ReportDataGrid.ItemsSource = attendanceData;
        }

        // Отчет по договорам
        private void GenerateContractsReport(DateTime startDate, DateTime endDate)
        {
            var contractsData = context.Contract
                .Include("Client") // Включаем связанные данные
                .Include("Student.Client")
                .Include("Price")
                .Include("FormOfStudy")
                .Include("StatusContract")
               .Where(c => c.DateCreate >= startDate && c.DateCreate <= endDate) // По дате создания
               .Select(c => new
               {
                   Номер = c.ContractID,
                   ДатаСоздания = c.DateCreate,
                   ДатаНачала = c.DateStart,
                   ДатаОкончания = c.DateEnd,
                   Клиент = (c.Client != null) ? (c.Client.LastName + " " + c.Client.FirstName) : "Н/Д",
                   Студент = (c.Student != null && c.Student.Client != null) ? (c.Student.Client.LastName + " " + c.Student.Client.FirstName) : "Н/Д",
                   Стоимость = c.Price != null ? c.Price.PriceQuantity : (decimal?)null, // CostTotal? Проверьте поле
                   ФормаОбучения = c.FormOfStudy != null ? c.FormOfStudy.FormOfStudyName : "Н/Д", // FormOfStudyName? Проверьте поле
                   Статус = c.StatusContract != null ? c.StatusContract.StatusContractName : "Н/Д" // StatusContractName? Проверьте поле
               })
               .OrderBy(c => c.ДатаСоздания)
               .ToList();

            ReportDataGrid.ItemsSource = contractsData;
        }

        // --- Заглушки для новых отчетов ---
        private void GeneratePaymentsReport(DateTime startDate, DateTime endDate) { ReportDataGrid.ItemsSource = null; MessageBox.Show("Отчет по оплатам еще не реализован."); }
        private void GenerateTeachersReport(DateTime startDate, DateTime endDate) { ReportDataGrid.ItemsSource = null; MessageBox.Show("Отчет по преподавателям еще не реализован."); }
        private void GenerateGroupsReport(DateTime startDate, DateTime endDate) { ReportDataGrid.ItemsSource = null; MessageBox.Show("Отчет по учебным группам еще не реализован."); }
        private void GenerateClassroomUsageReport(DateTime startDate, DateTime endDate) { ReportDataGrid.ItemsSource = null; MessageBox.Show("Отчет по использованию аудиторий еще не реализован."); }
        private void GenerateOverallPeriodReport(DateTime startDate, DateTime endDate) { ReportDataGrid.ItemsSource = null; MessageBox.Show("Итоговый отчет за учебный период еще не реализован."); }


        // Статистика по количеству студентов (по дате начала контракта)
        private void GenerateStudentCountStatistics(DateTime startDate, DateTime endDate)
        {
            var studentStats = context.Contract
               .Where(c => c.DateStart >= startDate && c.DateStart <= endDate && c.StudentID != null)
               .GroupBy(c => c.DateStart.Value.Month)
               .Select(g => new { MonthKey = g.Key, Count = g.Select(c => c.StudentID).Distinct().Count() }) // Считаем уникальных студентов
                .ToList() // Материализуем перед получением имени месяца
                .Select(g => new
                {
                    Месяц = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.MonthKey),
                    КоличествоСтудентов = g.Count,
                    MonthSortKey = g.MonthKey // Добавляем ключ для сортировки
                })
               .OrderBy(s => s.MonthSortKey)
               .ToList();

            StatisticsDataGrid.ItemsSource = studentStats;

            if (studentStats.Any())
            {
                GenerateChart(studentStats.Select(s => s.Месяц).ToList(),
                               studentStats.Select(s => (double)s.КоличествоСтудентов).ToList(),
                               "Новые студенты по месяцам");
            }
            else
            {
                GenerateChart(new List<string>(), new List<double>(), "Новые студенты по месяцам (нет данных)");
            }
        }

        // Статистика по популярным курсам (по студентам с контрактами, начавшимися в период)
        private void GeneratePopularCoursesStatistics(DateTime startDate, DateTime endDate)
        {
            // Получаем ID студентов, чьи контракты начались в период
            var studentIdsInPeriod = context.Contract
               .Where(c => c.DateStart >= startDate && c.DateStart <= endDate && c.StudentID != null)
               .Select(c => c.StudentID)
               .Distinct()
               .ToList();

            if (!studentIdsInPeriod.Any())
            {
                StatisticsDataGrid.ItemsSource = null;
                GenerateChart(new List<string>(), new List<double>(), "Популярность курсов (нет данных)");
                return;
            }

            // Ищем курсы для этих студентов
            var courseStats = context.GroupStudent
                .Where(gs => studentIdsInPeriod.Contains(gs.StudentID)) // Фильтруем по студентам из периода
                .Include(gs => gs.Group.Course) // Включаем Group и Course
                .Where(gs => gs.Group != null && gs.Group.Course != null) // Убеждаемся, что связи есть
                .GroupBy(gs => gs.Group.Course) // Группируем по объекту Course
                .Select(g => new
                {
                    Курс = g.Key.CourseName, // Название курса из ключа
                                             // Считаем уникальных студентов *из периода*, которые есть в этой группе курса
                    КоличествоСтудентов = g.Select(gs => gs.StudentID).Distinct().Count(sid => studentIdsInPeriod.Contains(sid))
                })
                .Where(s => s.КоличествоСтудентов > 0)
                .OrderByDescending(s => s.КоличествоСтудентов)
                .ToList();


            StatisticsDataGrid.ItemsSource = courseStats;

            if (courseStats.Any())
            {
                GenerateChart(courseStats.Select(s => s.Курс).ToList(),
                               courseStats.Select(s => (double)s.КоличествоСтудентов).ToList(),
                               "Популярность курсов (новые студенты)");
            }
            else
            {
                // Может случиться, если студенты есть, но не привязаны к курсам
                GenerateChart(new List<string>(), new List<double>(), "Популярность курсов (нет данных)");
            }
        }

        // --- Заглушки для новой статистики ---
        private void GenerateCourseCompletionStatistics(DateTime startDate, DateTime endDate) { StatisticsDataGrid.ItemsSource = null; ChartContainer.Content = null; MessageBox.Show("Статистика по завершению курсов еще не реализована."); }
        private void GenerateAgeStatistics(DateTime startDate, DateTime endDate) { StatisticsDataGrid.ItemsSource = null; ChartContainer.Content = null; MessageBox.Show("Статистика по возрасту еще не реализована."); }
        private void GenerateTeacherLoadStatistics(DateTime startDate, DateTime endDate) { StatisticsDataGrid.ItemsSource = null; ChartContainer.Content = null; MessageBox.Show("Статистика по нагрузке преподавателей еще не реализована."); }
        private void GenerateAdEffectivenessStatistics(DateTime startDate, DateTime endDate) { StatisticsDataGrid.ItemsSource = null; ChartContainer.Content = null; MessageBox.Show("Статистика по эффективности рекламы еще не реализована."); }
        private void GenerateStudyFormatStatistics(DateTime startDate, DateTime endDate) { StatisticsDataGrid.ItemsSource = null; ChartContainer.Content = null; MessageBox.Show("Статистика по дистанционному и очному обучению еще не реализована."); }


        // --- Генерация графика (зависит от библиотеки) ---
        private void GenerateChart(List<string> labels, List<double> values, string title)
        {
            ChartContainer.Content = null; // Очищаем предыдущий график

            if (labels == null || !labels.Any() || values == null || !values.Any() || labels.Count != values.Count)
            {
                // Можно показать текстовое сообщение вместо графика
                ChartContainer.Content = new TextBlock
                {
                    Text = $"Нет данных для построения графика '{title}'",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                return;
            }

            try
            {
                // --- НАЧАЛО: Пример для LiveCharts (WPF .NET Framework) ---
                // Установите NuGet: LiveCharts.Wpf
                /*
                 var cartesianChart = new LiveCharts.Wpf.CartesianChart
                 {
                     AxisX = new AxesCollection { new Axis { Title = "Категории", Labels = labels } },
                     AxisY = new AxesCollection { new Axis { Title = "Значения", LabelFormatter = value => value.ToString("N0") } }, // Формат без десятичных знаков
                     Series = new SeriesCollection
                     {
                         new ColumnSeries // Или LineSeries, PieSeries и т.д.
                         {
                             Title = title,
                             Values = new ChartValues<double>(values)
                         }
                     },
                     LegendLocation = LegendLocation.Top // Расположение легенды
                 };
                 ChartContainer.Content = cartesianChart; // Помещаем график в ContentControl
                 */
                // --- КОНЕЦ: Пример для LiveCharts ---

                // --- Если используете другую библиотеку, код будет здесь ---

                // --- Заглушка, если LiveCharts не используется ---
                // (Удалите эту заглушку, если раскомментировали код LiveCharts или добавили свою библиотеку)
                ChartContainer.Content = new TextBlock
                {
                    Text = $"Здесь должен быть график '{title}'\n(Библиотека графиков не подключена/не реализовано)",
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                // --- Конец заглушки ---

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении графика: {ex.Message}", "Ошибка графика", MessageBoxButton.OK, MessageBoxImage.Error);
                ChartContainer.Content = new TextBlock { Text = "Ошибка построения графика." }; // Сообщение об ошибке в области графика
            }
        }


        // --- Экспорт в CSV ---
        private void ExportToCsv(DataGrid dataGrid, string filePrefix)
        {
            if (dataGrid.ItemsSource == null || !dataGrid.Items.Cast<object>().Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV файл (*.csv)|*.csv",
                FileName = $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv", // Имя файла по умолчанию
                Title = "Сохранить как CSV"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    // Используем StreamWriter для записи в файл с кодировкой UTF-8 (важно для кириллицы)
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Заголовки столбцов
                        IEnumerable<string> headers = dataGrid.Columns.Select(column => CsvEscape(column.Header?.ToString() ?? ""));
                        sw.WriteLine(string.Join(",", headers)); // Используем запятую как разделитель

                        // Данные строк
                        foreach (var item in dataGrid.ItemsSource)
                        {
                            List<string> rowValues = new List<string>();
                            var itemType = item.GetType();

                            foreach (var column in dataGrid.Columns)
                            {
                                string cellValue = "";
                                // Пытаемся получить значение через Binding Path или SortMemberPath
                                string propertyName = null;
                                if (column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
                                {
                                    propertyName = binding.Path.Path;
                                }
                                else if (!string.IsNullOrEmpty(column.SortMemberPath))
                                {
                                    propertyName = column.SortMemberPath;
                                }

                                if (!string.IsNullOrEmpty(propertyName))
                                {
                                    try
                                    {
                                        // Обработка вложенных свойств (например, "Client.LastName")
                                        object propertyValue = item;
                                        var properties = propertyName.Split('.');
                                        foreach (var prop in properties)
                                        {
                                            if (propertyValue == null) break;
                                            var propInfo = propertyValue.GetType().GetProperty(prop);
                                            if (propInfo == null)
                                            {
                                                propertyValue = null; // Свойство не найдено
                                                break;
                                            }
                                            propertyValue = propInfo.GetValue(propertyValue, null);
                                        }

                                        // Форматирование значения
                                        if (propertyValue is DateTime dtValue)
                                        {
                                            cellValue = dtValue.ToString("dd.MM.yyyy HH:mm:ss"); // Или нужный формат даты/времени
                                        }
                                        else if (propertyValue is decimal decValue)
                                        {
                                            cellValue = decValue.ToString(CultureInfo.InvariantCulture); // Точка как разделитель
                                        }
                                        else if (propertyValue is double dblValue)
                                        {
                                            cellValue = dblValue.ToString(CultureInfo.InvariantCulture); // Точка как разделитель
                                        }
                                        else
                                        {
                                            cellValue = propertyValue?.ToString() ?? "";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error getting property {propertyName}: {ex.Message}");
                                        cellValue = "#ERROR"; // Ошибка получения свойства
                                    }
                                }
                                else
                                {
                                    // Попытка получить текстовое значение из ячейки (менее надежно)
                                    try
                                    {
                                        var cellContent = column.GetCellContent(item);
                                        if (cellContent is TextBlock tb)
                                        {
                                            cellValue = tb.Text;
                                        }
                                        else if (cellContent != null)
                                        {
                                            // Попытка найти TextBlock внутри (если это ContentPresenter и т.п.)
                                            var innerTb = FindVisualChild<TextBlock>(cellContent);
                                            cellValue = innerTb?.Text ?? cellContent.ToString() ?? "";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error getting cell content: {ex.Message}");
                                        cellValue = "#ERROR";
                                    }
                                }

                                rowValues.Add(CsvEscape(cellValue));
                            }
                            sw.WriteLine(string.Join(",", rowValues));
                        }
                    }
                    MessageBox.Show($"Данные успешно экспортированы в файл:\n{saveFileDialog.FileName}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в CSV: {ex.Message}", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        // Вспомогательный метод для экранирования данных в CSV
        private string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Если значение содержит запятую, кавычки или перенос строки, его нужно обернуть в кавычки
            bool needsQuotes = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");

            // Заменяем внутренние кавычки на двойные кавычки
            string escapedValue = value.Replace("\"", "\"\"");

            if (needsQuotes)
            {
                return $"\"{escapedValue}\"";
            }
            else
            {
                return escapedValue;
            }
        }

        // Вспомогательный метод для поиска визуального потомка (нужен, если GetCellContent возвращает не TextBlock)
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

    } // Конец класса Reports
} // Конец namespace