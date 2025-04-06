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
    /// Логика взаимодействия для EditTypeCourseWindow.xaml
    /// </summary>
    public partial class EditTypeCourseWindow : Window
    {
        public TypeCourse CurrentTypeCourse { get; set; }
        private bool IsAdding = false;

        public EditTypeCourseWindow(TypeCourse typeCourse = null)
        {
            InitializeComponent();
            DataContext = this;

            if (typeCourse == null)
            {
                CurrentTypeCourse = new TypeCourse();
                IsAdding = true;
                Title = "Добавить Тип Курса";
            }
            else
            {
                CurrentTypeCourse = typeCourse;
                IsAdding = false;
                Title = "Редактировать Тип Курса";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentTypeCourse.TypeCourseName))
            {
                MessageBox.Show("Название типа не может быть пустым.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        context.TypeCourse.Add(CurrentTypeCourse);
                    }
                    else
                    {
                        context.TypeCourse.Attach(CurrentTypeCourse);
                        context.Entry(CurrentTypeCourse).State = EntityState.Modified;
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
