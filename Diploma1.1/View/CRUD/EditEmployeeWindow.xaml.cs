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
    /// Логика взаимодействия для EditEmployeeWindow.xaml
    /// </summary>
    public partial class EditEmployeeWindow : Window
    {
        public Employee CurrentEmployee { get; set; }
        private bool IsAdding = false;

        public EditEmployeeWindow(Employee employee = null)
        {
            InitializeComponent();
            DataContext = this;

            if (employee == null)
            {
                CurrentEmployee = new Employee();
                IsAdding = true;
                Title = "Добавить Сотрудника";
            }
            else
            {
                CurrentEmployee = employee; // Consider loading fresh copy if needed
                IsAdding = false;
                Title = "Редактировать Сотрудника";
            }

            LoadPositions(); // Load data for ComboBox
        }

        // Load Positions into ComboBox
        private void LoadPositions()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    // Assuming you have a Position table/entity
                    PositionComboBox.ItemsSource = context.EmployeeRole.OrderBy(p => p.EmployeeRoleName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (string.IsNullOrWhiteSpace(CurrentEmployee.LastName) || string.IsNullOrWhiteSpace(CurrentEmployee.FirstName))
            {
                MessageBox.Show("Фамилия и Имя сотрудника обязательны.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CurrentEmployee.EmployeeRoleID == null || CurrentEmployee.EmployeeRoleID <= 0) // Check if Position is selected
            {
                MessageBox.Show("Пожалуйста, выберите должность.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                PositionComboBox.Focus();
                return;
            }

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.Employee.Add(CurrentEmployee);
                    }
                    else
                    {
                        context.Employee.Attach(CurrentEmployee);
                        context.Entry(CurrentEmployee).State = EntityState.Modified;
                        // If Position object is loaded, EF might try to save it too.
                        // Ensure only scalar properties or the FK (PositionID) are marked modified if needed.
                    }
                    context.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
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
