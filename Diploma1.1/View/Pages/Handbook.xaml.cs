using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // Needed for Binding
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;
using System.Data.Entity.Infrastructure; // For DbUpdateException
using Diploma1._1.Model; // Assuming EF models are here
using Diploma1._1.View.CRUD;
using System.Collections; // Assuming Edit Windows are here

namespace Diploma1._1.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для Handbook.xaml
    /// </summary>
    public partial class Handbook : Page
    {
        private Dimploma1Entities context; // Use using block or dispose properly
        private string currentTable;

        public Handbook()
        {
            InitializeComponent();
            // Consider creating context within using blocks or managing its lifetime properly
            context = new Dimploma1Entities();
            // Add Loaded event handler to ensure controls are ready
            this.Loaded += Handbook_Loaded;
            this.Unloaded += Page_Unloaded; // Add handler to dispose context
        }

        // Use Loaded event to ensure ListBox is ready before setting SelectedIndex
        private void Handbook_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TablesListBox.Items.Count > 0 && TablesListBox.SelectedIndex == -1) // Select only if not already selected
                {
                    TablesListBox.SelectedIndex = 0; // Select the first item to load initially
                }
                else if (TablesListBox.SelectedIndex != -1) // If already selected, just load data
                {
                    // Initial load if an item is pre-selected (might happen)
                    if (TablesListBox.SelectedItem is ListBoxItem selectedListBoxItem)
                    {
                        currentTable = selectedListBoxItem.Content.ToString();
                        ConfigureGrid(); // Configure columns first
                        LoadData();      // Then load data
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации выбора таблицы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Remove handler after first load if only needed once (optional)
            // this.Loaded -= Handbook_Loaded;
        }


        private void TablesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TablesListBox.SelectedItem is ListBoxItem selectedListBoxItem)
            {
                currentTable = selectedListBoxItem.Content.ToString();
                ConfigureGrid(); // Configure columns first
                LoadData();      // Then load data
                SearchBox.Text = ""; // Clear search on table change
            }
        }

        private void ConfigureGrid()
        {
            DataGrid.Columns.Clear();
            DataGrid.AutoGenerateColumns = false; // Disable auto-generation for manual control

            try
            {
                switch (currentTable)
                {
                    case "Источники информации":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("InformationSourceID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("InformationSourceName") });
                        break;

                    case "Сотрудники": // Includes Teachers if they are in Employee table
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("EmployeeID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new Binding("LastName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new Binding("FirstName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new Binding("MiddleName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Должность", Binding = new Binding("EmployeeRole.EmployeeRoleName") }); // Assuming 'Position' navigation property
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new Binding("PhoneNumber") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("Email") });
                        break;

                    case "Клиенты":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("ClientID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new Binding("LastName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new Binding("FirstName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new Binding("MiddleName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new Binding("PhoneNumber") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("Email") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата Создания", Binding = new Binding("CreationDate") { StringFormat = "dd.MM.yyyy" } });
                        // Display Information Source Name (Requires Include in LoadData)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Источник", Binding = new Binding("InformationSource.InformationSourceName") });
                        break;

                    case "Скидки":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("DiscountID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("DiscountName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "%", Binding = new Binding("DiscountValue") { StringFormat = "N2" } }); // Format as number
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new Binding("Description"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Начало", Binding = new Binding("DateStart") { StringFormat = "dd.MM.yyyy" } });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Окончание", Binding = new Binding("DateEnd") { StringFormat = "dd.MM.yyyy" } });
                        break;

                    case "Оплаты":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("PaymentID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID Договора", Binding = new Binding("ContractID") });
                        // Display Client Name via Contract (Requires Include in LoadData)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Клиент", Binding = new Binding("Contract.Client.LastName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата Оплаты", Binding = new Binding("PaymentDate") { StringFormat = "dd.MM.yyyy HH:mm" } });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("Quantity") { StringFormat = "C2" } }); // Format as currency
                        // Display Payment Method Name (Requires Include in LoadData)
                        // *** Check your Model: Is the navigation property called 'PaymentMethod'? ***
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Метод Оплаты", Binding = new Binding("PaymentMethod.PaymentMethodName") }); // Assuming PaymentMethod table and MethodName field
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Комментарий", Binding = new Binding("Note"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                        break;

                    case "Договоры":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("ContractID"), IsReadOnly = true });
                        // Display Client/Student Names (Requires Include in LoadData)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Клиент", Binding = new Binding("Client.LastName") }); // Show LastName for brevity
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Студент", Binding = new Binding("Student.Client.LastName") }); // Assumes Student -> Client -> LastName
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата Создания", Binding = new Binding("DateCreate") { StringFormat = "dd.MM.yyyy" } });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Начало", Binding = new Binding("DateStart") { StringFormat = "dd.MM.yyyy" } });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Окончание", Binding = new Binding("DateEnd") { StringFormat = "dd.MM.yyyy" } });
                        // Display Status Name (Requires Include in LoadData)
                        // *** Check your Model: Is the navigation property called 'StatusContract'? ***
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("StatusContract.StatusContractName") });
                        // Display Form of Study Name (Requires Include in LoadData)
                        // *** Check your Model: Is the navigation property called 'FormOfStudy'? ***
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Форма", Binding = new Binding("FormOfStudy.FormOfStudyName") });
                        // Display Course Name (Requires Include via Price)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Курс", Binding = new Binding("Price.Course.CourseName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Цена ID", Binding = new Binding("PriceID") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Реквизиты ID", Binding = new Binding("RequisiteID") });
                        break;

                    case "Цены":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("PriceID"), IsReadOnly = true });
                        // Display Course Name (Requires Include in LoadData)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Курс", Binding = new Binding("Course.CourseName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Цена/час", Binding = new Binding("PricePerLesson") { StringFormat = "C2" } });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Цена общ.", Binding = new Binding("PriceQuantity") { StringFormat = "C2" } });
                        break;

                    case "Типы занятий": // Assuming this maps to TypeCourse
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("TypeCourseID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Название типа", Binding = new Binding("TypeCourseName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                        break;

                    case "Курсы":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("CourseID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("CourseName") });
                        // Display Type Name (Requires Include in LoadData)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Тип", Binding = new Binding("TypeCourse.TypeCourseName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Часы", Binding = new Binding("QuantityHours") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new Binding("Description"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                        break;

                    case "Кабинеты":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("CabinetID"), IsReadOnly = true });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Название/Номер", Binding = new Binding("CabinetName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Вместимость", Binding = new Binding("Interchangeability") });
                        break;

                    case "Студенты":
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("StudentID"), IsReadOnly = true });
                        // Display Client info (Requires Include in LoadData)
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new Binding("Client.LastName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new Binding("Client.FirstName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new Binding("Client.MiddleName") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата Рождения", Binding = new Binding("Birthdate") { StringFormat = "dd.MM.yyyy" } });
                        // Include Client Phone/Email if needed
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new Binding("Client.PhoneNumber") });
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("Client.Email") });
                        break;
                    default:
                        // Можно добавить пустой столбец с сообщением
                        DataGrid.Columns.Add(new DataGridTextColumn { Header = "Таблица не настроена", Binding = null, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка конфигурации таблицы '{currentTable}': {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            object itemsSource = null; // Use object to store different list types
            try
            {
                switch (currentTable)
                {
                    case "Источники информации":
                        itemsSource = context.InformationSource.AsNoTracking().ToList(); // Use AsNoTracking for read-only views
                        break;

                    case "Сотрудники":
                        // Include Position for display
                        // *** Check navigation property name: Position or EmployeeRole? ***
                        itemsSource = context.Employee.AsNoTracking().Include(e => e.EmployeeRole).ToList(); // Assuming 'Position'
                        break;

                    case "Клиенты":
                        // Include InformationSource for display
                        itemsSource = context.Client.AsNoTracking().Include(c => c.InformationSource).ToList();
                        break;

                    case "Скидки":
                        itemsSource = context.Discount.AsNoTracking().ToList();
                        break;

                    case "Оплаты":
                        // Include PaymentMethod and Contract->Client for display
                        // *** Check navigation property name: PaymentMethod? ***
                        itemsSource = context.Payment.AsNoTracking()
                                       .Include(p => p.PaymentMethod) // Assuming 'PaymentMethod'
                                       .Include(p => p.Contract.Client)
                                       .ToList();
                        break;

                    case "Договоры":
                        // Include related entities for display
                        // *** Check navigation property names: StatusContract, FormOfStudy? ***
                        itemsSource = context.Contract.AsNoTracking()
                                       .Include(c => c.Client)
                                       .Include(c => c.Student.Client)
                                       .Include(c => c.StatusContract) // Assuming 'StatusContract'
                                       .Include(c => c.FormOfStudy)    // Assuming 'FormOfStudy'
                                       .Include(c => c.Price.Course)
                                       .ToList();
                        break;

                    case "Цены":
                        // Include Course for display
                        itemsSource = context.Price.AsNoTracking().Include(p => p.Course).ToList();
                        break;

                    case "Типы занятий": // Assuming maps to TypeCourse
                        itemsSource = context.TypeCourse.AsNoTracking().ToList();
                        break;

                    case "Курсы":
                        // Include TypeCourse for display
                        itemsSource = context.Course.AsNoTracking().Include(c => c.TypeCourse).ToList();
                        break;

                    case "Кабинеты":
                        itemsSource = context.Cabinet.AsNoTracking().ToList();
                        break;

                    case "Студенты":
                        // Include Client for display
                        itemsSource = context.Student.AsNoTracking().Include(s => s.Client).ToList();
                        break;

                    // --- Добавьте другие таблицы при необходимости ---
                    // case "ТаблицаX":
                    // itemsSource = context.ТаблицаX.AsNoTracking().Include(...).ToList();
                    // break;

                    default:
                        itemsSource = null; // No data for unconfigured tables
                        break;
                }

                // *** FIX: Cast itemsSource to IEnumerable ***
                DataGrid.ItemsSource = itemsSource as IEnumerable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для '{currentTable}': {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                DataGrid.ItemsSource = null; // Clear grid on error
            }
            finally
            {
                Mouse.OverrideCursor = null;
                // Update button states after loading
                DataGrid_SelectionChanged(null, null);
            }
        }

        // Helper method to detach all tracked entities from the context (if reusing context)
        private void ResetChanges()
        {
            var changedEntries = context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged; // Or reload original values if needed
                        break;
                }
            }
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the appropriate edit window for adding a new item
            Window editWindow = CreateEditWindow(null); // Pass null for new item
            if (editWindow != null)
            {
                // ShowDialog returns true if the user saved (usually OK/Save button)
                if (editWindow.ShowDialog() == true)
                {
                    LoadData(); // Reload data to show the newly added item
                }
            }
            else
            {
                MessageBox.Show($"Окно добавления для таблицы '{currentTable}' не настроено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DataGrid.SelectedItem;
            if (selectedItem != null)
            {
                // --- НАЧАЛО ИСПРАВЛЕННОГО БЛОКА ---
                try // Обернем создание окна в try-catch на всякий случай
                {
                    // Просто передаем выбранный элемент. Окно редактирования
                    // само будет отвечать за работу с контекстом БД.
                    Window editWindow = CreateEditWindow(selectedItem);

                    if (editWindow != null)
                    {
                        if (editWindow.ShowDialog() == true)
                        {
                            // Перезагружаем данные в DataGrid после успешного редактирования
                            LoadData();
                        }
                        // else: Пользователь нажал "Отмена" в окне редактирования
                    }
                    else
                    {
                        MessageBox.Show($"Окно редактирования для таблицы '{currentTable}' не настроено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии окна редактирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // --- КОНЕЦ ИСПРАВЛЕННОГО БЛОКА ---
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись для редактирования.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Factory method to create the correct edit window based on the current table
        private Window CreateEditWindow(object item)
        {
            Window editWindow = null;
            bool isAdding = (item == null); // Flag to indicate add mode

            try
            {
                switch (currentTable)
                {
                    case "Источники информации":
                        editWindow = new EditInformationSourceWindow(item as InformationSource);
                        break;
                    case "Сотрудники":
                        editWindow = new EditEmployeeWindow(item as Employee);
                        break;
                    case "Клиенты":
                        editWindow = new EditClientWindow(item as Client);
                        break;
                    case "Скидки":
                        editWindow = new EditDiscountWindow(item as Discount);
                        break;
                    case "Оплаты":
                        editWindow = new EditPaymentWindow(item as Payment);
                        break;
                    case "Договоры":
                        editWindow = new EditContractWindow(item as Contract);
                        break;
                    case "Цены":
                        editWindow = new EditPriceWindow(item as Price);
                        break;
                    case "Типы занятий": // TypeCourse
                        editWindow = new EditTypeCourseWindow(item as TypeCourse);
                        break;
                    case "Курсы":
                        editWindow = new EditCourseWindow(item as Course);
                        break;
                    case "Кабинеты":
                        editWindow = new EditCabinetWindow(item as Cabinet);
                        break;
                    case "Студенты":
                        editWindow = new EditStudentWindow(item as Student);
                        break;
                    // --- Добавьте создание других окон ---
                    // case "ТаблицаX":
                    // editWindow = new EditТаблицаXWindow(item as ТаблицаX);
                    // break;
                    default:
                        // editWindow remains null if no configuration exists
                        break;
                }

                // Set window title based on add/edit mode
                if (editWindow != null)
                {
                    editWindow.Title = isAdding ? $"Добавить - {currentTable}" : $"Редактировать - {currentTable}";
                    // Optionally set Owner for better dialog behavior
                    // editWindow.Owner = Window.GetWindow(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании окна редактирования для '{currentTable}':\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null; // Return null on error
            }

            return editWindow;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DataGrid.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // More specific confirmation message
            string itemDescription = GetItemDescription(selectedItem); // Helper to get a user-friendly description
            var result = MessageBox.Show($"Вы уверены, что хотите удалить запись?\n\n{itemDescription}",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    // Use a fresh context for the delete operation for safety
                    using (var deleteContext = new Dimploma1Entities())
                    {
                        // Attach the item to the new context and set its state to Deleted
                        var entry = deleteContext.Entry(selectedItem);
                        if (entry.State == EntityState.Detached)
                        {
                            // Need to find the correct DbSet to attach or Remove
                            var dbSet = deleteContext.Set(selectedItem.GetType());
                            dbSet.Attach(selectedItem); // Attach first
                        }
                        // Now mark as deleted
                        deleteContext.Entry(selectedItem).State = EntityState.Deleted;

                        // Or use the DbSet.Remove method which handles attaching automatically (often preferred)
                        // deleteContext.Set(selectedItem.GetType()).Remove(selectedItem);

                        deleteContext.SaveChanges();
                    } // Context is disposed

                    LoadData(); // Reload data after successful deletion
                    MessageBox.Show("Запись успешно удалена.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateException dbEx) // Catch specific EF update exceptions
                {
                    // Provide a more user-friendly FK constraint message
                    MessageBox.Show($"Ошибка при удалении записи: Невозможно удалить запись, так как она используется в других таблицах или связана с другими данными.\n\n(Подробности: {dbEx.InnerException?.InnerException?.Message ?? dbEx.InnerException?.Message ?? dbEx.Message})",
                                    "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        // Helper to get a user-friendly description of the selected item for the delete confirmation
        private string GetItemDescription(object item)
        {
            if (item == null) return "";
            // Add more specific descriptions based on type
            try
            {
                switch (item)
                {
                    case InformationSource s: return $"Источник: {s.InformationSourceName} (ID: {s.InformationSourceID})";
                    case Employee e: return $"Сотрудник: {e.LastName} {e.FirstName} (ID: {e.EmployeeID})";
                    case Client c: return $"Клиент: {c.LastName} {c.FirstName} (ID: {c.ClientID})";
                    case Discount d: return $"Скидка: {d.DiscountName} ({d.DiscountValue}%) (ID: {d.DiscountID})";
                    case Payment p: return $"Оплата: {p.Quantity:C2} от {p.PaymentDate:d} (ID: {p.PaymentID})";
                    case Contract con: return $"Договор ID: {con.ContractID} (Клиент ID: {con.ClientID})";
                    case Price pr: return $"Цена: {pr.PriceQuantity:C2} (Курс ID: {pr.CourseID}, ID: {pr.PriceID})";
                    case TypeCourse tc: return $"Тип курса: {tc.TypeCourseName} (ID: {tc.TypeCourseID})";
                    case Course crs: return $"Курс: {crs.CourseName} (ID: {crs.CourseID})";
                    case Cabinet cab: return $"Кабинет: {cab.CabinetName} (ID: {cab.CabinetID})";
                    case Student stu: return $"Студент ID: {stu.StudentID} (Клиент ID: {stu.ClientID})";
                    default: return item.ToString(); // Fallback
                }
            }
            catch { return item.ToString(); } // Fallback on error
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Use try-catch as filtering can sometimes cause issues
            try
            {
                string searchText = SearchBox.Text.ToLower().Trim();
                var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);

                if (view == null) return; // Exit if no view (e.g., ItemsSource is null)

                if (string.IsNullOrEmpty(searchText))
                {
                    view.Filter = null; // Remove filter if search box is empty
                }
                else
                {
                    view.Filter = item =>
                    {
                        if (item == null) return false;
                        // Implement filtering logic based on the current table
                        switch (currentTable)
                        {
                            case "Источники информации":
                                var source = item as InformationSource;
                                return source?.InformationSourceName?.ToLower().Contains(searchText) == true;

                            case "Сотрудники":
                                var employee = item as Employee;
                                // *** Check navigation property name: Position or EmployeeRole? ***
                                string positionName = employee?.EmployeeRole?.EmployeeRoleName?.ToLower() ?? ""; // Assuming 'Position'
                                return employee?.LastName?.ToLower().Contains(searchText) == true ||
                                       employee?.FirstName?.ToLower().Contains(searchText) == true ||
                                       employee?.MiddleName?.ToLower().Contains(searchText) == true ||
                                       employee?.PhoneNumber?.ToLower().Contains(searchText) == true ||
                                       employee?.Email?.ToLower().Contains(searchText) == true ||
                                       positionName.Contains(searchText);

                            case "Клиенты":
                                var client = item as Client;
                                string infoSourceName = client?.InformationSource?.InformationSourceName?.ToLower() ?? "";
                                return client?.LastName?.ToLower().Contains(searchText) == true ||
                                       client?.FirstName?.ToLower().Contains(searchText) == true ||
                                       client?.MiddleName?.ToLower().Contains(searchText) == true ||
                                       client?.PhoneNumber?.ToLower().Contains(searchText) == true ||
                                       client?.Email?.ToLower().Contains(searchText) == true ||
                                       infoSourceName.Contains(searchText);

                            case "Скидки":
                                var discount = item as Discount;
                                return discount?.DiscountName?.ToLower().Contains(searchText) == true ||
                                       discount?.DiscountValue.ToString().Contains(searchText) == true;

                            case "Оплаты":
                                // Search by amount, payment method, comment, client name (via contract)
                                var payment = item as Payment;
                                // *** Check navigation property name: PaymentMethod? ***
                                string paymentMethod = payment?.PaymentMethod?.PaymentMethodName?.ToLower() ?? ""; // Assuming 'PaymentMethod'
                                string clientLastName = payment?.Contract?.Client?.LastName?.ToLower() ?? "";
                                string clientFirstName = payment?.Contract?.Client?.FirstName?.ToLower() ?? "";
                                return payment?.Quantity.ToString().Contains(searchText) == true || // Simple amount search
                                       paymentMethod.Contains(searchText) ||
                                       payment?.Note?.ToLower().Contains(searchText) == true ||
                                       clientLastName.Contains(searchText) ||
                                       clientFirstName.Contains(searchText) ||
                                       payment?.ContractID.ToString().Contains(searchText) == true;


                            case "Договоры":
                                // Search by client/student name, status, form
                                var contract = item as Contract;
                                string clientL = contract?.Client?.LastName?.ToLower() ?? "";
                                string clientF = contract?.Client?.FirstName?.ToLower() ?? "";
                                string studentL = contract?.Student?.Client?.LastName?.ToLower() ?? "";
                                string studentF = contract?.Student?.Client?.FirstName?.ToLower() ?? "";
                                // *** Check navigation property names: StatusContract, FormOfStudy? ***
                                string status = contract?.StatusContract?.StatusContractName?.ToLower() ?? ""; // Assuming 'StatusContract'
                                string form = contract?.FormOfStudy?.FormOfStudyName?.ToLower() ?? ""; // Assuming 'FormOfStudy'
                                string courseNameC = contract?.Price?.Course?.CourseName?.ToLower() ?? "";
                                return clientL.Contains(searchText) || clientF.Contains(searchText) ||
                                       studentL.Contains(searchText) || studentF.Contains(searchText) ||
                                       status.Contains(searchText) || form.Contains(searchText) ||
                                       courseNameC.Contains(searchText) ||
                                       contract?.ContractID.ToString() == searchText; // Search by ID

                            case "Цены":
                                var price = item as Price;
                                string courseNameP = price?.Course?.CourseName?.ToLower() ?? "";
                                return courseNameP.Contains(searchText) ||
                                       price?.PricePerLesson.ToString().Contains(searchText) == true ||
                                       price?.PriceQuantity.ToString().Contains(searchText) == true;

                            case "Типы занятий": // TypeCourse
                                var typeCourse = item as TypeCourse;
                                return typeCourse?.TypeCourseName?.ToLower().Contains(searchText) == true;

                            case "Курсы":
                                var course = item as Course;
                                string typeNameC = course?.TypeCourse?.TypeCourseName?.ToLower() ?? "";
                                return course?.CourseName?.ToLower().Contains(searchText) == true ||
                                       typeNameC.Contains(searchText) ||
                                       course?.QuantityHours.ToString().Contains(searchText) == true ||
                                       course?.Description?.ToLower().Contains(searchText) == true;

                            case "Кабинеты":
                                var cabinet = item as Cabinet;
                                return cabinet?.CabinetName?.ToLower().Contains(searchText) == true ||
                                       cabinet?.Interchangeability.ToString().Contains(searchText) == true;

                            case "Студенты":
                                var student = item as Student;
                                string cliL = student?.Client?.LastName?.ToLower() ?? "";
                                string cliF = student?.Client?.FirstName?.ToLower() ?? "";
                                string cliM = student?.Client?.MiddleName?.ToLower() ?? "";
                                string cliP = student?.Client?.PhoneNumber?.ToLower() ?? "";
                                string cliE = student?.Client?.Email?.ToLower() ?? "";
                                return cliL.Contains(searchText) || cliF.Contains(searchText) || cliM.Contains(searchText) ||
                                       cliP.Contains(searchText) || cliE.Contains(searchText);

                            // --- Добавьте фильтрацию для других таблиц ---
                            // case "ТаблицаX":
                            // var x = item as ТаблицаX;
                            // return x?.SomeProperty?.ToLower().Contains(searchText) == true;

                            default:
                                return true; // Show all if no filter defined
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}", "Ошибка фильтрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Optionally clear the filter on error
                var view = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);
                if (view != null) view.Filter = null;
            }
        }

        // Enable/disable Edit/Delete buttons based on selection
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isSelected = DataGrid.SelectedItem != null;
            // Check if buttons exist before accessing IsEnabled (can be null during initialization)
            if (EditButton != null) EditButton.IsEnabled = isSelected;
            if (DeleteButton != null) DeleteButton.IsEnabled = isSelected;
        }

        // Dispose context when page is unloaded
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            context?.Dispose();
            // Debug.WriteLine("Handbook context disposed."); // For debugging disposal
        }

    } // End class Handbook
}