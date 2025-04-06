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
    /// Логика взаимодействия для EditCabinetWindow.xaml
    /// </summary>
    public partial class EditCabinetWindow : Window
    {
        public Cabinet CurrentCabinet { get; set; }
        private bool IsAdding = false;

        public EditCabinetWindow(Cabinet cabinet = null)
        {
            InitializeComponent();
            DataContext = this;

            if (cabinet == null)
            {
                CurrentCabinet = new Cabinet();
                IsAdding = true;
                Title = "Добавить Кабинет";
            }
            else
            {
                CurrentCabinet = cabinet;
                IsAdding = false;
                Title = "Редактировать Кабинет";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentCabinet.CabinetName))
            {
                MessageBox.Show("Название/номер кабинета обязательно.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentCabinet.Interchangeability == null || CurrentCabinet.Interchangeability <= 0)
            {
                MessageBox.Show("Вместимость должна быть положительным числом.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            // Add validation

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.Cabinet.Add(CurrentCabinet);
                    }
                    else
                    {
                        context.Cabinet.Attach(CurrentCabinet);
                        context.Entry(CurrentCabinet).State = EntityState.Modified;
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
