﻿using System;
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
    /// Логика взаимодействия для EditInformationSourceWindow.xaml
    /// </summary>
    public partial class EditInformationSourceWindow : Window
    {
        public InformationSource CurrentSource { get; set; }
        private bool IsAdding = false;

        public EditInformationSourceWindow(InformationSource sourceToEdit = null)
        {
            InitializeComponent();
            DataContext = this; // Set DataContext for Binding

            if (sourceToEdit == null)
            {
                // Adding new source
                CurrentSource = new InformationSource();
                IsAdding = true;
                Title = "Добавить Источник Информации";
            }
            else
            {
                // Editing existing source
                CurrentSource = sourceToEdit;
                IsAdding = false;
                Title = "Редактировать Источник Информации";
                // Consider loading a fresh copy if needed, depends on context lifetime
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Basic Validation
            if (string.IsNullOrWhiteSpace(CurrentSource.InformationSourceName))
            {
                MessageBox.Show("Название источника не может быть пустым.", "Ошибка Валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                SourceNameTextBox.Focus();
                return;
            }

            try
            {
                using (var context = new Dimploma1Entities())
                {
                    if (IsAdding)
                    {
                        // Add new entity
                        context.InformationSource.Add(CurrentSource);
                    }
                    else
                    {
                        // Attach and mark as modified
                        context.InformationSource.Attach(CurrentSource);
                        context.Entry(CurrentSource).State = EntityState.Modified;
                    }
                    context.SaveChanges();
                } // Context disposed

                DialogResult = true; // Indicate success
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
            DialogResult = false; // Indicate cancellation
            Close();
        }
    }
}
