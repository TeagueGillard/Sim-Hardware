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
using Xceed.Wpf.Toolkit;
using System.IO.Ports;
using System.IO;
using System.Xml.Linq;
using SharpDX.DirectInput;
using MessageBox = System.Windows.MessageBox;

namespace Sim_Wheel_Config
{
    public partial class NewDevice : Window
    {

        private SerialPort _serialPort;
        private DirectInput directInput;
        private Joystick joystick;
        private JoystickState joystickState;
        public NewDevice()
        {
            InitializeComponent();
            RGBStripPopulateAndOpenComPort();
            WheelPopulateAndOpenComPort();
            WheelPopulateAndSetInputDevice();
        }


        // Checks if the text in LEDCountTextBox is a number and sorts it out if not
        private void RGBLEDCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RGBLEDIsTextNumeric(e.Text);
        }
        private bool RGBLEDIsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }
        private void WheelLEDCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !WheelIsTextNumeric(e.Text);
        }
        private bool WheelIsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }


        // Populate the COM port list and open selected port
        private void RGBStripPopulateAndOpenComPort()
        {
            string[] comPorts = SerialPort.GetPortNames();

            if (comPorts.Any())
            {
                string selectedPort = comPorts[0];
                RGBStripcomPortComboBox.ItemsSource = comPorts;
                RGBStripcomPortComboBox.SelectedIndex = 0;
                _serialPort = new SerialPort(selectedPort, 115200);
            }
        }
        private void WheelPopulateAndOpenComPort()
        {
            string[] comPorts = SerialPort.GetPortNames();

            if (comPorts.Any())
            {
                string selectedPort = comPorts[0];
                WheelcomPortComboBox.ItemsSource = comPorts;
                WheelcomPortComboBox.SelectedIndex = 0;
                _serialPort = new SerialPort(selectedPort, 115200);
            }
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

        // Change selected Com port
        private void RGBStripcomPortComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RGBStripcomPortComboBox.SelectedItem != null)
            {
                string selectedPort = RGBStripcomPortComboBox.SelectedItem.ToString();
            }
        }
        private void WheelcomPortComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WheelcomPortComboBox.SelectedItem != null)
            {
                string selectedPort = WheelcomPortComboBox.SelectedItem.ToString();
            }
        }
        private void WheelInputDeviceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WheelInputDeviceComboBox.SelectedItem != null)
            {
                string selectedInputDevice = WheelInputDeviceComboBox.SelectedItem.ToString();
            }
        }

        private void RGBAddRGBStripDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string number = RGBStripLEDCountTextBox.Text;
            if (int.TryParse(number, out _))
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
                    new XElement("DeviceType", "RGBStrip"),
                    new XElement("DeviceName", RGBStripDeviceNameTextBox.Text),
                    new XElement("LEDCount", number),
                    new XElement("DeviceComPort", _serialPort.PortName)
                );
                doc.Root.Add(newDevice);
                doc.Save(filePath);

                System.Windows.MessageBox.Show($"RGB Strip {RGBStripDeviceNameTextBox.Text} Added Successfully!");
            }
            else
            {
                System.Windows.MessageBox.Show("Error with LED count or Com Port!");
            }
        }

        private void AddWheelDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string number = WheelLEDCountTextBox.Text;
            string WheelInputDevice = WheelInputDeviceComboBox.Text;
            if (int.TryParse(number, out _))
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
                    new XElement("LEDCount", number),
                    new XElement("DeviceComPort", _serialPort.PortName),
                    new XElement("InputDevice", WheelInputDevice)
                );
                doc.Root.Add(newDevice);
                doc.Save(filePath);

                System.Windows.MessageBox.Show($"Wheel {WheelDeviceNameTextBox.Text} Added Successfully!");
            }
            else
            {
                System.Windows.MessageBox.Show("Error with LED count or Com Port!");
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
