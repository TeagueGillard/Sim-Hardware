using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

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
        private void AddTGU1DeviceButton_Click(Object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrameInstance.Navigate(new NewTGU1Page());
        }
    }
}
