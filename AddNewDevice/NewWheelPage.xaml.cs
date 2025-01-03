using SharpDX.DirectInput;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Sim_Wheel_Config.AddNewDevice
{
    /// <summary>
    /// Interaction logic for NewWheelPage.xaml
    /// </summary>
    public partial class NewWheelPage : Page
    {
        private DirectInput directInput;
        private Joystick joystick;

        public NewWheelPage()
        {
            InitializeComponent();
            WheelPopulateAndSetInputDevice();
        }

        private void WheelPopulateAndSetInputDevice()
        {
            try
            {

                directInput = new DirectInput();
                var devices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices).ToList();

                if (devices.Any())
                {

                    WheelInputDeviceComboBox.ItemsSource = devices.Select(device => device.InstanceName).ToList();
                    WheelInputDeviceComboBox.SelectedIndex = 0;
                    var selectedInputDevice = devices.First();
                    joystick = new Joystick(directInput, selectedInputDevice.InstanceGuid);
                }
                else
                {
                    MessageBox.Show("No gamepad connected!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while populating devices: {ex.Message}");
            }
        }
        private void WheelInputDeviceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WheelInputDeviceComboBox.SelectedItem != null)
            {
                string selectedInputDevice = WheelInputDeviceComboBox.SelectedItem.ToString();
            }
        }

        private void WheelLEDCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !WheelIsTextNumeric(e.Text);
        }
        private bool WheelIsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }

        private void AddWheelDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string WheelLedCount = WheelLEDCountTextBox.Text;
            string WheelInputDevice = WheelInputDeviceComboBox.Text;

            if (int.TryParse(WheelLedCount, out _))
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = System.IO.Path.Combine(folderPath, "devices.xml");

                XDocument doc;
                if (File.Exists(filePath))
                {
                    doc = XDocument.Load(filePath);
                }
                else
                {
                    doc = new XDocument(new XElement("Devices"));
                }

                int nextDeviceNumber = GetNextDeviceNumber(doc);

                XElement newDevice = new XElement("Device",
                    new XAttribute("id", nextDeviceNumber),
                    new XElement("DeviceType", "Wheel"),
                    new XElement("DeviceName", WheelDeviceNameTextBox.Text),
                    new XElement("LEDCount", WheelLedCount),
                    new XElement("DeviceVID", WheelVIDTextBox.Text),
                    new XElement("DevicePID", WheelPIDTextBox.Text),
                    new XElement("InputDevice", WheelInputDevice)
                );
                doc.Root.Add(newDevice);
                doc.Save(filePath);

                System.Windows.MessageBox.Show($"Wheel {WheelDeviceNameTextBox.Text} Added Successfully!");
            }
            else
            {
                System.Windows.MessageBox.Show("Error with LED count, VID or PID!");
            }
        }

        private int GetNextDeviceNumber(XDocument doc)
        {
            int maxdevicenumber = 0;

            foreach (XElement device in doc.Descendants("Device"))
            {
                if (device.Attribute("id") != null && int.TryParse(device.Attribute("id").Value, out int deviceId))
                {
                    maxdevicenumber = Math.Max(maxdevicenumber, deviceId);
                }
            }
            return maxdevicenumber + 1;
        }

    }
}
