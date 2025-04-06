using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportTypeComboBox.SelectedItem != null)
            {
                GenerateReport();
            }
        }

        private void StatisticsTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatisticsTypeComboBox.SelectedItem != null)
            {
                GenerateStatistics();
            }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void GenerateStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateStatistics();
        }

        private void GenerateReport()
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите период для отчета");
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            switch (((ComboBoxItem)ReportTypeComboBox.SelectedItem).Content.ToString())
            {
                case "Отчет по расписанию занятий":
                    GenerateScheduleReport(startDate, endDate);
                    break;
                case "Отчет по посещаемости":
                    GenerateAttendanceReport(startDate, endDate);
                    break;
                case "Отчет по договорам":
                   // GenerateContractsReport(startDate, endDate);
                    break;
                // Добавьте остальные case для других типов отчетов
            }
        }

        private void GenerateStatistics()
        {
            if (StatStartDatePicker.SelectedDate == null || StatEndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите период для статистики");
                return;
            }

            var startDate = StatStartDatePicker.SelectedDate.Value;
            var endDate = StatEndDatePicker.SelectedDate.Value;

            switch (((ComboBoxItem)StatisticsTypeComboBox.SelectedItem).Content.ToString())
            {
                case "Статистика по количеству обучающихся":
                    GenerateStudentCountStatistics(startDate, endDate);
                    break;
                case "Статистика по популярным курсам":
                    GeneratePopularCoursesStatistics(startDate, endDate);
                    break;
                // Добавьте остальные case для других типов статистики
            }
        }

        private void GenerateScheduleReport(DateTime startDate, DateTime endDate)
        {
            var scheduleData = context.Schedule
                .Where(s => s.ClassDate >= startDate && s.ClassDate <= endDate)
                .Select(s => new
                {
                    Дата = s.ClassDate,
                    Время = s.ClassTime,
                    Группа = s.Group.GroupName,
                    Предмет = s.Course.CourseName,
                    Преподаватель = s.Teacher.LastName + " " + s.Teacher.FirstName,
                    Аудитория = s.Classroom.ClassroomNumber
                })
                .OrderBy(s => s.Дата)
                .ThenBy(s => s.Время)
                .ToList();

            ReportDataGrid.ItemsSource = scheduleData;
        }

        private void GenerateAttendanceReport(DateTime startDate, DateTime endDate)
        {
            var attendanceData = context.Attendance
                .Where(a => a.Schedule.ClassDate >= startDate && a.Schedule.ClassDate <= endDate)
                .Select(a => new
                {
                    Дата = a.Schedule.ClassDate,
                    Студент = a.Student.LastName + " " + a.Student.FirstName,
                    Группа = a.Schedule.Group.GroupName,
                    Предмет = a.Schedule.Course.CourseName,
                    Статус = a.StatusAttendance.StatusAttendanceName
                })
                .OrderBy(a => a.Дата)
                .ThenBy(a => a.Студент)
                .ToList();

            ReportDataGrid.ItemsSource = attendanceData;
        }

        private void GenerateStudentCountStatistics(DateTime startDate, DateTime endDate)
        {
            var studentStats = context.Student
                .Where(s => s.EnrollmentDate >= startDate && s.EnrollmentDate <= endDate)
                .GroupBy(s => s.EnrollmentDate.Month)
                .Select(g => new
                {
                    Месяц = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    КоличествоСтудентов = g.Count()
                })
                .OrderBy(s => s.Месяц)
                .ToList();

            StatisticsDataGrid.ItemsSource = studentStats;
            GenerateChart(studentStats.Select(s => s.Месяц).ToList(),
                         studentStats.Select(s => (double)s.КоличествоСтудентов).ToList(),
                         "Количество студентов по месяцам");
        }

        private void GeneratePopularCoursesStatistics(DateTime startDate, DateTime endDate)
        {
            var courseStats = context.GroupStudent
                .Where(gs => gs.EnrollmentDate >= startDate && gs.EnrollmentDate <= endDate)
                .GroupBy(gs => gs.Group.Course.CourseName)
                .Select(g => new
                {
                    Курс = g.Key,
                    КоличествоСтудентов = g.Count()
                })
                .OrderByDescending(s => s.КоличествоСтудентов)
                .ToList();

            StatisticsDataGrid.ItemsSource = courseStats;
            GenerateChart(courseStats.Select(s => s.Курс).ToList(),
                         courseStats.Select(s => (double)s.КоличествоСтудентов).ToList(),
                         "Популярность курсов");
        }

        private void GenerateChart(List<string> labels, List<double> values, string title)
        {
            // Здесь должна быть реализация построения графика
            // Можно использовать библиотеки как LiveCharts, OxyPlot или ScottPlot
        }

        private void ExportReportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel(ReportDataGrid);
        }

        private void ExportStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel(StatisticsDataGrid);
        }

        private void ExportToExcel(DataGrid dataGrid)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = true;
                Workbook workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);
                Worksheet sheet = (Worksheet)workbook.Sheets[1];

                // Заголовки
                for (int j = 0; j < dataGrid.Columns.Count; j++)
                {
                    Range myRange = (Range)sheet.Cells[1, j + 1];
                    sheet.Cells[1, j + 1].Font.Bold = true;
                    sheet.Columns[j + 1].ColumnWidth = 15;
                    myRange.Value2 = dataGrid.Columns[j].Header;
                }

                // Данные
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    var item = dataGrid.Items[i];
                    for (int j = 0; j < dataGrid.Columns.Count; j++)
                    {
                        Range myRange = (Range)sheet.Cells[i + 2, j + 1];
                        var value = (dataGrid.Columns[j].GetCellContent(item) as TextBlock)?.Text;
                        myRange.Value2 = value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}");
            }
        }
    }
}
