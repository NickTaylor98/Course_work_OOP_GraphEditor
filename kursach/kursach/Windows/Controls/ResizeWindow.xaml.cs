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

namespace kursach.Windows.Controls
{
    /// <summary>
    /// Логика взаимодействия для ResizeWindow.xaml
    /// </summary>
    public partial class ResizeWindow : Window
    {
        NewWindow window;

        public ResizeWindow (NewWindow window)
        {
            this.window = window;
            InitializeComponent();
            window.PrepareCanvasForFiltering();
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            window.ChangeSize((int)HeightSlider.Value, (int)WidthSlider.Value);
            this.Close();
        }
    }

}
