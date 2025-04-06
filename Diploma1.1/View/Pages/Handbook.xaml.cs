using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Handbook.xaml
    /// </summary>
    public partial class Handbook : Page
    {
        private readonly Dimploma1Entities context;
        private string currentTable;

        public Handbook()
        {
            InitializeComponent();
            context = new Dimploma1Entities();
            TablesListBox.SelectedIndex = 0;
        }

        private void TablesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TablesListBox.SelectedItem != null)
            {
                currentTable = ((ListBoxItem)TablesListBox.SelectedItem).Content.ToString();
                ConfigureGrid();
                LoadData();
            }
        }

        private void ConfigureGrid()
        {
            DataGrid.Columns.Clear();

            switch (currentTable)
            {
                case "Источники информации":
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("InformationSourceID") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new System.Windows.Data.Binding("SourceName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new System.Windows.Data.Binding("Description") });
                    break;

                case "Сотрудники":
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("EmployeeID") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new System.Windows.Data.Binding("LastName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new System.Windows.Data.Binding("FirstName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new System.Windows.Data.Binding("MiddleName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Должность", Binding = new System.Windows.Data.Binding("Position") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email") });
                    break;

                case "Клиенты":
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("ClientID") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new System.Windows.Data.Binding("LastName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new System.Windows.Data.Binding("FirstName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new System.Windows.Data.Binding("MiddleName") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone") });
                    DataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email") });
                    break;

                // Добавьте конфигурацию для остальных таблиц
            }
        }

        private void LoadData()
        {
            try
            {
                switch (currentTable)
                {
                    case "Источники информации":
                        DataGrid.ItemsSource = context.InformationSource.ToList();
                        break;

                    case "Сотрудники":
                        DataGrid.ItemsSource = context.Employee.ToList();
                        break;

                    case "Клиенты":
                        DataGrid.ItemsSource = context.Client.ToList();
                        break;

                    case "Скидки":
                        DataGrid.ItemsSource = context.Discount.ToList();
                        break;

                    case "Оплаты":
                        DataGrid.ItemsSource = context.Payment.ToList();
                        break;

                    case "Договоры":
                        DataGrid.ItemsSource = context.Contract.ToList();
                        break;

                    case "Цены":
                        DataGrid.ItemsSource = context.Price.ToList();
                        break;

                    case "Типы занятий":
                        DataGrid.ItemsSource = context.TypeLesson.ToList();
                        break;

                    case "Курсы":
                        DataGrid.ItemsSource = context.Course.ToList();
                        break;

                    case "Кабинеты":
                        DataGrid.ItemsSource = context.Cabinet.ToList();
                        break;

                    case "Студенты":
                        DataGrid.ItemsSource = context.Student.ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = CreateEditWindow(null);
            if (editWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DataGrid.SelectedItem;
            if (selectedItem != null)
            {
                var editWindow = CreateEditWindow(selectedItem);
                if (editWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования");
            }
        }

        private Window CreateEditWindow(object item)
        {
            Window editWindow = null;

            switch (currentTable)
            {
                case "Источники информации":
                    //editWindow = new EditInformationSourceWindow(item as InformationSource);
                    break;

                case "Сотрудники":
                    //editWindow = new EditEmployeeWindow(item as Employee);
                    break;

                case "Клиенты":
                    //editWindow = new EditClientWindow(item as Client);
                    break;

                // Добавьте создание окон редактирования для остальных таблиц
            }

            return editWindow;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DataGrid.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", 
                                       "Подтверждение удаления", 
                                       MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    context.Entry(selectedItem).State = EntityState.Deleted;
                    context.SaveChanges();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении записи: {ex.Message}");
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);
            
            if (string.IsNullOrEmpty(searchText))
            {
                view.Filter = null;
                return;
            }

            view.Filter = item =>
            {
                // Реализуйте поиск в зависимости от текущей таблицы
                switch (currentTable)
                {
                    case "Источники информации":
                        var source = item as InformationSource;
                        return source.InformationSourceName.ToLower().Contains(searchText);

                    case "Сотрудники":
                        var employee = item as Employee;
                        return employee.LastName.ToLower().Contains(searchText) ||
                               employee.FirstName.ToLower().Contains(searchText);

                    // Добавьте фильтрацию для остальных таблиц
                    default:
                        return true;
                }
            };
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditButton.IsEnabled = DeleteButton.IsEnabled = DataGrid.SelectedItem != null;
        }
    }
}
