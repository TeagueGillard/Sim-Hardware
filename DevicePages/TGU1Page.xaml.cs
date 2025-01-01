using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace Sim_Wheel_Config
{
    /// <summary>
    /// Interaction logic for DeviceDisplayPage.xaml
    /// </summary>
    public partial class TGU1Page : Page
    {
        private SerialPort _serialPort;
        private DispatcherTimer _timer;
        private int totalDevices = 0;
        private int deviceNumber = 0;
        private string MainWindowDisplayDeviceType = "0";
        private string MainWindowDisplayCurrentDeviceType = "No Device";
        private string MainWindowDisplayDeviceID = "0";
        private string MainWindowDisplayCurrentDeviceID = "No Device";
        private string MainWindowDisplayDeviceName = "0";
        private string MainWindowDisplayCurrentDeviceName = "No Device";
        private string MainWindowDisplayDeviceLEDCount = "0";
        private string MainWindowDisplayCurrentDeviceLEDCount = "No Device";
        private string MainWindowDisplayDeviceComPort = "0";
        private string MainWindowDisplayCurrentDeviceComPort = "No Device";
        private string MainWindowDisplayDeviceStatus = "Not Connected";
        private string MainWindowDisplayCurrentDeviceStatus = "Not Connected";
        private FileSystemWatcher fileWatcher;
        private ColorPicker colorPicker1;
        private ColorPicker colorPicker2;
        private ColorPicker colorPicker3;
        private ColorPicker colorPicker4;


        public TGU1Page(string deviceType, string deviceID, string deviceName, string ledCount, string deviceComPort, string LED1, string LED2, string LED3, string LED4)
        {
            MainWindowDisplayDeviceType = deviceType;
            MainWindowDisplayDeviceID = deviceID;
            MainWindowDisplayDeviceName = deviceName;
            MainWindowDisplayDeviceLEDCount = ledCount;
            MainWindowDisplayDeviceComPort = deviceComPort;
            InitializeComponent();
            DeviceDisplay();
            this.Unloaded += Page_Unloaded;
        }

        private void DeviceDisplay()
        {

            UpdateOrCreateLabel(
                "MainWindowDisplayDeviceNameLabel",
                MainWindowDisplayDeviceName,
                ref MainWindowDisplayCurrentDeviceName,
                new Thickness(280, 55, 0, 0),
                32
            );

            UpdateOrCreateBorder(
                "MainWindowDisplayDeviceNameBorder",
                new Thickness(287, 105, 0, 0),
                665,
                1,
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                2
            );

            UpdateOrCreateConnectButton(
                "MainWindowDisplayDeviceConnectButton",
                "Connect",
                new Thickness(852, 111, 0, 0),
                100,
                20,
                MainWindowDisplayDeviceComPort,
                MainWindowDisplayDeviceLEDCount
            );

            UpdateOrCreateDeleteButton(
                "MainWindowDisplayDeviceDeleteButton",
                "Delete Device",
                new Thickness(852, 511, 0, 0),
                100,
                20,
                MainWindowDisplayDeviceID,
                MainWindowDisplayDeviceName
            );

            UpdateOrCreateLabel(
                "MainWindowDisplayDeviceTypeLabel",
                $"{MainWindowDisplayDeviceType} - ",
                ref MainWindowDisplayCurrentDeviceType,
                new Thickness(283, 100, 0, 0),
                16
            );

            UpdateOrCreateLabel(
                "MainWindowDisplayDeviceLEDCountLabel",
                $"{MainWindowDisplayDeviceLEDCount} Leds -",
                ref MainWindowDisplayCurrentDeviceLEDCount,
                new Thickness(363, 100, 0, 0),
                16
            );

            UpdateOrCreateLabel(
                "MainWindowDisplayDeviceComPortLabel",
                MainWindowDisplayDeviceComPort,
                ref MainWindowDisplayCurrentDeviceComPort,
                new Thickness(443, 100, 0, 0),
                16
            );
            UpdateOrCreateStatusLabel(
                "MainWindowDisplayDeviceStatusLabel",
                $"Status: {MainWindowDisplayDeviceStatus}",
                ref MainWindowDisplayCurrentDeviceStatus,
                new Thickness(0, 75, 27, 0),
                16,
                MainWindowDisplayDeviceStatus
            );
        }

        private void UpdateOrCreateLabel(string labelName, string content, ref string currentContent, Thickness margin, double fontSize)
        {
            if (content != currentContent)
            {

                Label foundLabel = (Label)TGU1PageGrid.FindName(labelName);

                if (foundLabel != null)
                {
                    TGU1PageGrid.Children.Remove(foundLabel);
                    UnregisterName(labelName);
                }

                Label newLabel = new Label()
                {
                    Name = labelName,
                    Content = content,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = margin,
                    FontSize = fontSize,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                };

                RegisterName(newLabel.Name, newLabel);
                TGU1PageGrid.Children.Add(newLabel);
                currentContent = content;
            }
        }
        private void UpdateOrCreateStatusLabel(string labelName, string content, ref string currentContent, Thickness margin, double fontSize, string deviceStatus)
        {
            if (content != currentContent)
            {
                Label foundLabel = (Label)TGU1PageGrid.FindName(labelName);
                if (foundLabel != null)
                {
                    TGU1PageGrid.Children.Remove(foundLabel);
                    UnregisterName(labelName);
                }
                if (deviceStatus == "Not Connected")
                {
                    Label newLabel = new Label()
                    {
                        Name = labelName,
                        Content = content,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = margin,
                        FontSize = fontSize,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
                    };
                    RegisterName(newLabel.Name, newLabel);
                    TGU1PageGrid.Children.Add(newLabel);
                    currentContent = content;
                }
                if (deviceStatus == "Connected")
                {
                    Label newLabel = new Label()
                    {
                        Name = labelName,
                        Content = content,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = margin,
                        FontSize = fontSize,
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
                    };
                    RegisterName(newLabel.Name, newLabel);
                    TGU1PageGrid.Children.Add(newLabel);
                    currentContent = content;
                }
            }
        }
        private void UpdateOrCreateConnectButton(string buttonName, string content, Thickness margin, double width, double height, string deviceComPort, string ledCount)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { deviceComPort, ledCount };
            newButton.Click += ConnectButton_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateDeleteButton(string buttonName, string content, Thickness margin, double width, double height, string device, string deviceName)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { device, deviceName };
            newButton.Click += DeleteButton_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateRainbowWaveButton1(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += RainbowWaveButton1_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateRainbowWaveButton2(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += RainbowWaveButton2_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateRainbowWaveButton3(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += RainbowWaveButton3_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateRainbowWaveButton4(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += RainbowWaveButton4_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }

        private void UpdateOrCreateSimhubButton(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += SimhubButton_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateSendColorButton1(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += SendColorButton1_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateSendColorButton2(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += SendColorButton2_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateSendColorButton3(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += SendColorButton3_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateSendColorButton4(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)TGU1PageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                TGU1PageGrid.Children.Remove(foundButton);
                UnregisterName(buttonName);
            }

            Button newButton = new Button()
            {
                Name = buttonName,
                Content = content,
                Margin = margin,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            newButton.Tag = new { command };
            newButton.Click += SendColorButton4_Click;
            RegisterName(newButton.Name, newButton);
            TGU1PageGrid.Children.Add(newButton);
        }

        private void UpdateOrCreateBorder(string borderName, Thickness margin, double width, double height, Brush borderBrush, double borderThickness)
        {

            Border foundBorder = (Border)TGU1PageGrid.FindName(borderName);

            if (foundBorder != null)
            {
                TGU1PageGrid.Children.Remove(foundBorder);
                UnregisterName(borderName);
            }

            Border newBorder = new Border()
            {
                Name = borderName,
                Margin = margin,
                Width = width,
                Height = height,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(borderThickness),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            RegisterName(newBorder.Name, newBorder);
            TGU1PageGrid.Children.Add(newBorder);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string deviceComPort = buttonData.deviceComPort;
            string ledCount = buttonData.ledCount;
            // Open the selected COM port
            try
            {
                _serialPort = new SerialPort(deviceComPort, 115200); // Set the port and baud rate
                _serialPort.Open(); // Open the COM port
                MainWindowDisplayDeviceStatus = "Connected";

                UpdateOrCreateStatusLabel(
                "MainWindowDisplayDeviceStatusLabel",
                $"Status: {MainWindowDisplayDeviceStatus}",
                ref MainWindowDisplayCurrentDeviceStatus,
                new Thickness(0, 75, 27, 0),
                16,
                MainWindowDisplayDeviceStatus
                );

                AddColorPicker1();
                AddColorPicker2();
                AddColorPicker3();
                AddColorPicker4();

                UpdateOrCreateRainbowWaveButton1(
                "MainWindowDisplayRainbowWaveButton1",
                "Rainbow Wave",
                new Thickness(700, 140, 0, 0),
                200,
                20,
                "RainbowWave"
                );
                UpdateOrCreateRainbowWaveButton2(
                "MainWindowDisplayRainbowWaveButton2",
                "Rainbow Wave",
                new Thickness(700, 190, 0, 0),
                200,
                20,
                "RainbowWave"
                );
                UpdateOrCreateRainbowWaveButton3(
                "MainWindowDisplayRainbowWaveButton3",
                "Rainbow Wave",
                new Thickness(700, 240, 0, 0),
                200,
                20,
                "RainbowWave"
                );
                UpdateOrCreateRainbowWaveButton4(
                "MainWindowDisplayRainbowWaveButton4",
                "Rainbow Wave",
                new Thickness(700, 290, 0, 0),
                200,
                20,
                "RainbowWave"
                );

                UpdateOrCreateSimhubButton(
                "MainWindowDisplaySimhubButton",
                "Connect to Simhub",
                new Thickness(600, 111, 0, 0),
                200,
                20,
                "Simhub"
                );

                UpdateOrCreateSendColorButton1(
                "MainWindowDisplaySendColorButton1",
                "Send Color",
                new Thickness(500, 140, 0, 0),
                200,
                20,
                "SendColor"
                );
                UpdateOrCreateSendColorButton2(
                "MainWindowDisplaySendColorButton2",
                "Send Color",
                new Thickness(500, 190, 0, 0),
                200,
                20,
                "SendColor"
                );
                UpdateOrCreateSendColorButton3(
                "MainWindowDisplaySendColorButton3",
                "Send Color",
                new Thickness(500, 240, 0, 0),
                200,
                20,
                "SendColor"
                );
                UpdateOrCreateSendColorButton4(
                "MainWindowDisplaySendColorButton4",
                "Send Color",
                new Thickness(500, 290, 0, 0),
                200,
                20,
                "SendColor"
                );
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error opening COM port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string deviceID = buttonData.device;
            string deviceName = buttonData.deviceName;
            MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete {deviceName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
                string filePath = System.IO.Path.Combine(folderPath, "devices.xml");
                try
                {

                    XDocument doc = XDocument.Load(filePath);
                    XElement deviceToDelete = doc.Descendants("Device").FirstOrDefault(d => d.Attribute("id")?.Value == deviceID);

                    if (deviceToDelete != null)
                    {

                        deviceToDelete.Remove();
                        doc.Save(filePath);

                        MessageBox.Show($"{deviceName} was deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainWindowDisplayDeviceType = " ";
                        MainWindowDisplayCurrentDeviceType = "Device Deleted!";
                        MainWindowDisplayDeviceID = " ";
                        MainWindowDisplayCurrentDeviceID = "Device Deleted!";
                        MainWindowDisplayDeviceName = "Device was Deleted!";
                        MainWindowDisplayCurrentDeviceName = "Device Deleted!";
                        MainWindowDisplayDeviceLEDCount = " ";
                        MainWindowDisplayCurrentDeviceLEDCount = "Device Deleted!";
                        MainWindowDisplayDeviceComPort = " ";
                        MainWindowDisplayCurrentDeviceComPort = "Device Deleted!";
                    }
                    else
                    {
                        MessageBox.Show($"An error occurred while deleting the device: {deviceName}", "Cancel", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the device: {deviceName}", "Cancel", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"{deviceName} was not deleted.", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RainbowWaveButton1_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string command = buttonData.command;
            // Open the selected COM port
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("led: 1, rainbow_wave, ee"); // Send the command to Arduino to start rainbow wave
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error opening COM port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void RainbowWaveButton2_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string command = buttonData.command;
            // Open the selected COM port
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("led: 2, rainbow_wave, ee"); // Send the command to Arduino to start rainbow wave
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error opening COM port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void RainbowWaveButton3_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string command = buttonData.command;
            // Open the selected COM port
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("led: 3, rainbow_wave, ee"); // Send the command to Arduino to start rainbow wave
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error opening COM port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void RainbowWaveButton4_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string command = buttonData.command;
            // Open the selected COM port
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("led: 4, rainbow_wave, ee"); // Send the command to Arduino to start rainbow wave
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error opening COM port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }




        private void SimhubButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string command = buttonData.command;
            // Open the selected COM port
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("SIMHUB, ee"); // Send the command to Arduino to start rainbow wave
                    _serialPort.Close();    // Close serial port so SimHub can communicate with the Arduino
                    MainWindowDisplayDeviceStatus = "Not Connected";

                    UpdateOrCreateStatusLabel(
                    "MainWindowDisplayDeviceStatusLabel",
                    $"Status: {MainWindowDisplayDeviceStatus}",
                    ref MainWindowDisplayCurrentDeviceStatus,
                    new Thickness(0, 75, 27, 0),
                    16,
                    MainWindowDisplayDeviceStatus
                    );

                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Error opening COM port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void SendColorButton1_Click(object sender, RoutedEventArgs e)
        {

            if (colorPicker1 != null)
            {

                if (colorPicker1.SelectedColor.HasValue)
                {
                    var selectedColor = colorPicker1.SelectedColor.Value;
                    byte red = selectedColor.R;
                    byte green = selectedColor.G;
                    byte blue = selectedColor.B;

                    string colorString = $"led: 1, red: {red}, green: {green}, blue: {blue}, ee";

                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _serialPort.WriteLine(colorString);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                    }
                }
                else
                {

                    MessageBox.Show("Please select a color before sending.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {

                MessageBox.Show("Color Picker is not initialized. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendColorButton2_Click(object sender, RoutedEventArgs e)
        {

            if (colorPicker2 != null)
            {

                if (colorPicker2.SelectedColor.HasValue)
                {
                    var selectedColor = colorPicker2.SelectedColor.Value;
                    byte red = selectedColor.R;
                    byte green = selectedColor.G;
                    byte blue = selectedColor.B;

                    string colorString = $"led: 2, red: {red}, green: {green}, blue: {blue}, ee";

                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _serialPort.WriteLine(colorString);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                    }
                }
                else
                {

                    MessageBox.Show("Please select a color before sending.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {

                MessageBox.Show("Color Picker is not initialized. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendColorButton3_Click(object sender, RoutedEventArgs e)
        {

            if (colorPicker3 != null)
            {

                if (colorPicker3.SelectedColor.HasValue)
                {
                    var selectedColor = colorPicker3.SelectedColor.Value;
                    byte red = selectedColor.R;
                    byte green = selectedColor.G;
                    byte blue = selectedColor.B;

                    string colorString = $"led: 3, red: {red}, green: {green}, blue: {blue}, ee";

                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _serialPort.WriteLine(colorString);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                    }
                }
                else
                {

                    MessageBox.Show("Please select a color before sending.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {

                MessageBox.Show("Color Picker is not initialized. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendColorButton4_Click(object sender, RoutedEventArgs e)
        {

            if (colorPicker4 != null)
            {

                if (colorPicker4.SelectedColor.HasValue)
                {
                    var selectedColor = colorPicker4.SelectedColor.Value;
                    byte red = selectedColor.R;
                    byte green = selectedColor.G;
                    byte blue = selectedColor.B;

                    string colorString = $"led: 4, red: {red}, green: {green}, blue: {blue}, ee";

                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        _serialPort.WriteLine(colorString);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("COM port is not open.");
                    }
                }
                else
                {

                    MessageBox.Show("Please select a color before sending.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {

                MessageBox.Show("Color Picker is not initialized. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewDevice_Click(object sender, RoutedEventArgs e)
        {
            NewDevice_Legacy NewDeviceWindow = new NewDevice_Legacy();
            NewDeviceWindow.Show();
        }
        private void AddColorPicker1()
        {
            // Create and initialize the ColorPicker
            colorPicker1 = new ColorPicker()
            {
                Name = "colorPicker1",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(290, 140, 0, 0),
                Width = 200,
                Height = 40,
                Background = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                HeaderBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                TabBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                Foreground = Brushes.White,
                HeaderForeground = Brushes.White,
                TabForeground = Brushes.White,
                DropDownBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
            };

            // Add the ColorPicker to the TGU1PageGrid
            TGU1PageGrid.Children.Add(colorPicker1);
        }
        private void AddColorPicker2()
        {
            // Create and initialize the ColorPicker
            colorPicker2 = new ColorPicker()
            {
                Name = "colorPicker2",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(290, 190, 0, 0),
                Width = 200,
                Height = 40,
                Background = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                HeaderBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                TabBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                Foreground = Brushes.White,
                HeaderForeground = Brushes.White,
                TabForeground = Brushes.White,
                DropDownBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
            };

            // Add the ColorPicker to the TGU1PageGrid
            TGU1PageGrid.Children.Add(colorPicker2);
        }
        private void AddColorPicker3()
        {
            // Create and initialize the ColorPicker
            colorPicker3 = new ColorPicker()
            {
                Name = "colorPicker1",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(290, 240, 0, 0),
                Width = 200,
                Height = 40,
                Background = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                HeaderBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                TabBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                Foreground = Brushes.White,
                HeaderForeground = Brushes.White,
                TabForeground = Brushes.White,
                DropDownBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
            };

            // Add the ColorPicker to the TGU1PageGrid
            TGU1PageGrid.Children.Add(colorPicker3);
        }
        private void AddColorPicker4()
        {
            // Create and initialize the ColorPicker
            colorPicker4 = new ColorPicker()
            {
                Name = "colorPicker2",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(290, 290, 0, 0),
                Width = 200,
                Height = 40,
                Background = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                HeaderBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                TabBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
                Foreground = Brushes.White,
                HeaderForeground = Brushes.White,
                TabForeground = Brushes.White,
                DropDownBackground = new SolidColorBrush(Color.FromArgb(255, 9, 9, 16)), // #FF090910
            };

            // Add the ColorPicker to the TGU1PageGrid
            TGU1PageGrid.Children.Add(colorPicker4);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
    }
}
