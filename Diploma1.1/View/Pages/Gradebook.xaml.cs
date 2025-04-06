using Diploma1._1.Model; // Предполагаем, что здесь ваши EF-модели
// using Diploma1._1.View.CRUD; // Закомментировано, т.к. EditGradeWindow не используется в этом коде напрямую
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization; // Для CultureInfo
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Для Cursors

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Gradebook.xaml
    /// </summary>
    public partial class Gradebook : Page
    {
        // Коллекция для отображения в DataGrid
        private ObservableCollection<GradeRecord> gradeRecords;
        // Список возможных строковых представлений оценок для ComboBox в DataGrid
        private List<string> gradesList = new List<string> { "", "5", "4", "3", "2", "Н", "Б" }; // Добавил "Б" (болел?), пустую строку для отсутствия оценки
        private bool isIndividualLesson = false; // Флаг режима (индивидуальное/групповое)

        public Gradebook()
        {
            InitializeComponent();
            // Инициализация коллекции ДО InitializeControls, т.к. она используется там
            gradeRecords = new ObservableCollection<GradeRecord>();
            InitializeControls();
        }

        private void InitializeControls()
        {
            GradesDataGrid.ItemsSource = gradeRecords;

            // Установка возможных оценок в выпадающий список столбца DataGrid
            var gradeColumn = GradesDataGrid.Columns.FirstOrDefault(c => c.Header?.ToString() == "Оценка") as DataGridComboBoxColumn;
            if (gradeColumn != null)
            {
                gradeColumn.ItemsSource = gradesList;
            }

            // Загрузка данных для ComboBox'ов в блоке try-catch
            try
            {
                using (var context = new Dimploma1Entities()) // Используем using для контекста
                {
                    // Загрузка групп
                    var groups = context.Group.OrderBy(g => g.GroupName).ToList();
                    GroupComboBox.ItemsSource = groups;
                    GroupComboBox.DisplayMemberPath = "GroupName";
                    GroupComboBox.SelectedValuePath = "GroupID";

                    // Загрузка студентов с ФИО для индивидуальных занятий
                    var students = context.Student
                                          .Include(s => s.Client) // Включаем клиента
                                          .Where(s => s.Client != null) // Только студенты со связанным клиентом
                                          .OrderBy(s => s.Client.LastName).ThenBy(s => s.Client.FirstName)
                                          .Select(s => new // Проекция для ComboBox
                                          {
                                              s.StudentID,
                                              FullName = (s.Client.LastName + " " + s.Client.FirstName + " " + (s.Client.MiddleName ?? "")).Trim()
                                          })
                                          .ToList();
                    StudentComboBox.ItemsSource = students;
                    StudentComboBox.DisplayMemberPath = "FullName"; // Отображаем полное имя
                    StudentComboBox.SelectedValuePath = "StudentID";

                    // Загрузка предметов (курсов)
                    var subjects = context.Course.OrderBy(c => c.CourseName).ToList();
                    SubjectComboBox.ItemsSource = subjects;
                    IndividualSubjectComboBox.ItemsSource = subjects;
                    SubjectComboBox.DisplayMemberPath = "CourseName";
                    IndividualSubjectComboBox.DisplayMemberPath = "CourseName";
                    SubjectComboBox.SelectedValuePath = "CourseID";
                    IndividualSubjectComboBox.SelectedValuePath = "CourseID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для инициализации: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Установка текущей даты
            LessonDatePicker.SelectedDate = DateTime.Today;

            // Изначально скрываем выбор студента (по умолчанию групповой режим)
            GroupSelectionGrid.Visibility = Visibility.Visible;
            StudentSelectionGrid.Visibility = Visibility.Collapsed;
        }

        // --- Обработчики событий изменения выбора ---

        private void LessonType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && GroupSelectionGrid != null && StudentSelectionGrid != null) // Проверка на null важна во время инициализации
            {
                isIndividualLesson = radioButton == IndividualLessonRadio;
                GroupSelectionGrid.Visibility = isIndividualLesson ? Visibility.Collapsed : Visibility.Visible;
                StudentSelectionGrid.Visibility = isIndividualLesson ? Visibility.Visible : Visibility.Collapsed;

                // Очищаем таблицу и сбрасываем выбор при смене типа
                gradeRecords.Clear();
                GroupComboBox.SelectedIndex = -1;
                SubjectComboBox.SelectedIndex = -1;
                StudentComboBox.SelectedIndex = -1;
                IndividualSubjectComboBox.SelectedIndex = -1;
            }
        }

        // Общий метод для проверки готовности и загрузки данных
        private void CheckAndLoadData()
        {
            bool canLoad = false;
            if (isIndividualLesson)
            {
                // Для индивидуального: нужен студент, предмет и дата
                canLoad = StudentComboBox.SelectedValue != null &&
                          IndividualSubjectComboBox.SelectedValue != null &&
                          LessonDatePicker.SelectedDate.HasValue;
            }
            else
            {
                // Для группового: нужна группа, предмет и дата
                canLoad = GroupComboBox.SelectedValue != null &&
                          SubjectComboBox.SelectedValue != null &&
                          LessonDatePicker.SelectedDate.HasValue;
            }

            if (canLoad)
            {
                LoadAttendanceData();
            }
            else
            {
                // Если не все выбрано, очищаем таблицу
                gradeRecords.Clear();
            }
        }

        // Обработчики просто вызывают CheckAndLoadData
        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => CheckAndLoadData();
        private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => CheckAndLoadData();
        private void StudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => CheckAndLoadData();
        private void IndividualSubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => CheckAndLoadData();
        private void LessonDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => CheckAndLoadData();


        // --- Основная логика загрузки данных о посещаемости/оценках ---

        private void LoadAttendanceData()
        {
            if (LessonDatePicker.SelectedDate == null) return; // Доп. проверка

            DateTime lessonDate = LessonDatePicker.SelectedDate.Value.Date; // Берем только дату
            gradeRecords.Clear(); // Очищаем перед загрузкой

            Mouse.OverrideCursor = Cursors.Wait; // Индикатор загрузки
            try
            {
                // Создаем контекст здесь, чтобы он был доступен в вызываемых методах
                using (var context = new Dimploma1Entities())
                {
                    if (isIndividualLesson)
                    {
                        LoadIndividualLessonData(context, lessonDate);
                    }
                    else
                    {
                        LoadGroupLessonData(context, lessonDate);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                gradeRecords.Clear(); // Очищаем в случае ошибки
            }
            finally
            {
                Mouse.OverrideCursor = null; // Убираем индикатор загрузки
            }
        }

        // Загрузка данных для индивидуального занятия
        private void LoadIndividualLessonData(Dimploma1Entities context, DateTime lessonDate)
        {
            if (StudentComboBox.SelectedValue == null || IndividualSubjectComboBox.SelectedValue == null)
                return;

            int studentId = (int)StudentComboBox.SelectedValue;
            int courseId = (int)IndividualSubjectComboBox.SelectedValue;

            // Загружаем студента и клиента
            var student = context.Student
                                 .Include(s => s.Client)
                                 .FirstOrDefault(s => s.StudentID == studentId);

            if (student != null && student.Client != null)
            {
                // Ищем оценку
                var appraisalRecord = context.Appraisal
                    .FirstOrDefault(a => a.StudentID == studentId &&
                                         a.CourseID == courseId &&
                                         a.DateAssessment == lessonDate);

                string grade = appraisalRecord?.Appraisal1.ToString() ?? ""; // Используем Appraisal
                string note = appraisalRecord?.Cooment ?? ""; // Используем Cooment

                // Добавляем одну запись
                gradeRecords.Add(new GradeRecord
                {
                    Number = 1,
                    StudentID = student.StudentID,
                    StudentName = $"{student.Client.LastName} {student.Client.FirstName} {student.Client.MiddleName ?? ""}".Trim(),
                    Grade = grade,
                    Note = note,
                    AppraisalID = appraisalRecord?.AppraisalID
                });
            }
        }

        // Загрузка данных для группового занятия
        private void LoadGroupLessonData(Dimploma1Entities context, DateTime lessonDate)
        {
            if (GroupComboBox.SelectedValue == null || SubjectComboBox.SelectedValue == null)
                return;

            int groupId = (int)GroupComboBox.SelectedValue;
            int courseId = (int)SubjectComboBox.SelectedValue;

            // Загружаем студентов группы
            var studentsInGroup = context.GroupStudent
                .Where(gs => gs.GroupID == groupId)
                .Select(gs => gs.Student)
                .Include(s => s.Client)
                .Where(s => s != null && s.Client != null)
                .OrderBy(s => s.Client.LastName).ThenBy(s => s.Client.FirstName)
                .ToList();

            if (!studentsInGroup.Any())
            {
                // Можно показать сообщение или просто оставить таблицу пустой
                // MessageBox.Show("В выбранной группе нет студентов.");
                return;
            }

            int counter = 1;
            // Сначала создаем записи для всех студентов
            foreach (var student in studentsInGroup)
            {
                gradeRecords.Add(new GradeRecord
                {
                    Number = counter++,
                    StudentID = student.StudentID,
                    StudentName = $"{student.Client.LastName} {student.Client.FirstName} {student.Client.MiddleName ?? ""}".Trim(),
                    Grade = "", // По умолчанию оценки нет
                    Note = "",
                    AppraisalID = null
                });
            }

            // Затем ищем существующие оценки для этих студентов, курса и даты
            var studentIds = studentsInGroup.Select(s => s.StudentID).ToList();
            var existingAppraisals = context.Appraisal
                .Where(a => studentIds.Contains(a.StudentID.Value) && // Учитываем, что StudentID в Appraisal может быть NULL
                             a.CourseID == courseId &&
                             a.DateAssessment == lessonDate)
                .ToList();

            // Обновляем записи в gradeRecords найденными оценками
            foreach (var record in gradeRecords)
            {
                var appraisal = existingAppraisals.FirstOrDefault(a => a.StudentID == record.StudentID);
                if (appraisal != null)
                {
                    record.Grade = appraisal.Appraisal1.ToString() ?? ""; // Используем Appraisal
                    record.Note = appraisal.Cooment ?? ""; // Используем Cooment
                    record.AppraisalID = appraisal.AppraisalID;
                }
            }
        }


        // --- Логика сохранения ---

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gradeRecords.Any())
            {
                MessageBox.Show("Нет данных для сохранения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (LessonDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату занятия.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime lessonDate = LessonDatePicker.SelectedDate.Value.Date;
            int? courseId = isIndividualLesson
                ? (int?)IndividualSubjectComboBox.SelectedValue
                : (int?)SubjectComboBox.SelectedValue;

            if (courseId == null)
            {
                MessageBox.Show("Пожалуйста, выберите предмет.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // !!! ВАЖНО: Определить TeacherID !!!
            // Откуда брать ID преподавателя, который ставит оценку?
            // Возможно, из текущего залогиненного пользователя или другого источника.
            // Здесь заглушка:
            int? currentTeacherId = GetCurrentTeacherId(); // <- ЗАМЕНИТЕ НА ВАШУ ЛОГИКУ ПОЛУЧЕНИЯ ID ПРЕПОДАВАТЕЛЯ
            if (currentTeacherId == null)
            {
                MessageBox.Show("Не удалось определить преподавателя для сохранения оценок.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    foreach (var record in gradeRecords)
                    {
                        // Парсим оценку обратно в int? (или что у вас в БД)
                        // "Н", "Б", "" -> null
                        int? gradeValue = null;
                        if (int.TryParse(record.Grade, out int parsedGrade))
                        {
                            gradeValue = parsedGrade;
                        }
                        // Обработка нечисловых значений (если нужно хранить "Н", "Б" как спец. коды, логика будет сложнее)

                        // Ищем существующую запись оценки
                        Appraisal existingAppraisal = null;
                        if (record.AppraisalID.HasValue)
                        {
                            existingAppraisal = context.Appraisal.Find(record.AppraisalID.Value);
                        }
                        else
                        {
                            // Если ID не было, ищем по ключу (студент, курс, дата) на всякий случай
                            existingAppraisal = context.Appraisal.FirstOrDefault(a =>
                                a.StudentID == record.StudentID &&
                                a.CourseID == courseId.Value &&
                                a.DateAssessment == lessonDate);
                        }


                        // Определяем, нужно ли что-то делать
                        bool gradeEntered = gradeValue.HasValue || !string.IsNullOrWhiteSpace(record.Grade); // Учтем "Н", "Б" как введенные
                        bool noteEntered = !string.IsNullOrWhiteSpace(record.Note);


                        if (existingAppraisal != null)
                        {
                            // Запись существует
                            if (gradeEntered || noteEntered)
                            {
                                // Обновляем, если есть оценка или заметка
                                if (existingAppraisal.Appraisal1 != gradeValue || existingAppraisal.Cooment != record.Note)
                                {
                                    existingAppraisal.Appraisal1 = gradeValue;
                                    existingAppraisal.Cooment = record.Note;
                                    existingAppraisal.TeacherID = currentTeacherId; // Обновляем преподавателя
                                    context.Entry(existingAppraisal).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                // Оценка и заметка стерты - удаляем запись из БД
                                context.Appraisal.Remove(existingAppraisal);
                            }
                        }
                        else
                        {
                            // Записи не было
                            if (gradeEntered || noteEntered)
                            {
                                // Создаем новую, если есть оценка или заметка
                                var newAppraisal = new Appraisal
                                {
                                    StudentID = record.StudentID,
                                    CourseID = courseId.Value,
                                    DateAssessment = lessonDate,
                                    Appraisal1 = gradeValue,
                                    Cooment = record.Note,
                                    TeacherID = currentTeacherId // Устанавливаем преподавателя
                                };
                                context.Appraisal.Add(newAppraisal);
                            }
                        }
                    } // end foreach record

                    int changes = context.SaveChanges();
                    MessageBox.Show($"Сохранено изменений: {changes}", "Результат сохранения", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Перезагружаем данные, чтобы обновить AppraisalID у новых записей
                    LoadAttendanceData();
                } // end using context
            }
            catch (Exception ex)
            {
                // TODO: Более детальная обработка ошибок, включая DbEntityValidationException
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // --- Заглушка для получения ID преподавателя ---
        private int? GetCurrentTeacherId()
        {
            // !!! Реализуйте здесь логику получения ID текущего преподавателя !!!
            // Например, из данных аутентификации, выбранного элемента и т.д.
            // Возвращаем заглушку
            // Найдем первого попавшегося преподавателя для примера
            using (var context = new Dimploma1Entities())
            {
                // Предполагаем, что у вас есть таблица Teacher или Employee
                // Замените Teacher на Employee, если нужно
                var teacher = context.Employee.FirstOrDefault();
                return teacher?.EmployeeID; // Или teacher.EmployeeID
            }
            // return 1; // Или null, если определить не удалось
        }


        // --- Остальные методы (EditGrade_Click и т.п.) ---
        // Метод UpdateGrade больше не нужен в таком виде, логика в SaveButton_Click
        // Метод EditGrade_Click (если он используется для открытия окна редактирования):
        /*
        private void EditGrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is GradeRecord item)
            {
                // Открытие окна редактирования заметки или детальной информации
                // var editWindow = new EditGradeWindow(item); // Передаем всю запись
                // if (editWindow.ShowDialog() == true)
                // {
                //    // Если в окне были сделаны изменения, можно пометить запись
                //    // или просто предложить пользователю нажать "Сохранить"
                //    // Возможно, перезагрузка не нужна, если окно само обновляет 'item'
                //    // LoadAttendanceData(); // Перезагрузка после редактирования
                // }
                MessageBox.Show($"Редактирование для студента: {item.StudentName}\n(Функционал не реализован)", "Редактирование");
            }
        }
        */

    } // Конец класса Gradebook


    // --- Класс для хранения данных в строке DataGrid ---
    // (Оставьте его здесь или вынесите в отдельный файл)
    public class GradeRecord // Можно добавить : INotifyPropertyChanged для динамического обновления
    {
        public int Number { get; set; }
        public string StudentName { get; set; }
        public int StudentID { get; set; }
        public string Grade { get; set; } // Оставляем строкой для "Н", "Б" и т.д.
        public string Note { get; set; }
        public int? AppraisalID { get; set; } // ID существующей оценки в БД (nullable)
    }
}