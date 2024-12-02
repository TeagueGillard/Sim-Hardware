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

namespace Sim_Wheel_Config
{
    public partial class NewDevice : Window
    {

        private SerialPort _serialPort;

        public NewDevice()
        {
            InitializeComponent();
            PopulateAndOpenComPort();
        }


        // Checks if the text in "RGBLEDCountTextBox" is a number and sorts it out if not
        private void RGBLEDCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }
        private bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }


        // Populate the COM port list and open selected port
        private void PopulateAndOpenComPort()
        {
            string[] comPorts = SerialPort.GetPortNames();

            if (comPorts.Any())
            {
                string selectedPort = comPorts[0];
                comPortComboBox.ItemsSource = comPorts; 
                comPortComboBox.SelectedIndex = 0;
                _serialPort = new SerialPort(selectedPort, 115200);
            }
        }


        // Change selected Com port
        private void ComPortComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comPortComboBox.SelectedItem != null)
            {
                string selectedPort = comPortComboBox.SelectedItem.ToString();
            }
        }

        private void RGBAddRGBStripDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string number = RGBLEDCountTextBox.Text;
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
                    new XElement("DeviceName", RGBDeviceNameTextBox.Text),
                    new XElement("LEDCount", number),
                    new XElement("DeviceComPort", _serialPort.PortName)
                );
                doc.Root.Add(newDevice);
                doc.Save(filePath);

                System.Windows.MessageBox.Show($"RGB Strip {RGBDeviceNameTextBox.Text} Added Successfully!");
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
