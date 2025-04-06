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
    /// Логика взаимодействия для EditPaymentWindow.xaml
    /// </summary>
    public partial class EditPaymentWindow : Window
    {
        public Payment CurrentPayment { get; set; }
        private bool IsAdding = false;

        public EditPaymentWindow(Payment payment = null)
        {
            InitializeComponent();
            DataContext = this;

            if (payment == null)
            {
                CurrentPayment = new Payment { PaymentDate = DateTime.Now };
                IsAdding = true;
                Title = "Добавить Оплату";
            }
            else
            {
                CurrentPayment = payment;
                IsAdding = false;
                Title = "Редактировать Оплату";
            }

            LoadContracts();
            LoadPaymentMethods();
        }

        // Load Contracts (Simplified - needs better display)
        private void LoadContracts()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    // Load contracts with client info for better display
                    var contracts = context.Contract
                        .Include(c => c.Client)
                        .Select(c => new {
                            c.ContractID,
                            // Create a display string - adjust as needed
                            ContractIdentifier = "ID: " + c.ContractID + " (" + c.Client.LastName + " " + c.Client.FirstName + ")"
                        })
                        .OrderBy(c => c.ContractIdentifier)
                        .ToList();
                    ContractComboBox.ItemsSource = contracts;
                    // DisplayMemberPath is set in XAML to "ContractIdentifier"
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки договоров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load Payment Methods
        private void LoadPaymentMethods()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    // Assuming PaymentMethod entity exists
                    MethodComboBox.ItemsSource = context.PaymentMethod.OrderBy(pm => pm.PaymentMethodName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки методов оплаты: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (CurrentPayment.ContractID == null || CurrentPayment.ContractID <= 0)
            {
                MessageBox.Show("Выберите договор.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentPayment.PaymentDate == null)
            {
                MessageBox.Show("Укажите дату оплаты.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentPayment.Quantity == null || CurrentPayment.Quantity <= 0)
            {
                MessageBox.Show("Укажите положительную сумму оплаты.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentPayment.PaymentMethodID == null || CurrentPayment.PaymentMethodID <= 0)
            {
                MessageBox.Show("Выберите метод оплаты.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }


            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.Payment.Add(CurrentPayment);
                    }
                    else
                    {
                        context.Payment.Attach(CurrentPayment);
                        context.Entry(CurrentPayment).State = EntityState.Modified;
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
