using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Diploma1._1.View.CRUD
{
    /// <summary>
    /// Логика взаимодействия для EditStudentWindow.xaml
    /// </summary>
    public partial class EditStudentWindow : Window
    {
        public Student CurrentStudent { get; set; }
        private bool IsAdding = false;

        public EditStudentWindow(Student student = null)
        {
            InitializeComponent();
            DataContext = this;

            if (student == null)
            {
                CurrentStudent = new Student();
                IsAdding = true;
                Title = "Добавить Студента";
            }
            else
            {
                CurrentStudent = student;
                IsAdding = false;
                Title = "Редактировать Студента";
            }
            LoadClients();
        }

        // Load Clients into ComboBox
        private void LoadClients()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    ClientComboBox.ItemsSource = context.Client
                        .Select(c => new { c.ClientID, FullName = (c.LastName + " " + c.FirstName + " " + (c.MiddleName ?? "")).Trim() })
                        .OrderBy(c => c.FullName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentStudent.ClientID == null || CurrentStudent.ClientID <= 0)
            {
                MessageBox.Show("Выберите клиента.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            // Add more validation if needed (e.g., Date of Birth)

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        // Check if a student record for this client already exists? Optional.
                        bool exists = context.Student.Any(s => s.ClientID == CurrentStudent.ClientID);
                        if (exists)
                        {
                            MessageBox.Show("Студент для выбранного клиента уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        context.Student.Add(CurrentStudent);
                    }
                    else
                    {
                        context.Student.Attach(CurrentStudent);
                        context.Entry(CurrentStudent).State = EntityState.Modified;
                        // Usually ClientID shouldn't change once set, but allow it for now
                        // context.Entry(CurrentStudent).Property(x => x.ClientID).IsModified = false; // If ClientID is fixed
                    }
                    context.SaveChanges();
                }
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                // Handle potential unique constraint violation if ClientID must be unique in Student table
                MessageBox.Show($"Ошибка сохранения данных: {dbEx.InnerException?.InnerException?.Message ?? dbEx.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
