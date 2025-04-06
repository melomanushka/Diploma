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

namespace Diploma1._1.View.CRUD
{
    /// <summary>
    /// Логика взаимодействия для EditGradeWindow.xaml
    /// </summary>
    public partial class EditGradeWindow : Window
    {
        private GradeRecord item;

        public EditGradeWindow()
        {
            InitializeComponent();
        }

        public EditGradeWindow(GradeRecord item)
        {
            this.item = item;
        }
    }
}
