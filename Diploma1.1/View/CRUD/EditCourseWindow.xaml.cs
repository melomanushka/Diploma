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
    /// Логика взаимодействия для EditCourseWindow.xaml
    /// </summary>
    public partial class EditCourseWindow : Window
    {
        public Course CurrentCourse { get; set; }
        private bool IsAdding = false;

        public EditCourseWindow(Course course = null)
        {
            InitializeComponent();
            DataContext = this;

            if (course == null)
            {
                CurrentCourse = new Course();
                IsAdding = true;
                Title = "Добавить Курс";
            }
            else
            {
                CurrentCourse = course;
                IsAdding = false;
                Title = "Редактировать Курс";
            }
            LoadTypes();
        }

        private void LoadTypes()
        {
            try
            {
                using (var context = new Dimploma1Entities())
                {
                    TypeComboBox.ItemsSource = context.TypeCourse.OrderBy(t => t.TypeCourseName).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentCourse.CourseName))
            {
                MessageBox.Show("Название курса обязательно.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentCourse.TypeCourseID == null || CurrentCourse.TypeCourseID <= 0)
            {
                MessageBox.Show("Выберите тип курса.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (CurrentCourse.QuantityHours == null || CurrentCourse.QuantityHours <= 0)
            {
                MessageBox.Show("Количество часов должно быть положительным.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            // Add more validation if needed

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.Course.Add(CurrentCourse);
                    }
                    else
                    {
                        context.Course.Attach(CurrentCourse);
                        context.Entry(CurrentCourse).State = EntityState.Modified;
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
