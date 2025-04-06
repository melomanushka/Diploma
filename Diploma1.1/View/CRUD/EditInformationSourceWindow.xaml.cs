using System;
using System.Data.Entity;
using System.Windows;

namespace Diploma1._1.View.CRUD
{
    public partial class EditInformationSourceWindow : Window
    {
        private readonly InformationSource _source;
        private readonly Dimploma1Entities _context;
        private readonly bool _isNew;

        public EditInformationSourceWindow(InformationSource source = null)
        {
            InitializeComponent();
            _context = new Dimploma1Entities();
            
            if (source == null)
            {
                _source = new InformationSource();
                _isNew = true;
            }
            else
            {
                _source = source;
                _isNew = false;
            }

            DataContext = _source;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_source.SourceName))
            {
                MessageBox.Show("Введите название источника");
                return;
            }

            try
            {
                if (_isNew)
                {
                    _context.InformationSource.Add(_source);
                }
                else
                {
                    _context.Entry(_source).State = EntityState.Modified;
                }

                _context.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
} 