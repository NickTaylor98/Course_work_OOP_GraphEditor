using System;
using System.Windows;

namespace kursach.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ColorBalanceControl.xaml
    /// </summary>
    public partial class SolarizationWindow : Window
    {
        private NewWindow ControlledWindow { get; set; }

        public SolarizationWindow(NewWindow controlledWindow)
        {
            ControlledWindow = controlledWindow;
            InitializeComponent();
            ControlledWindow.PrepareCanvasForFiltering();
        }

        private void RedSlider_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ControlledWindow.ChangeSolarization((int)RedSlider.Value, (int)GreenSlider.Value, (int)BlueSlider.Value);
        }

        private void GreenSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ControlledWindow.ChangeSolarization((int)RedSlider.Value, (int)GreenSlider.Value, (int)BlueSlider.Value);
        }

        private void BlueSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ControlledWindow.ChangeSolarization((int)RedSlider.Value, (int)GreenSlider.Value, (int)BlueSlider.Value);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            ControlledWindow.UpdateCanvasAfterFiltering();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RedSlider.Value = 0;
            GreenSlider.Value = 0;
            BlueSlider.Value = 0;
            this.Close();
        }
    }
}
