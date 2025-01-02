using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Sim_Wheel_Config.AddNewDevice
{
    /// <summary>
    /// Interaction logic for NewRGBStripPage.xaml
    /// </summary>
    public partial class NewRGBStripPage : Page
    {
        public NewRGBStripPage()
        {
            InitializeComponent();
        }

        private void RGBLEDCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !RGBLEDIsTextNumeric(e.Text);
        }
        private bool RGBLEDIsTextNumeric(string text)
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
