using Diploma1._1.View.Pages;
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
using System.Windows.Shapes;

namespace Diploma1._1.View
{
    /// <summary>
    /// Логика взаимодействия для Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            this.Loaded += Home_Loaded;
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем страницу по умолчанию
            PagesNavigation.Content = new Desktop();
            // Устанавливаем выбранный пункт меню
            rdSound.IsChecked = true;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                switch (radioButton.Content.ToString())
                {
                    case " Рабочий стол":
                        PagesNavigation.Content = new Desktop();
                        break;
                    case " Расписание":
                        PagesNavigation.Content = new SchedulePage();
                        break;
                    case " Планировщик задач":
                        PagesNavigation.Content = new Planner();
                        break;
                    case " Посещение":
                        PagesNavigation.Content = new Visit();
                        break;
                    case " Журнал успеваемости":
                        PagesNavigation.Content = new Gradebook();
                        break;
                    case " Справочник":
                        PagesNavigation.Content = new Handbook();
                        break;
                    case " Отчёты":
                        PagesNavigation.Content = new Reports();
                        break;
                }
            }
        }
        private void rdClose_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }   
        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                SwitchWindowState();
                return;
            }
            if (Window.GetWindow(this).WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed) Window.GetWindow(this).DragMove();
            }
        }
        private void MaximizeWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Maximized;
        }

        private void RestoreWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Normal;
        }

        private void SwitchWindowState()
        {
            if (Window.GetWindow(this).WindowState == WindowState.Normal) MaximizeWindow();
            else RestoreWindow();
        }
    }
}
