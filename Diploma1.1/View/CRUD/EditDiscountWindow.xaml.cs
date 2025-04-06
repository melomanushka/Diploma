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
    /// Логика взаимодействия для EditDiscountWindow.xaml
    /// </summary>
    public partial class EditDiscountWindow : Window
    {
        public Discount CurrentDiscount { get; set; }
        private bool IsAdding = false;

        public EditDiscountWindow(Discount discount = null)
        {
            InitializeComponent();
            DataContext = this;

            if (discount == null)
            {
                CurrentDiscount = new Discount();
                IsAdding = true;
                Title = "Добавить Скидку";
            }
            else
            {
                CurrentDiscount = discount;
                IsAdding = false;
                Title = "Редактировать Скидку";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (string.IsNullOrWhiteSpace(CurrentDiscount.DiscountName))
            {
                MessageBox.Show("Название скидки обязательно.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CurrentDiscount.DiscountValue== null || CurrentDiscount.DiscountValue <= 0)
            {
                MessageBox.Show("Процент скидки должен быть положительным числом.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                PercentTextBox.Focus(); // Assuming TextBox name
                return;
            }
            // Add date validation (start <= end?) if needed

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.Discount.Add(CurrentDiscount);
                    }
                    else
                    {
                        context.Discount.Attach(CurrentDiscount);
                        context.Entry(CurrentDiscount).State = EntityState.Modified;
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
