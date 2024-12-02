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
                string filePath = System.IO.Path.Combine(folderPath, "devices.txt");
                int nextDeviceNumber = GetNextDeviceNumber(filePath);
                File.AppendAllText(filePath, Environment.NewLine + "<device" + nextDeviceNumber + ">" + Environment.NewLine);
                File.AppendAllText(filePath, "devicetype= RGBStrip" + Environment.NewLine);
                File.AppendAllText(filePath, "devicename= " + RGBDeviceNameTextBox.Text + Environment.NewLine);
                File.AppendAllText(filePath, "ledcount= " + number + Environment.NewLine);
                File.AppendAllText(filePath, "devicecomport= " + _serialPort.PortName + Environment.NewLine);
                System.Windows.MessageBox.Show($"RGB Strip {RGBDeviceNameTextBox.Text} Added Successfully!");
            }
            else
            {
                System.Windows.MessageBox.Show("Error with LED count or Com Port!");
            }
        }

        private int GetNextDeviceNumber(string filePath)
        {
            int maxdevicenumber = 0;
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("<device"))
                    {
                        string devicepart = line.Substring(7, line.IndexOf(">") - 7);
                        if (int.TryParse(devicepart, out int devicenumber))
                        {
                            if (devicenumber > maxdevicenumber)
                            {
                                maxdevicenumber = devicenumber;
                            }
                        }
                    }
                }
            }
            return maxdevicenumber + 1;


        }
    }
}
