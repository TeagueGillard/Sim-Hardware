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
using Microsoft.Win32;

namespace Sim_Wheel_Config
{
    public partial class NewDevice : Window
    {

        private SerialPort _serialPort;

        public NewDevice()
        {
            InitializeComponent();
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

        private void RGBAddRGBStripDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string RGBStripLedCount = RGBStripLEDCountTextBox.Text;
            if (int.TryParse(RGBStripLedCount, out _))
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
                    new XElement("LEDCount", RGBStripLedCount),
                    new XElement("DeviceVID", RGBStripVIDTextBox.Text),
                    new XElement("DevicePID", RGBStripPIDTextBox.Text)
                );
                doc.Root.Add(newDevice);
                doc.Save(filePath);

                System.Windows.MessageBox.Show($"RGB Strip {RGBStripDeviceNameTextBox.Text} Added Successfully!");
            }
            else
            {
                System.Windows.MessageBox.Show("Error with LED count, VID or PID!");
            }
        }

        private void AddWheelDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string WheelLedCount = WheelLEDCountTextBox.Text;
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
                    new XElement("DevicePID", WheelPIDTextBox.Text)
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
