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
    /// Логика взаимодействия для EditPriceWindow.xaml
    /// </summary>
    public partial class EditPriceWindow : Window
    {
        public Price CurrentPrice { get; set; }
        private bool IsAdding = false;

        public EditPriceWindow(Price price = null)
        {
            InitializeComponent();
            DataContext = this;

            if (price == null)
            {
                CurrentPrice = new Price();
                IsAdding = true;
                Title = "Добавить Цену";
            }
            else
            {
                CurrentPrice = price;
                IsAdding = false;
                Title = "Редактировать Цену";
            }
            LoadCourses();
        }

        private void LoadCourses()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    CourseComboBox.ItemsSource = context.Course.OrderBy(c => c.CourseName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (CurrentPrice.CourseID == null || CurrentPrice.CourseID <= 0)
            {
                MessageBox.Show("Выберите курс.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentPrice.PriceQuantity == null || CurrentPrice.PriceQuantity <= 0) // Or CostPerHour? Add validation as needed
            {
                MessageBox.Show("Общая цена должна быть положительной.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            // Add date validation?

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.Price.Add(CurrentPrice);
                    }
                    else
                    {
                        context.Price.Attach(CurrentPrice);
                        context.Entry(CurrentPrice).State = EntityState.Modified;
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
