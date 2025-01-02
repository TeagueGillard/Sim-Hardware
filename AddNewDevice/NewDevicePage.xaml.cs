using System.Windows;
using System.Windows.Controls;

namespace Sim_Wheel_Config.AddNewDevice
{
    /// <summary>
    /// Interaction logic for NewDevicePage.xaml
    /// </summary>
    public partial class NewDevicePage : Page
    {
        public NewDevicePage()
        {
            InitializeComponent();
        }

        private void AddRGBStripDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrameInstance.Navigate(new NewRGBStripPage());
        }
        private void AddWheelDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrameInstance.Navigate(new NewWheelPage());
        }
    }
}
