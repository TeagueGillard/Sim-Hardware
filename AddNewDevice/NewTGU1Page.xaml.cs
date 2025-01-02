using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Sim_Wheel_Config.AddNewDevice
{
    public partial class NewTGU1Page : Page
    {
        public NewTGU1Page()
        {
            InitializeComponent();
        }

        private void TGU1LEDCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TGU1LEDIsTextNumeric(e.Text);
        }
        private bool TGU1LEDIsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }

        private void AddTGU1DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            string TGU1LedCount = TGU1LEDCountTextBox.Text;
            if (int.TryParse(TGU1LedCount, out _))
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
                    new XElement("DeviceType", "TGU1"),
                    new XElement("DeviceName", TGU1DeviceNameTextBox.Text),
                    new XElement("LEDCount", TGU1LedCount),
                    new XElement("DeviceVID", TGU1VIDTextBox.Text),
                    new XElement("DevicePID", TGU1PIDTextBox.Text),
                    new XElement("LED1", TGU1LED1CheckBox.IsChecked == true ? "true" : "false"),
                    new XElement("LED2", TGU1LED2CheckBox.IsChecked == true ? "true" : "false"),
                    new XElement("LED3", TGU1LED3CheckBox.IsChecked == true ? "true" : "false"),
                    new XElement("LED4", TGU1LED4CheckBox.IsChecked == true ? "true" : "false")
                );
                doc.Root.Add(newDevice);
                doc.Save(filePath);

                System.Windows.MessageBox.Show($"TG - U1 {TGU1DeviceNameTextBox.Text} Added Successfully!");
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
