using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Xceed.Document.NET; // Библиотека DocX
using Xceed.Words.NET;
using System.Drawing;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Reports.xaml
    /// </summary>
    public partial class Reports : Page
    {
        private readonly Dimploma1Entities context;

        public Reports()
        {
            InitializeComponent();
            context = new Dimploma1Entities();
            InitializeDatePickers();
        }

        private void InitializeDatePickers()
        {
            StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Today;
            StatStartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
            StatEndDatePicker.SelectedDate = DateTime.Today;
        }

        // --- Обработчики событий ---
        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void StatisticsTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

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
            ExportReportToWord();
        }

        private void ExportStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            ExportStatisticsToWord();
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
            var endDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

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
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при формировании отчета:\n{ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

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

            try
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
                        StatisticsDataGrid.ItemsSource = null;
                        ChartContainer.Content = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при формировании статистики:\n{ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                StatisticsDataGrid.ItemsSource = null;
                ChartContainer.Content = null;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // --- Реализации методов генерации отчетов ---
        private void GenerateScheduleReport(DateTime startDate, DateTime endDate)
        {
            var exactStartDate = startDate.Date;
            var exactEndDate = endDate.Date;

            var scheduleData = context.Schedule
                .Where(s => s.ClassDate >= exactStartDate && s.ClassDate <= exactEndDate)
                .Include(s => s.Time)
                .Include(s => s.Group)
                .Include(s => s.Course)
                .Include(s => s.Cabinet)
                .Select(s => new
                {
                    Дата = s.ClassDate,
                    Время = s.Time != null
                               ? $"{s.Time.TimeStart:hh\\:mm} - {s.Time.TimeEnd:hh\\:mm}"
                               : "Не указано",
                    Группа = s.Group != null ? s.Group.GroupName : "Не указана",
                    Курс = s.Course != null ? s.Course.CourseName : "Не указан",
                    Аудитория = s.Cabinet != null ? s.Cabinet.CabinetName : "Не указана",
                    ДеньНедели = s.ClassDate != null
                                    ? CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(s.ClassDate.Value.DayOfWeek)
                                    : "Не указан"
                })
                .OrderBy(s => s.Дата)
                .ThenBy(s => s.Время)
                .ToList();

        }

        private void GenerateAttendanceReport(DateTime startDate, DateTime endDate)
        {
            var attendanceData = context.Attendance
                .Include("Schedule")
                .Include("Student.Client")
                .Include("Schedule.Group")
                .Include("Schedule.Course")
                .Include("StatusAttendance")
               .Where(a => a.Schedule != null && a.Schedule.ClassDate >= startDate && a.Schedule.ClassDate <= endDate)
               .Select(a => new
               {
                   ДатаЗанятия = a.Schedule.ClassDate,
                   Студент = (a.Student != null && a.Student.Client != null) ? (a.Student.Client.LastName + " " + a.Student.Client.FirstName) : "Н/Д",
                   Группа = (a.Schedule != null && a.Schedule.Group != null) ? a.Schedule.Group.GroupName : "Н/Д",
                   Курс = (a.Schedule != null && a.Schedule.Course != null) ? a.Schedule.Course.CourseName : "Н/Д",
                   Статус = a.StatusAttendance != null ? a.StatusAttendance.StatusAttendanceName : "Н/Д"
               })
               .OrderBy(a => a.ДатаЗанятия)
               .ThenBy(a => a.Студент)
               .ToList();
        }

        private void GenerateContractsReport(DateTime startDate, DateTime endDate)
        {
            var contractsData = context.Contract
                .Include("Client")
                .Include("Student.Client")
                .Include("Price")
                .Include("FormOfStudy")
                .Include("StatusContract")
               .Where(c => c.DateCreate >= startDate && c.DateCreate <= endDate)
               .Select(c => new
               {
                   Номер = c.ContractID,
                   ДатаСоздания = c.DateCreate,
                   ДатаНачала = c.DateStart,
                   ДатаОкончания = c.DateEnd,
                   Клиент = (c.Client != null) ? (c.Client.LastName + " " + c.Client.FirstName) : "Н/Д",
                   Студент = (c.Student != null && c.Student.Client != null) ? (c.Student.Client.LastName + " " + c.Student.Client.FirstName) : "Н/Д",
                   Стоимость = c.Price != null ? c.Price.PriceQuantity : (decimal?)null,
                   ФормаОбучения = c.FormOfStudy != null ? c.FormOfStudy.FormOfStudyName : "Н/Д",
                   Статус = c.StatusContract != null ? c.StatusContract.StatusContractName : "Н/Д"
               })
               .OrderBy(c => c.ДатаСоздания)
               .ToList();

        }

        // Заглушки для новых отчетов
        private void GeneratePaymentsReport(DateTime startDate, DateTime endDate)
        {
            MessageBox.Show("Отчет по оплатам еще не реализован.");
        }

        private void GenerateTeachersReport(DateTime startDate, DateTime endDate)
        {
            MessageBox.Show("Отчет по преподавателям еще не реализован.");
        }

        private void GenerateGroupsReport(DateTime startDate, DateTime endDate)
        {
            MessageBox.Show("Отчет по учебным группам еще не реализован.");
        }

        private void GenerateClassroomUsageReport(DateTime startDate, DateTime endDate)
        {
            MessageBox.Show("Отчет по использованию аудиторий еще не реализован.");
        }

        private void GenerateOverallPeriodReport(DateTime startDate, DateTime endDate)
        {
            MessageBox.Show("Итоговый отчет за учебный период еще не реализован.");
        }

        // --- Статистика ---
        private void GenerateStudentCountStatistics(DateTime startDate, DateTime endDate)
        {
            var studentStats = context.Contract
               .Where(c => c.DateStart >= startDate && c.DateStart <= endDate && c.StudentID != null)
               .GroupBy(c => c.DateStart.Value.Month)
               .Select(g => new { MonthKey = g.Key, Count = g.Select(c => c.StudentID).Distinct().Count() })
                .ToList()
                .Select(g => new
                {
                    Месяц = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.MonthKey),
                    КоличествоСтудентов = g.Count,
                    MonthSortKey = g.MonthKey
                })
               .OrderBy(s => s.MonthSortKey)
               .ToList();

            StatisticsDataGrid.ItemsSource = studentStats;
            GenerateChart(studentStats.Select(s => s.Месяц).ToList(),
                           studentStats.Select(s => (double)s.КоличествоСтудентов).ToList(),
                           "Новые студенты по месяцам");
        }

        private void GeneratePopularCoursesStatistics(DateTime startDate, DateTime endDate)
        {
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

            var courseStats = context.GroupStudent
                .Where(gs => studentIdsInPeriod.Contains(gs.StudentID))
                .Include(gs => gs.Group.Course)
                .Where(gs => gs.Group != null && gs.Group.Course != null)
                .GroupBy(gs => gs.Group.Course)
                .Select(g => new
                {
                    Курс = g.Key.CourseName,
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
                GenerateChart(new List<string>(), new List<double>(), "Популярность курсов (нет данных)");
            }
        }

        // Заглушки для новой статистики
        private void GenerateCourseCompletionStatistics(DateTime startDate, DateTime endDate)
        {
            StatisticsDataGrid.ItemsSource = null;
            ChartContainer.Content = null;
            MessageBox.Show("Статистика по завершению курсов еще не реализована.");
        }

        private void GenerateAgeStatistics(DateTime startDate, DateTime endDate)
        {
            StatisticsDataGrid.ItemsSource = null;
            ChartContainer.Content = null;
            MessageBox.Show("Статистика по возрасту еще не реализована.");
        }

        private void GenerateTeacherLoadStatistics(DateTime startDate, DateTime endDate)
        {
            StatisticsDataGrid.ItemsSource = null;
            ChartContainer.Content = null;
            MessageBox.Show("Статистика по нагрузке преподавателей еще не реализована.");
        }

        private void GenerateAdEffectivenessStatistics(DateTime startDate, DateTime endDate)
        {
            StatisticsDataGrid.ItemsSource = null;
            ChartContainer.Content = null;
            MessageBox.Show("Статистика по эффективности рекламы еще не реализована.");
        }

        private void GenerateStudyFormatStatistics(DateTime startDate, DateTime endDate)
        {
            StatisticsDataGrid.ItemsSource = null;
            ChartContainer.Content = null;
            MessageBox.Show("Статистика по дистанционному и очному обучению еще не реализована.");
        }

        private void GenerateChart(List<string> labels, List<double> values, string title)
        {
            ChartContainer.Content = null;

            if (labels == null || !labels.Any() || values == null || !values.Any() || labels.Count != values.Count)
            {
                ChartContainer.Content = new TextBlock
                {
                    Text = $"Нет данных для построения графика '{title}'",
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                return;
            }

            ChartContainer.Content = new TextBlock
            {
                Text = $"Здесь должен быть график '{title}'\n(Библиотека графиков не подключена/не реализовано)",
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
        }

        // --- Экспорт в Word с использованием DocX ---
        private void ExportReportToWord()
        {

            var selectedItem = ReportTypeComboBox.SelectedItem as ComboBoxItem;
            string reportType = selectedItem?.Content?.ToString() ?? "Отчет";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word документ (*.docx)|*.docx",
                FileName = $"{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                Title = "Сохранить отчет как Word документ"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    MessageBox.Show($"Отчет успешно сохранен:\n{saveFileDialog.FileName}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании Word документа: {ex.Message}", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExportStatisticsToWord()
        {
            if (StatisticsDataGrid.ItemsSource == null || !StatisticsDataGrid.Items.Cast<object>().Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedItem = StatisticsTypeComboBox.SelectedItem as ComboBoxItem;
            string statisticsType = selectedItem?.Content?.ToString() ?? "Статистика";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word документ (*.docx)|*.docx",
                FileName = $"{statisticsType}_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                Title = "Сохранить статистику как Word документ"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    CreateWordReport(saveFileDialog.FileName, statisticsType, StatisticsDataGrid.ItemsSource);
                    MessageBox.Show($"Статистика успешно сохранена:\n{saveFileDialog.FileName}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании Word документа: {ex.Message}", "Ошибка экспорта", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void CreateWordReport(string fileName, string reportTitle, System.Collections.IEnumerable data)
        {
            // Создаем новый документ Word
            using (var document = DocX.Create(fileName))
            {
                // Добавляем заголовок документа
                var title = document.InsertParagraph(reportTitle)
                    .FontSize(16)
                    .Bold()
                    .Alignment = Alignment.center;

                // Добавляем информацию о периоде
                var period = document.InsertParagraph($"Период: {StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? StatStartDatePicker.SelectedDate?.ToString("dd.MM.yyyy")} - {EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy") ?? StatEndDatePicker.SelectedDate?.ToString("dd.MM.yyyy")}")
                    .FontSize(12)
                    .Alignment = Alignment.center;

                // Добавляем дату формирования отчета
                var dateGenerated = document.InsertParagraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                    .FontSize(10)
                    .Alignment = Alignment.right;

                document.InsertParagraph(); // Пустая строка

                // Получаем данные из DataGrid
                var dataList = data.Cast<object>().ToList();
                if (!dataList.Any()) return;

                // Получаем свойства первого объекта для заголовков таблицы
                var firstItem = dataList.First();
                var properties = firstItem.GetType().GetProperties();
                var columnCount = properties.Length;

                // Создаем таблицу
                var table = document.AddTable(dataList.Count + 1, columnCount);
                table.Design = TableDesign.ColorfulGridAccent1;
                table.Alignment = Alignment.center;

                // Заполняем заголовки таблицы
                for (int i = 0; i < properties.Length; i++)
                {
                    table.Rows[0].Cells[i].Paragraphs[0]
                        .Append(properties[i].Name)
                        .Bold()
                        .FontSize(11);
                    table.Rows[0].Cells[i].FillColor = Xceed.Drawing.Color.AliceBlue;
                }

                // Заполняем данные таблицы
                for (int rowIndex = 0; rowIndex < dataList.Count; rowIndex++)
                {
                    var item = dataList[rowIndex];
                    for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                    {
                        var value = properties[colIndex].GetValue(item);
                        string cellValue = "";

                        // Форматируем значения в зависимости от типа
                        if (value is DateTime dateValue)
                        {
                            cellValue = dateValue.ToString("dd.MM.yyyy");
                        }
                        else if (value is decimal decValue)
                        {
                            cellValue = decValue.ToString("N2");
                        }
                        else if (value is double doubleValue)
                        {
                            cellValue = doubleValue.ToString("N2");
                        }
                        else if (value is int intValue)
                        {
                            cellValue = intValue.ToString();
                        }
                        else
                        {
                            cellValue = value?.ToString() ?? "";
                        }

                        table.Rows[rowIndex + 1].Cells[colIndex].Paragraphs[0]
                            .Append(cellValue)
                            .FontSize(10);
                    }
                }

                // Вставляем таблицу в документ
                document.InsertTable(table);

                // Добавляем итоговую информацию
                document.InsertParagraph();
                var summary = document.InsertParagraph($"Всего записей: {dataList.Count}")
                    .FontSize(11)
                    .Bold();

                // Добавляем информацию о системе
                document.InsertParagraph();
                document.InsertParagraph("Документ сформирован автоматически системой управления учебным процессом")
                    .FontSize(8)
                    .Italic()
                    .Alignment = Alignment.center;

                // Сохраняем документ
                document.Save();
            }
        }
    }
}