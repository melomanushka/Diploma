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
        private List<object> currentReportData; // Хранение текущих данных отчета

        public Reports()
        {
            InitializeComponent();
            context = new Dimploma1Entities();
            InitializeDatePickers();
            currentReportData = new List<object>();
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
                        //GenerateGroupsReport(startDate, endDate);
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

            currentReportData = scheduleData.Cast<object>().ToList();

            if (scheduleData.Any())
            {
                // Отображение в DataGrid (если есть)
                MessageBox.Show($"Отчет по расписанию сформирован. Найдено записей: {scheduleData.Count}", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("За указанный период занятий не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

            currentReportData = attendanceData.Cast<object>().ToList();

            if (attendanceData.Any())
            {
                MessageBox.Show($"Отчет по посещаемости сформирован. Найдено записей: {attendanceData.Count}", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("За указанный период данных о посещаемости не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

            currentReportData = contractsData.Cast<object>().ToList();

            if (contractsData.Any())
            {
                MessageBox.Show($"Отчет по договорам сформирован. Найдено записей: {contractsData.Count}", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("За указанный период договоров не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GeneratePaymentsReport(DateTime startDate, DateTime endDate)
        {
            var paymentsData = context.Payment
                .Include("Contract.Client")
                .Include("Contract.Student.Client")
                .Include("StatusPayment")
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .Select(p => new
                {
                    НомерПлатежа = p.PaymentID,
                    ДатаПлатежа = p.PaymentDate,
                    Сумма = p.Quantity,
                    НомерДоговора = p.Contract != null ? p.Contract.ContractID : (int?)null,
                    Клиент = (p.Contract != null && p.Contract.Client != null) ? (p.Contract.Client.LastName + " " + p.Contract.Client.FirstName) : "Н/Д",
                    Студент = (p.Contract != null && p.Contract.Student != null && p.Contract.Student.Client != null) ? (p.Contract.Student.Client.LastName + " " + p.Contract.Student.Client.FirstName) : "Н/Д",
                    Комментарий = p.Note ?? ""
                })
                .OrderBy(p => p.ДатаПлатежа)
                .ToList();

            currentReportData = paymentsData.Cast<object>().ToList();

            if (paymentsData.Any())
            {
                var totalAmount = paymentsData.Where(p => p.Сумма != null).Sum(p => p.Сумма.Value);
                MessageBox.Show($"Отчет по оплатам сформирован. Найдено записей: {paymentsData.Count}\nОбщая сумма: {totalAmount:C}", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("За указанный период платежей не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateTeachersReport(DateTime startDate, DateTime endDate)
        {
            var teachersData = context.Schedule
                    .Where(s => s.ClassDate >= startDate &&
                               s.ClassDate <= endDate &&
                               s.TeacherID != null)
                    .Join(context.Employee,
                        s => s.TeacherID,
                        e => e.EmployeeID,
                        (s, e) => new { Schedule = s, Employee = e })
                    .Include(x => x.Schedule.Course)
                    .Include(x => x.Schedule.Group)
                    .AsEnumerable()
                    .GroupBy(x => x.Employee.EmployeeID)
                    .Select(g =>
                    {
                        var teacher = g.FirstOrDefault()?.Employee;

                        return new
                        {
                            Преподаватель = teacher != null
                                ? $"{teacher.LastName} {teacher.FirstName}"
                                : "Н/Д",
                            КоличествоЗанятий = g.Count(),
                            Курсы = g.Select(x => x.Schedule.Course?.CourseName)
                                    .Distinct()
                                    .Count(n => n != null),
                            Группы = g.Select(x => x.Schedule.Group?.GroupName)
                                    .Distinct()
                                    .Count(n => n != null),
                            СписокКурсов = string.Join(", ",
                                g.Select(x => x.Schedule.Course?.CourseName ?? "Н/Д")
                                 .Distinct()
                                 .OrderBy(n => n)),
                            ПервоеЗанятие = g.Min(x => x.Schedule.ClassDate),
                            ПоследнееЗанятие = g.Max(x => x.Schedule.ClassDate)
                        };
                    })
                    .OrderByDescending(t => t.КоличествоЗанятий)
                    .ToList();

            currentReportData = teachersData.Cast<object>().ToList();

            if (teachersData.Any())
            {
                MessageBox.Show($"Отчет по преподавателям сформирован. Найдено преподавателей: {teachersData.Count}", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("За указанный период данных о преподавателях не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateCourseCompletionStatistics(DateTime startDate, DateTime endDate)
        {
            var completionStats = (
                from contract in context.Contract
                join price in context.Price on contract.PriceID equals price.PriceID
                join course in context.Course on price.CourseID equals course.CourseID
                join status in context.StatusContract on contract.StatusContractID equals status.StatusContractID
                where contract.DateEnd >= startDate && contract.DateEnd <= endDate
                group new { contract, course, status } by new
                {
                    CourseName = course.CourseName ?? "Н/Д",
                    StatusName = status.StatusContractName
                } into g
                select new
                {
                    Курс = g.Key.CourseName,
                    Статус = g.Key.StatusName,
                    Количество = g.Count()
                })
                .OrderBy(x => x.Курс)
                .ThenBy(x => x.Статус)
                .ToList();

            StatisticsDataGrid.ItemsSource = completionStats;

            var completedStatuses = new[] { "Завершен", "Окончен", "Выполнен" };
            var chartData = completionStats
                .Where(x => completedStatuses.Contains(x.Статус))
                .GroupBy(x => x.Курс)
                .Select(g => new
                {
                    Курс = g.Key,
                    Завершено = g.Sum(x => x.Количество)
                })
                .ToList();

            GenerateChart(
                labels: chartData.Select(x => x.Курс).ToList(),
                values: chartData.Select(x => (double)x.Завершено).ToList(),
                title: chartData.Any() ? "Завершение курсов" : "Завершение курсов (нет данных)"
            );
        }

        private void GenerateClassroomUsageReport(DateTime startDate, DateTime endDate)
        {
            var classroomData = context.Schedule
                .Include("Cabinet")
                .Include("Group")
                .Include("Course")
                .Where(s => s.ClassDate >= startDate && s.ClassDate <= endDate && s.Cabinet != null)
                .GroupBy(s => s.Cabinet)
                .Select(g => new
                {
                    Аудитория = g.Key.CabinetName ?? "Н/Д",
                    КоличествоЗанятий = g.Count(),
                    УникальныхГрупп = g.Select(s => s.GroupID).Distinct().Count(),
                    УникальныхКурсов = g.Select(s => s.CourseID).Distinct().Count(),
                    ЗагруженностьПроцент = Math.Round((double)g.Count() / DateTime.DaysInMonth(startDate.Year, startDate.Month) * 100, 2),
                    ПервоеЗанятие = g.Min(s => s.ClassDate),
                    ПоследнееЗанятие = g.Max(s => s.ClassDate),
                    СписокКурсов = string.Join(", ", g.Select(s => s.Course != null ? s.Course.CourseName : "Н/Д").Distinct())
                })
                .OrderByDescending(c => c.КоличествоЗанятий)
                .ToList();

            currentReportData = classroomData.Cast<object>().ToList();

            if (classroomData.Any())
            {
                MessageBox.Show($"Отчет по использованию аудиторий сформирован. Найдено аудиторий: {classroomData.Count}", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("За указанный период данных об использовании аудиторий не найдено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateOverallPeriodReport(DateTime startDate, DateTime endDate)
        {
            // Собираем общую статистику за период
            var totalContracts = context.Contract.Count(c => c.DateCreate >= startDate && c.DateCreate <= endDate);
            var totalStudents = context.Contract.Where(c => c.DateStart >= startDate && c.DateStart <= endDate && c.StudentID != null)
                                               .Select(c => c.StudentID).Distinct().Count();
            var totalClasses = context.Schedule.Count(s => s.ClassDate >= startDate && s.ClassDate <= endDate);
            var totalPayments = context.Payment.Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate).Sum(p => p.Quantity) ?? 0;
            var activeGroups = context.Schedule.Where(s => s.ClassDate >= startDate && s.ClassDate <= endDate)
                                              .Select(s => s.GroupID).Distinct().Count();
            var activeTeachers = context.Schedule.Where(s => s.ClassDate >= startDate && s.ClassDate <= endDate)
                                                .Select(s => s.TeacherID).Distinct().Count();

            var overallData = new List<object>
            {
                new { Показатель = "Общее количество договоров", Значение = totalContracts.ToString() },
                new { Показатель = "Количество новых студентов", Значение = totalStudents.ToString() },
                new { Показатель = "Проведено занятий", Значение = totalClasses.ToString() },
                new { Показатель = "Общая сумма платежей", Значение = totalPayments.ToString("C") },
                new { Показатель = "Активных групп", Значение = activeGroups.ToString() },
                new { Показатель = "Работающих преподавателей", Значение = activeTeachers.ToString() },
                new { Показатель = "Период отчета", Значение = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}" },
                new { Показатель = "Дата формирования", Значение = DateTime.Now.ToString("dd.MM.yyyy HH:mm") }
            };

            currentReportData = overallData;

            MessageBox.Show($"Итоговый отчет за учебный период сформирован.", "Отчет готов", MessageBoxButton.OK, MessageBoxImage.Information);
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

        //private void GenerateCourseCompletionStatistics(DateTime startDate, DateTime endDate)
        //{
        //    var completionStats = context.Contract
        //        .Include("Course")
        //        .Where(c => c.DateEnd >= startDate && c.DateEnd <= endDate && c.StatusContract != null)
        //        .GroupBy(c => new { CourseName = c.Course != null ? c.Course.CourseName : "Н/Д", Status = c.StatusContract.StatusContractName })
        //        .Select(g => new
        //        {
        //            Курс = g.Key.CourseName,
        //            Статус = g.Key.Status,
        //            Количество = g.Count()
        //        })
        //        .OrderBy(s => s.Курс)
        //        .ThenBy(s => s.Статус)
        //        .ToList();

        //    StatisticsDataGrid.ItemsSource = completionStats;

        //    if (completionStats.Any())
        //    {
        //        var completedCourses = completionStats.Where(s => s.Статус.Contains("Завершен") || s.Статус.Contains("Окончен"))
        //                                             .GroupBy(s => s.Курс)
        //                                             .Select(g => new { Курс = g.Key, Завершено = g.Sum(x => x.Количество) })
        //                                             .ToList();

        //        GenerateChart(completedCourses.Select(s => s.Курс).ToList(),
        //                     completedCourses.Select(s => (double)s.Завершено).ToList(),
        //                     "Завершение курсов");
        //    }
        //    else
        //    {
        //        GenerateChart(new List<string>(), new List<double>(), "Завершение курсов (нет данных)");
        //    }
        //}

        private void GenerateAgeStatistics(DateTime startDate, DateTime endDate)
        {
            var ageStats = context.Contract
                .Include("Student.Client")
                .Where(c => c.DateStart >= startDate && c.DateStart <= endDate && c.Student != null && c.Student.Client != null && c.Student.Birthdate!= null)
                .ToList() // Загружаем в память для вычисления возраста
                .Select(c => new
                {
                    Contract = c,
                    Age = DateTime.Now.Year - c.Student.Birthdate.Value.Year - (DateTime.Now.DayOfYear < c.Student.Birthdate.Value.DayOfYear ? 1 : 0)
                })
                .GroupBy(x => x.Age / 10 * 10) // Группируем по десятилетиям
                .Select(g => new
                {
                    ВозрастнаяГруппа = $"{g.Key}-{g.Key + 9} лет",
                    КоличествоСтудентов = g.Select(x => x.Contract.StudentID).Distinct().Count(),
                    СреднийВозраст = Math.Round(g.Average(x => x.Age), 1),
                    GroupKey = g.Key
                })
                .OrderBy(s => s.GroupKey)
                .ToList();

            StatisticsDataGrid.ItemsSource = ageStats;

            if (ageStats.Any())
            {
                GenerateChart(ageStats.Select(s => s.ВозрастнаяГруппа).ToList(),
                             ageStats.Select(s => (double)s.КоличествоСтудентов).ToList(),
                             "Распределение студентов по возрасту");
            }
            else
            {
                GenerateChart(new List<string>(), new List<double>(), "Статистика по возрасту (нет данных)");
            }
        }

        private void GenerateTeacherLoadStatistics(DateTime startDate, DateTime endDate)
        {
            var totalLessons = context.Schedule
                .Count(s => s.ClassDate >= startDate && s.ClassDate <= endDate);

            var teacherLoadStats = context.Schedule
                .Where(s => s.ClassDate >= startDate && s.ClassDate <= endDate)
                .GroupJoin(
                    context.Employee,
                    schedule => schedule.TeacherID,
                    employee => employee.EmployeeID,
                    (schedule, employees) => new { schedule, employees }
                )
                .SelectMany(
                    x => x.employees.DefaultIfEmpty(),
                    (x, teacher) => new { x.schedule, teacher }
                )
                .GroupBy(x => x.teacher)
                .Select(g => new
                {
                    Преподаватель = g.Key != null
                        ? $"{g.Key.LastName} {g.Key.FirstName}"
                        : "Н/Д",
                    КоличествоЗанятий = g.Count(),
                    УникальныхГрупп = g.Select(x => x.schedule.GroupID).Distinct().Count(),
                    УникальныхКурсов = g.Select(x => x.schedule.CourseID).Distinct().Count(),
                    ЗанятийВНеделю = totalLessons > 0
                        ? Math.Round((double)g.Count() / ((endDate - startDate).Days / 7.0), 1)
                        : 0,
                    НагрузкаПроцент = totalLessons > 0
                        ? Math.Round((double)g.Count() / totalLessons * 100, 2)
                        : 0
                })
                .OrderByDescending(t => t.КоличествоЗанятий)
                .AsNoTracking()
                .ToList();

            StatisticsDataGrid.ItemsSource = teacherLoadStats;

            if (teacherLoadStats.Any())
            {
                GenerateChart(teacherLoadStats.Select(s => s.Преподаватель).ToList(),
                             teacherLoadStats.Select(s => (double)s.КоличествоЗанятий).ToList(),
                             "Нагрузка преподавателей");
            }
            else
            {
                GenerateChart(new List<string>(), new List<double>(), "Нагрузка преподавателей (нет данных)");
            }
        }

        private void GenerateAdEffectivenessStatistics(DateTime startDate, DateTime endDate)
        {
            // Предполагаем, что в таблице Client есть поле SourceOfInformation или подобное
            var adStats = context.Contract
                .Include("Client")
                .Where(c => c.DateCreate >= startDate && c.DateCreate <= endDate && c.Client != null)
                .GroupBy(c => c.Client.InformationSource.InformationSourceName ?? "Не указано")
                .Select(g => new
                {
                    ИсточникИнформации = g.Key,
                    КоличествоДоговоров = g.Count(),
                    КоличествоКлиентов = g.Select(c => c.ClientID).Distinct().Count(),
                    ПроцентОтОбщего = 0.0 // Будет вычислен после загрузки
                })
                .OrderByDescending(s => s.КоличествоДоговоров)
                .ToList();

            // Вычисляем проценты
            var totalContracts = adStats.Sum(s => s.КоличествоДоговоров);
            if (totalContracts > 0)
            {
                adStats = adStats.Select(s => new
                {
                    s.ИсточникИнформации,
                    s.КоличествоДоговоров,
                    s.КоличествоКлиентов,
                    ПроцентОтОбщего = Math.Round((double)s.КоличествоДоговоров / totalContracts * 100, 1)
                }).ToList();
            }

            StatisticsDataGrid.ItemsSource = adStats;

            if (adStats.Any())
            {
                GenerateChart(adStats.Select(s => s.ИсточникИнформации).ToList(),
                             adStats.Select(s => (double)s.КоличествоДоговоров).ToList(),
                             "Эффективность источников информации");
            }
            else
            {
                GenerateChart(new List<string>(), new List<double>(), "Эффективность рекламы (нет данных)");
            }
        }

        private void GenerateStudyFormatStatistics(DateTime startDate, DateTime endDate)
        {
            var formatStats = context.Contract
                .Include("FormOfStudy")
                .Where(c => c.DateStart >= startDate && c.DateStart <= endDate && c.FormOfStudy != null)
                .GroupBy(c => c.FormOfStudy.FormOfStudyName)
                .Select(g => new
                {
                    ФормаОбучения = g.Key,
                    КоличествоДоговоров = g.Count(),
                    КоличествоСтудентов = g.Select(c => c.StudentID).Distinct().Count(),
                    ПроцентОтОбщего = 0.0 // Будет вычислен после загрузки
                })
                .OrderByDescending(s => s.КоличествоДоговоров)
                .ToList();

            // Вычисляем проценты
            var totalContracts = formatStats.Sum(s => s.КоличествоДоговоров);
            if (totalContracts > 0)
            {
                formatStats = formatStats.Select(s => new
                {
                    s.ФормаОбучения,
                    s.КоличествоДоговоров,
                    s.КоличествоСтудентов,
                    ПроцентОтОбщего = Math.Round((double)s.КоличествоДоговоров / totalContracts * 100, 1)
                }).ToList();
            }

            StatisticsDataGrid.ItemsSource = formatStats;

            if (formatStats.Any())
            {
                GenerateChart(formatStats.Select(s => s.ФормаОбучения).ToList(),
                             formatStats.Select(s => (double)s.КоличествоДоговоров).ToList(),
                             "Распределение по формам обучения");
            }
            else
            {
                GenerateChart(new List<string>(), new List<double>(), "Формы обучения (нет данных)");
            }
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
            if (currentReportData == null || !currentReportData.Any())
            {
                MessageBox.Show("Нет данных для экспорта. Пожалуйста, сначала сформируйте отчет.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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
                    CreateWordReport(saveFileDialog.FileName, reportType, currentReportData, true);
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
                    CreateWordReport(saveFileDialog.FileName, statisticsType, StatisticsDataGrid.ItemsSource, false);
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

        private void CreateWordReport(string fileName, string reportTitle, System.Collections.IEnumerable data, bool isReport = true)
        {
            // Создаем новый документ Word
            using (var document = DocX.Create(fileName))
            {
                // Добавляем заголовок документа
                var title = document.InsertParagraph(reportTitle);
                title.FontSize(18).Bold().Alignment = Alignment.center;

                document.InsertParagraph(); // Пустая строка

                // Добавляем информацию о периоде
                DateTime startDate, endDate;
                if (isReport)
                {
                    startDate = StartDatePicker.SelectedDate ?? DateTime.Today.AddMonths(-1);
                    endDate = EndDatePicker.SelectedDate ?? DateTime.Today;
                }
                else
                {
                    startDate = StatStartDatePicker.SelectedDate ?? DateTime.Today.AddMonths(-1);
                    endDate = StatEndDatePicker.SelectedDate ?? DateTime.Today;
                }

                var period = document.InsertParagraph($"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
                period.FontSize(12).Bold().Alignment = Alignment.center;

                // Добавляем дату формирования отчета
                var dateGenerated = document.InsertParagraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                dateGenerated.FontSize(10).Alignment = Alignment.right;

                document.InsertParagraph(); // Пустая строка

                // Получаем данные
                var dataList = data.Cast<object>().ToList();
                if (!dataList.Any())
                {
                    document.InsertParagraph("Нет данных для отображения.")
                        .FontSize(12)
                        .Alignment = Alignment.center;
                    document.Save();
                    return;
                }

                // Получаем свойства первого объекта для заголовков таблицы
                var firstItem = dataList.First();
                var properties = firstItem.GetType().GetProperties()
                    .Where(p => !p.Name.EndsWith("SortKey") && !p.Name.EndsWith("Key")) // Исключаем служебные поля
                    .ToArray();
                var columnCount = properties.Length;

                if (columnCount == 0)
                {
                    document.InsertParagraph("Ошибка: не удалось получить структуру данных.")
                        .FontSize(12)
                        .Alignment = Alignment.center;
                    document.Save();
                    return;
                }

                // Создаем таблицу
                var table = document.AddTable(dataList.Count + 1, columnCount);
                table.Design = TableDesign.ColorfulGridAccent1;
                table.Alignment = Alignment.center;

                // Устанавливаем ширину таблицы (аналог SetWidthsEvenly через явное задание ширины)
                float[] widths = new float[columnCount];
                float columnWidth = 100f / columnCount;
                for (int i = 0; i < columnCount; i++)
                {
                    widths[i] = columnWidth;
                }
                table.SetWidths(widths);

                // Заполняем заголовки таблицы
                for (int i = 0; i < properties.Length; i++)
                {
                    var headerCell = table.Rows[0].Cells[i];
                    headerCell.Paragraphs[0].Remove(false); // Добавляем параметр trackChanges = false
                    headerCell.InsertParagraph(properties[i].Name)
                        .Bold()
                        .FontSize(11)
                        .Alignment = Alignment.center;
                    // Используем Color.LightBlue вместо System.Drawing.Color.FromArgb
                    //headerCell.FillColor = Color.FromArgb(173, 216, 230);
                }

                // Заполняем данные таблицы
                for (int rowIndex = 0; rowIndex < dataList.Count; rowIndex++)
                {
                    var item = dataList[rowIndex];
                    for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                    {
                        var value = properties[colIndex].GetValue(item);
                        string cellValue = FormatCellValue(value);

                        var dataCell = table.Rows[rowIndex + 1].Cells[colIndex];
                        dataCell.Paragraphs[0].Remove(false); // Добавляем параметр trackChanges = false
                        var paragraph = dataCell.InsertParagraph(cellValue)
                            .FontSize(10);

                        // Выравниваем числовые значения по правому краю
                        if (IsNumericValue(value))
                        {
                            paragraph.Alignment = Alignment.right;
                        }
                        else
                        {
                            paragraph.Alignment = Alignment.left;
                        }
                    }
                }

                // Вставляем таблицу в документ
                document.InsertTable(table);

                // Добавляем итоговую информацию
                document.InsertParagraph();
                var summary = document.InsertParagraph($"Всего записей: {dataList.Count}");
                summary.FontSize(12).Bold().Alignment = Alignment.left;

                // Добавляем дополнительную аналитику для отчетов
                if (isReport)
                {
                    AddReportAnalytics(document, dataList, reportTitle);
                }

                // Вставка разрыва страницы (альтернатива InsertPageBreakBeforeSelf)
                document.InsertParagraph().InsertPageBreakAfterSelf();

                // Добавляем информацию о системе
                document.InsertParagraph();
                document.InsertParagraph("Документ сформирован автоматически системой управления учебным процессом")
                    .FontSize(9)
                    .Italic()
                    .Alignment = Alignment.center;

                // Добавляем место для подписи
                document.InsertParagraph();
                document.InsertParagraph("Ответственный: ________________________     Дата: _______________")
                    .FontSize(10)
                    .Alignment = Alignment.left;

                // Сохраняем документ
                document.Save();
            }
        }

        private string FormatCellValue(object value)
        {
            if (value == null) return "";

            switch (value)
            {
                case DateTime dateValue:
                    return dateValue.ToString("dd.MM.yyyy");
                case decimal decValue:
                    return decValue.ToString("N2");
                case double doubleValue:
                    return doubleValue.ToString("N2");
                case float floatValue:
                    return floatValue.ToString("N2");
                case int intValue:
                    return intValue.ToString("N0");
                case long longValue:
                    return longValue.ToString("N0");
                default:
                    return value.ToString();
            }
        }

        private bool IsNumericValue(object value)
        {
            return value is decimal || value is double || value is float ||
                   value is int || value is long || value is short;
        }

        private void AddReportAnalytics(DocX document, List<object> dataList, string reportTitle)
        {
            document.InsertParagraph();
            document.InsertParagraph("АНАЛИТИЧЕСКАЯ СВОДКА")
                .FontSize(14)
                .Bold()
                .Alignment = Alignment.center;

            document.InsertParagraph();

            try
            {
                // Добавляем специфичную аналитику в зависимости от типа отчета
                switch (reportTitle)
                {
                    case "Отчет по оплатам":
                        AddPaymentAnalytics(document, dataList);
                        break;
                    case "Отчет по договорам":
                        AddContractAnalytics(document, dataList);
                        break;
                    case "Отчет по преподавателям":
                        AddTeacherAnalytics(document, dataList);
                        break;
                    case "Отчет по учебным группам":
                        AddGroupAnalytics(document, dataList);
                        break;
                    default:
                        document.InsertParagraph($"Общий анализ данных за выбранный период показывает активность системы с общим количеством записей: {dataList.Count}")
                            .FontSize(11);
                        break;
                }
            }
            catch (Exception ex)
            {
                document.InsertParagraph($"Ошибка при формировании аналитики: {ex.Message}")
                    .FontSize(10)
                    .Italic();
            }
        }

        private void AddPaymentAnalytics(DocX document, List<object> dataList)
        {
            // Предполагаем, что в данных есть свойство Сумма
            try
            {
                var totalAmount = dataList.Select(item =>
                {
                    var sumProperty = item.GetType().GetProperty("Сумма");
                    var value = sumProperty?.GetValue(item);
                    return value as decimal? ?? 0m;
                }).Sum();

                document.InsertParagraph($"• Общая сумма платежей: {totalAmount:C}")
                    .FontSize(11);
                document.InsertParagraph($"• Количество операций: {dataList.Count}")
                    .FontSize(11);
                document.InsertParagraph($"• Средний размер платежа: {(dataList.Count > 0 ? totalAmount / dataList.Count : 0):C}")
                    .FontSize(11);
            }
            catch
            {
                document.InsertParagraph("• Аналитика по платежам недоступна")
                    .FontSize(11);
            }
        }

        private void AddContractAnalytics(DocX document, List<object> dataList)
        {
            document.InsertParagraph($"• Общее количество договоров: {dataList.Count}")
                .FontSize(11);

            // Можно добавить анализ по статусам, если это поле есть
            try
            {
                var statusGroups = dataList.GroupBy(item =>
                {
                    var statusProperty = item.GetType().GetProperty("Статус");
                    return statusProperty?.GetValue(item)?.ToString() ?? "Неизвестно";
                }).ToList();

                document.InsertParagraph("• Распределение по статусам:")
                    .FontSize(11);

                foreach (var group in statusGroups)
                {
                    document.InsertParagraph($"  - {group.Key}: {group.Count()}")
                        .FontSize(10);
                }
            }
            catch
            {
                document.InsertParagraph("• Детальная аналитика недоступна")
                    .FontSize(11);
            }
        }

        private void AddTeacherAnalytics(DocX document, List<object> dataList)
        {
            document.InsertParagraph($"• Количество активных преподавателей: {dataList.Count}")
                .FontSize(11);

            try
            {
                var totalClasses = dataList.Select(item =>
                {
                    var classProperty = item.GetType().GetProperty("КоличествоЗанятий");
                    var value = classProperty?.GetValue(item);
                    return value as int? ?? 0;
                }).Sum();

                document.InsertParagraph($"• Общее количество проведенных занятий: {totalClasses}")
                    .FontSize(11);
                document.InsertParagraph($"• Среднее количество занятий на преподавателя: {(dataList.Count > 0 ? (double)totalClasses / dataList.Count : 0):F1}")
                    .FontSize(11);
            }
            catch
            {
                document.InsertParagraph("• Детальная аналитика недоступна")
                    .FontSize(11);
            }
        }

        private void AddGroupAnalytics(DocX document, List<object> dataList)
        {
            document.InsertParagraph($"• Количество активных групп: {dataList.Count}")
                .FontSize(11);

            try
            {
                var totalStudents = dataList.Select(item =>
                {
                    var studentProperty = item.GetType().GetProperty("КоличествоСтудентов");
                    var value = studentProperty?.GetValue(item);
                    return value as int? ?? 0;
                }).Sum();

                document.InsertParagraph($"• Общее количество студентов: {totalStudents}")
                    .FontSize(11);
                document.InsertParagraph($"• Среднее количество студентов в группе: {(dataList.Count > 0 ? (double)totalStudents / dataList.Count : 0):F1}")
                    .FontSize(11);
            }
            catch
            {
                document.InsertParagraph("• Детальная аналитика недоступна")
                    .FontSize(11);
            }
        }
    }
}