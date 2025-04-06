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
    /// Логика взаимодействия для EditClientWindow.xaml
    /// </summary>
    public partial class EditClientWindow : Window
    {
        public Client CurrentClient { get; set; }
        private bool IsAdding = false;

        public EditClientWindow(Client client = null)
        {
            InitializeComponent();
            DataContext = this;

            if (client == null)
            {
                CurrentClient = new Client { CreationDate = DateTime.Now }; // Set creation date for new client
                IsAdding = true;
                Title = "Добавить Клиента";
            }
            else
            {
                CurrentClient = client;
                IsAdding = false;
                Title = "Редактировать Клиента";
            }

            LoadSources(); // Load data for ComboBox
        }

        // Load Information Sources into ComboBox
        private void LoadSources()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    SourceComboBox.ItemsSource = context.InformationSource.OrderBy(s => s.InformationSourceName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки источников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (string.IsNullOrWhiteSpace(CurrentClient.LastName) || string.IsNullOrWhiteSpace(CurrentClient.FirstName))
            {
                MessageBox.Show("Фамилия и Имя клиента обязательны.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Add more validation (phone format, email format, source selected?)

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        CurrentClient.CreationDate = DateTime.Now; // Ensure creation date is set
                        context.Client.Add(CurrentClient);
                    }
                    else
                    {
                        // Prevent changing CreationDate on edit
                        context.Client.Attach(CurrentClient);
                        context.Entry(CurrentClient).State = EntityState.Modified;
                        context.Entry(CurrentClient).Property(x => x.CreationDate).IsModified = false;
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
