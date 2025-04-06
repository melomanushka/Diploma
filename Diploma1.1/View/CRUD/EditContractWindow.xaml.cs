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
    /// Логика взаимодействия для EditContractWindow.xaml
    /// </summary>
    public partial class EditContractWindow : Window
    {
        public Contract CurrentContract { get; set; }
        private bool IsAdding = false;

        public EditContractWindow(Contract contract = null)
        {
            InitializeComponent();
            DataContext = this;

            if (contract == null)
            {
                CurrentContract = new Contract { DateCreate = DateTime.Now };
                IsAdding = true;
                Title = "Добавить Договор";
            }
            else
            {
                CurrentContract = contract;
                IsAdding = false;
                Title = "Редактировать Договор";
            }

            LoadComboBoxData();
        }

        // Load data for all ComboBoxes
        private void LoadComboBoxData()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    // Load Clients
                    ClientComboBox.ItemsSource = context.Client
                        .Select(c => new { c.ClientID, FullName = (c.LastName + " " + c.FirstName + " " + (c.MiddleName ?? "")).Trim() })
                        .OrderBy(c => c.FullName).ToList();

                    // Load Students (with Client names)
                    StudentComboBox.ItemsSource = context.Student
                        .Include(s => s.Client)
                        .Where(s => s.Client != null)
                        .Select(s => new { s.StudentID, FullName = (s.Client.LastName + " " + s.Client.FirstName + " " + (s.Client.MiddleName ?? "")).Trim() })
                        .OrderBy(s => s.FullName).ToList();

                    // Load Prices (with Course names - simplified identifier)
                    PriceComboBox.ItemsSource = context.Price
                        .Include(p => p.Course)
                        .Select(p => new { p.PriceID, PriceIdentifier = (p.Course.CourseName + " - " + p.PriceQuantity) })
                        .OrderBy(p => p.PriceIdentifier).ToList();

                    // Load Forms of Study
                    FormComboBox.ItemsSource = context.FormOfStudy.OrderBy(f => f.FormOfStudyName).ToList();

                    // Load Contract Statuses
                    StatusComboBox.ItemsSource = context.StatusContract.OrderBy(s => s.StatusContractName).ToList();

                    // Load Requisites (Optional - if needed and table exists)
                    // RequisiteComboBox.ItemsSource = context.Requisite.ToList();
                    // RequisiteComboBox.IsEnabled = (RequisiteComboBox.Items.Count > 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных для списков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // --- !!! Add Extensive Validation Here !!! ---
            if (CurrentContract.ClientID == null || CurrentContract.StudentID == null || CurrentContract.PriceID == null ||
                CurrentContract.FormOfStudyID == null || CurrentContract.StatusContractID == null ||
                CurrentContract.DateStart == null || CurrentContract.DateEnd == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля (Клиент, Студент, Цена/Курс, Даты, Форма, Статус).", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CurrentContract.DateStart > CurrentContract.DateEnd)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --- End Validation ---

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        CurrentContract.DateCreate = DateTime.Now; // Ensure creation date
                        context.Contract.Add(CurrentContract);
                    }
                    else
                    {
                        context.Contract.Attach(CurrentContract);
                        context.Entry(CurrentContract).State = EntityState.Modified;
                        // Prevent changing DateCreate on edit
                        context.Entry(CurrentContract).Property(x => x.DateCreate).IsModified = false;
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
