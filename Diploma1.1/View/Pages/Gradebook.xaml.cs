using Diploma1._1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Gradebook.xaml
    /// </summary>
    public partial class Gradebook : Page, INotifyPropertyChanged
    {
        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        // --- End INotifyPropertyChanged ---

        // --- Fields & Properties ---
        private readonly ObservableCollection<GradeRecord> _gradeRecords = new ObservableCollection<GradeRecord>();
        public ObservableCollection<GradeRecord> GradeRecords => _gradeRecords; // Bind ItemsSource to this

        // Store lists for ComboBoxes (no need for ObservableCollection here)
        private List<Group> _groupsList;
        private List<StudentInfo> _studentsList;
        private List<Course> _coursesList;
        // List for the Grade ComboBox column
        private List<string> _gradesList = new List<string> { "", "5", "4", "3", "2", "Н", "Б" };

        // Selected items (no need for full INPC properties if DataContext is this and changes trigger loads)
        private Group _selectedGroup;
        private StudentInfo _selectedStudent;
        private Course _selectedCourseGroup; // Course for group mode
        private Course _selectedCourseIndividual; // Course for individual mode
        private DateTime? _selectedDate = DateTime.Today;

        private bool _isGroupMode = true; // Matches XAML default IsChecked="True"
        private bool _isLoading = false;
        private string _statusMessage = ""; // For potential status bar (not in provided XAML)
        private bool _canSaveChanges = false;
        private int? _currentTeacherId;

        // Public properties for IsLoading/CanSave state if needed for binding button IsEnabled
        public bool IsLoading { get => _isLoading; private set => SetProperty(ref _isLoading, value); }
        public bool CanSaveChanges { get => _canSaveChanges; private set => SetProperty(ref _canSaveChanges, value); }
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
        // --- End Fields & Properties ---

        public Gradebook()
        {
            InitializeComponent();
            this.DataContext = this; // Set DataContext for potential future bindings

            // Assign collection to DataGrid
            GradesDataGrid.ItemsSource = _gradeRecords;

            // Get teacher ID
            _currentTeacherId = GetCurrentTeacherIdFromSomewhere();
            if (_currentTeacherId == null)
            {
                StatusMessage = "ОШИБКА: Не удалось определить преподавателя!";
                MessageBox.Show(StatusMessage, "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Disable functionality if needed (e.g., disable grid editing)
                GradesDataGrid.IsReadOnly = true;
            }

            // Load initial ComboBox data
            _ = LoadInitialDataAsync();
        }


        // --- Initial Loading ---
        private async Task LoadInitialDataAsync()
        {
            SetLoadingState(true, "Загрузка списков...");
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    context.Configuration.LazyLoadingEnabled = false;
                    _groupsList = await context.Group.AsNoTracking().OrderBy(g => g.GroupName).ToListAsync();
                    _studentsList = await context.Student.AsNoTracking()
                                          .Include(s => s.Client)
                                          .Where(s => s.Client != null)
                                          .OrderBy(s => s.Client.LastName).ThenBy(s => s.Client.FirstName)
                                          .Select(s => new StudentInfo
                                          {
                                              StudentID = s.StudentID,
                                              FullName = (s.Client.LastName + " " + s.Client.FirstName + " " + (s.Client.MiddleName ?? "")).Trim()
                                          })
                                          .ToListAsync();
                    _coursesList = await context.Course.AsNoTracking().OrderBy(c => c.CourseName).ToListAsync();
                }

                // Populate ComboBoxes (must be on UI thread)
                GroupComboBox.ItemsSource = _groupsList;
                GroupComboBox.DisplayMemberPath = "GroupName"; // Make sure Group has GroupName property
                GroupComboBox.SelectedValuePath = "GroupID"; // Make sure Group has GroupID property

                StudentComboBox.ItemsSource = _studentsList;
                StudentComboBox.DisplayMemberPath = "FullName";
                StudentComboBox.SelectedValuePath = "StudentID";

                SubjectComboBox.ItemsSource = _coursesList;
                SubjectComboBox.DisplayMemberPath = "CourseName"; // Make sure Course has CourseName
                SubjectComboBox.SelectedValuePath = "CourseID"; // Make sure Course has CourseID

                IndividualSubjectComboBox.ItemsSource = _coursesList;
                IndividualSubjectComboBox.DisplayMemberPath = "CourseName";
                IndividualSubjectComboBox.SelectedValuePath = "CourseID";

                // Populate DataGrid ComboBox Column
                // Accessing column by x:Name directly
                GradeColumn.ItemsSource = _gradesList;

                // Set initial date in DatePicker (already done in field init)
                LessonDatePicker.SelectedDate = _selectedDate;

                StatusMessage = "Выберите параметры для отображения оценок.";

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading initial data: {ex}");
                StatusMessage = $"Ошибка загрузки списков: {ex.Message}";
                MessageBox.Show(StatusMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        // --- Event Handlers from XAML ---

        private void LessonType_Checked(object sender, RoutedEventArgs e)
        {
            // Avoid null refs during initialization
            if (GroupSelectionGrid == null || StudentSelectionGrid == null) return;

            _isGroupMode = (GroupLessonRadio.IsChecked == true);

            // Toggle visibility of selection grids
            GroupSelectionGrid.Visibility = _isGroupMode ? Visibility.Visible : Visibility.Collapsed;
            StudentSelectionGrid.Visibility = !_isGroupMode ? Visibility.Visible : Visibility.Collapsed;

            // Clear selections and data grid
            GroupComboBox.SelectedIndex = -1;
            StudentComboBox.SelectedIndex = -1;
            SubjectComboBox.SelectedIndex = -1;
            IndividualSubjectComboBox.SelectedIndex = -1;
            _selectedGroup = null;
            _selectedStudent = null;
            _selectedCourseGroup = null;
            _selectedCourseIndividual = null;
            GradeRecords.Clear();
            StatusMessage = "Выберите параметры для загрузки оценок.";
            CheckCanSave();
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedGroup = GroupComboBox.SelectedItem as Group;
            _ = LoadGradeDataAsync(); // Trigger load
        }

        private void StudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStudent = StudentComboBox.SelectedItem as StudentInfo;
            _ = LoadGradeDataAsync(); // Trigger load
        }

        // Combined handler for both subject ComboBoxes
        private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == SubjectComboBox)
            {
                _selectedCourseGroup = SubjectComboBox.SelectedItem as Course;
            }
            else if (sender == IndividualSubjectComboBox)
            {
                _selectedCourseIndividual = IndividualSubjectComboBox.SelectedItem as Course;
            }
            _ = LoadGradeDataAsync(); // Trigger load
        }

        private void LessonDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDate = LessonDatePicker.SelectedDate;
            _ = LoadGradeDataAsync(); // Trigger load
        }

        // --- Data Loading for Grades ---

        private bool CanLoadGradeData()
        {
            if (IsLoading || _selectedDate == null) return false;
            Course currentCourse = _isGroupMode ? _selectedCourseGroup : _selectedCourseIndividual;
            if (currentCourse == null) return false;

            return (_isGroupMode && _selectedGroup != null) || (!_isGroupMode && _selectedStudent != null);
        }

        private async Task LoadGradeDataAsync()
        {
            if (!CanLoadGradeData())
            {
                GradeRecords.Clear();
                StatusMessage = "Выберите все параметры (группу/студента, предмет, дату).";
                CheckCanSave();
                return;
            }

            SetLoadingState(true, "Загрузка оценок...");
            GradeRecords.Clear();

            // Capture current filter values for the async operation
            DateTime lessonDate = _selectedDate.Value.Date;
            int courseId = (_isGroupMode ? _selectedCourseGroup.CourseID : _selectedCourseIndividual.CourseID);
            int? groupId = _isGroupMode ? _selectedGroup?.GroupID : null;
            int? studentId = !_isGroupMode ? _selectedStudent?.StudentID : null;


            try
            {
                using (var context = new Dimploma1Entities())
                {
                    context.Configuration.LazyLoadingEnabled = false;

                    List<Student> studentsToDisplay;
                    if (_isGroupMode && groupId.HasValue)
                    {
                        studentsToDisplay = await context.GroupStudent
                           .Where(gs => gs.GroupID == groupId.Value)
                           .Select(gs => gs.Student)
                           .Include(s => s.Client)
                           .Where(s => s != null && s.Client != null)
                           .OrderBy(s => s.Client.LastName).ThenBy(s => s.Client.FirstName)
                           .ToListAsync();
                    }
                    else if (!_isGroupMode && studentId.HasValue)
                    {
                        studentsToDisplay = await context.Student
                            .Include(s => s.Client)
                            .Where(s => s.StudentID == studentId.Value && s.Client != null)
                            .ToListAsync();
                    }
                    else { studentsToDisplay = new List<Student>(); }

                    if (!studentsToDisplay.Any())
                    {
                        StatusMessage = "Студенты не найдены.";
                        SetLoadingState(false); // Make sure loading state is reset
                        return;
                    }

                    var studentIds = studentsToDisplay.Select(s => s.StudentID).ToList();
                    // Load appraisals into a dictionary for quick lookup
                    var appraisalDict = await context.Appraisal.AsNoTracking()
                        .Where(a => a.StudentID.HasValue && studentIds.Contains(a.StudentID.Value) &&
                                    a.CourseID == courseId &&
                                    a.DateAssessment == lessonDate)
                        .ToDictionaryAsync(a => a.StudentID.Value);

                    int counter = 1;
                    foreach (var student in studentsToDisplay)
                    {
                        appraisalDict.TryGetValue(student.StudentID, out var appraisal);
                        var gradeRecord = new GradeRecord
                        {
                            Number = counter++,
                            StudentID = student.StudentID,
                            StudentName = $"{student.Client.LastName} {student.Client.FirstName} {student.Client.MiddleName ?? ""}".Trim(),
                            Grade = appraisal?.Appraisal1?.ToString() ?? "", // Use Appraisal
                            Note = appraisal?.Cooment ?? "",
                            AppraisalID = appraisal?.AppraisalID,
                            IsDirty = false // Start clean
                        };
                        // Subscribe to changes to enable Save button
                        gradeRecord.PropertyChanged += GradeRecord_PropertyChanged;
                        GradeRecords.Add(gradeRecord);
                    }
                    StatusMessage = $"Загружено {GradeRecords.Count} записей.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading grade data: {ex}");
                StatusMessage = $"Ошибка загрузки оценок: {ex.Message}";
                MessageBox.Show(StatusMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false); // Reset loading state and CanSave
            }
        }

        // --- Saving Logic ---

        private void CheckCanSave()
        {
            // Update CanSaveChanges based on IsLoading and IsDirty flags
            CanSaveChanges = !IsLoading && GradeRecords.Any(r => r.IsDirty);
        }

        private void GradeRecord_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // When Grade or Note changes in a record, check if we can save
            if (e.PropertyName == nameof(GradeRecord.Grade) || e.PropertyName == nameof(GradeRecord.Note))
            {
                if (sender is GradeRecord gr && !gr.IsDirty)
                {
                    gr.IsDirty = true;
                }
                CheckCanSave(); // Update save button state
            }
        }

        // Method to be called by your Save button's Click event
        public async void SaveButton_Click(object sender, RoutedEventArgs e) // Made public for button click
        {
            if (!CanSaveChanges || _currentTeacherId == null)
            {
                MessageBox.Show("Нет изменений для сохранения или не определен преподаватель.", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SetLoadingState(true, "Сохранение изменений...");

            // Capture current values needed for saving
            var changedRecords = GradeRecords.Where(r => r.IsDirty).ToList();
            if (!changedRecords.Any()) // Double check
            {
                SetLoadingState(false, "Нет изменений для сохранения.");
                return;
            }

            DateTime lessonDate = _selectedDate.Value.Date;
            int courseId = (_isGroupMode ? _selectedCourseGroup.CourseID : _selectedCourseIndividual.CourseID);
            int teacherId = _currentTeacherId.Value;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    var studentIdsToUpdate = changedRecords.Select(r => r.StudentID).Distinct().ToList();
                    // Load existing appraisals for changed students into dictionary *within this context*
                    var existingAppraisalsDict = await context.Appraisal
                        .Where(a => a.StudentID.HasValue && studentIdsToUpdate.Contains(a.StudentID.Value) &&
                                    a.CourseID == courseId &&
                                    a.DateAssessment == lessonDate)
                        .ToDictionaryAsync(a => a.StudentID.Value);

                    int added = 0, updated = 0, deleted = 0;

                    foreach (var record in changedRecords)
                    {
                        existingAppraisalsDict.TryGetValue(record.StudentID, out Appraisal existingAppraisal);

                        int? gradeValue = null;
                        if (int.TryParse(record.Grade, out int parsedGrade)) gradeValue = parsedGrade;
                        // Handle "Н", "Б", "" -> null automatically as TryParse fails or gradeValue remains null

                        bool gradeOrNoteEntered = !string.IsNullOrWhiteSpace(record.Grade) || !string.IsNullOrWhiteSpace(record.Note);

                        if (existingAppraisal != null)
                        { // Record exists in DB
                            if (gradeOrNoteEntered)
                            { // Update
                                if (existingAppraisal.Appraisal1 != gradeValue || existingAppraisal.Cooment != record.Note)
                                {
                                    existingAppraisal.Appraisal1 = gradeValue;
                                    existingAppraisal.Cooment = record.Note;
                                    existingAppraisal.TeacherID = teacherId;
                                    context.Entry(existingAppraisal).State = EntityState.Modified;
                                    updated++;
                                }
                            }
                            else
                            { // Delete
                                context.Appraisal.Remove(existingAppraisal);
                                deleted++;
                            }
                        }
                        else if (gradeOrNoteEntered)
                        { // Record doesn't exist in DB, Add new
                            var newAppraisal = new Appraisal
                            {
                                StudentID = record.StudentID,
                                CourseID = courseId,
                                DateAssessment = lessonDate,
                                Appraisal1 = gradeValue,
                                Cooment = record.Note,
                                TeacherID = teacherId
                            };
                            context.Appraisal.Add(newAppraisal);
                            added++;
                        }
                    }

                    int changes = await context.SaveChangesAsync();
                    StatusMessage = $"Сохранено. Добавлено: {added}, Обновлено: {updated}, Удалено: {deleted}. (Затронуто строк: {changes})";

                    // Mark saved records as not dirty
                    foreach (var record in changedRecords) record.IsDirty = false;
                    CheckCanSave(); // Update save button state

                    // Reload data to ensure AppraisalIDs are updated and UI reflects DB state
                    // Call LoadGradeDataAsync but without await here if you don't need to wait for it
                    // but be careful about race conditions if user clicks again quickly.
                    // A safer approach is await, even if slightly slower UI lock.
                    await LoadGradeDataAsync(); // Reload after save

                }
            }
            catch (DbEntityValidationException dbEx)
            {
                // Handle validation errors (same as before)
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Ошибка валидации при сохранении:");
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        sb.AppendLine($"Свойство: {validationError.PropertyName}, Ошибка: {validationError.ErrorMessage}");
                    }
                }
                Debug.WriteLine(sb.ToString());
                StatusMessage = "Ошибка валидации при сохранении.";
                MessageBox.Show(sb.ToString(), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving grades: {ex}");
                StatusMessage = $"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}";
                MessageBox.Show(StatusMessage, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false); // Reset loading state and CanSave
                Mouse.OverrideCursor = null;
            }
        }


        // --- Helper Methods ---

        private void SetLoadingState(bool isLoading, string status = null)
        {
            IsLoading = isLoading;
            if (status != null)
            {
                StatusMessage = status;
            }
            // Directly enabling/disabling controls might be more reliable
            // than relying solely on IsNotLoading binding if issues arise.
            // e.g., FilterPanel.IsEnabled = !isLoading; DataGrid.IsReadOnly = isLoading; SaveButton.IsEnabled = !isLoading && CanSaveChanges;
            CheckCanSave(); // Update CanSaveChanges which affects button IsEnabled via binding or direct setting
        }


        private int? GetCurrentTeacherIdFromSomewhere()
        {
            // !!! REPLACE WITH YOUR ACTUAL IMPLEMENTATION !!!
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    // Example: Find first employee
                    return context.Employee.Select(e => e.EmployeeID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get teacher ID: {ex.Message}");
                return null;
            }
        }

    } // End Gradebook Class


    // --- Helper class for Student ComboBox ---
    public class StudentInfo
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
    }


    // --- Data row class with INotifyPropertyChanged ---
    public class GradeRecord : INotifyPropertyChanged
    {
        // INotifyPropertyChanged Implementation (same as before)
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value; OnPropertyChanged(propertyName); return true;
        }

        private int _number;
        public int Number { get => _number; set => SetProperty(ref _number, value); }

        private string _studentName;
        public string StudentName { get => _studentName; set => SetProperty(ref _studentName, value); }

        private int _studentID;
        public int StudentID { get => _studentID; set => SetProperty(ref _studentID, value); }

        private string _grade;
        public string Grade { get => _grade; set { if (SetProperty(ref _grade, value)) IsDirty = true; } } // Mark dirty

        private string _note;
        public string Note { get => _note; set { if (SetProperty(ref _note, value)) IsDirty = true; } } // Mark dirty

        private int? _appraisalID;
        public int? AppraisalID { get => _appraisalID; internal set => SetProperty(ref _appraisalID, value); }

        private bool _isDirty;
        // Make IsDirty public get, internal set and notify changes
        public bool IsDirty { get => _isDirty; internal set => SetProperty(ref _isDirty, value); }
    }
}