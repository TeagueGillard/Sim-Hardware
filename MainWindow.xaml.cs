using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Path = System.IO.Path;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace Sim_Wheel_Config
{

    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;
        private DispatcherTimer _timer;
        private int totalDevices = 0;
        private int deviceNumber = 0;
        private string MainWindowDisplayDeviceType = "0";
        private string MainWindowDisplayCurrentDeviceType = "No Device";
        private string MainWindowDisplayDeviceName = "0";
        private string MainWindowDisplayCurrentDeviceName = "No Device";
        private string MainWindowDisplayDeviceLEDCount = "0";
        private string MainWindowDisplayCurrentDeviceLEDCount = "No Device";
        private string MainWindowDisplayDeviceComPort = "0";
        private string MainWindowDisplayCurrentDeviceComPort = "No Device";
        private FileSystemWatcher fileWatcher;
        private ColorPicker colorPicker;

        private void InitializeFileWatcher()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
            string filePath = System.IO.Path.Combine(folderPath, "devices.xml");

            fileWatcher = new FileSystemWatcher(folderPath, "devices.xml")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            fileWatcher.Changed += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (e.FullPath == filePath)
                    {
                        UpdateDevices();
                    }
                });
            };
        }

        public MainWindow()
        {

            InitializeComponent();
            InitializeFileWatcher();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateDevicesConnected();
            UpdateDevices();
            VersionNoLabel.Content = "V0.0.1a";
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDevicesConnected();
        }

        private void UpdateDevicesConnected()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
            string filePath = System.IO.Path.Combine(folderPath, "devices.xml");

            int totalDevices = 0;

            if (System.IO.File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);

                totalDevices = doc.Descendants("Device").Count();
            }

            DevicesLabel.Content = $"Connected Devices: {totalDevices}";

        }

        private void UpdateDevices()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
            string filePath = System.IO.Path.Combine(folderPath, "devices.xml");

            List<UIElement> elementsToRemove = MainGrid.Children
                .OfType<UIElement>()
                .Where(child =>
                    (child is Label label && label.Tag?.ToString() == "DeviceLabel") ||
                    (child is Button button && button.Tag != null))
                .ToList();

            foreach (UIElement element in elementsToRemove)
            {
                MainGrid.Children.Remove(element);
            }

            if (System.IO.File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);
                int verticalPosition = 70;

                foreach (XElement device in doc.Descendants("Device"))
                {
                    string deviceType = device.Element("DeviceType")?.Value;
                    string deviceName = device.Element("DeviceName")?.Value;
                    string ledCount = device.Element("LEDCount")?.Value;
                    string deviceComPort = device.Element("DeviceComPort")?.Value;

                    if (deviceType == "RGBStrip")
                    {
                        MainGrid.Children.Add(new Label()
                        {
                            Tag = "DeviceLabel",
                            Content = deviceName,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(40, verticalPosition, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        });
                        verticalPosition += 20;

                        MainGrid.Children.Add(new Label()
                        {
                            Tag = "DeviceLabel",
                            Content = "RGB Strip Device",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(50, verticalPosition, 0, 120),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        });
                        verticalPosition += 20;

                        MainGrid.Children.Add(new Label()
                        {
                            Tag = "DeviceLabel",
                            Content = $"{ledCount} Leds",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(50, verticalPosition, 0, 40),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        });
                        verticalPosition += 20;

                        MainGrid.Children.Add(new Label()
                        {
                            Tag = "DeviceLabel",
                            Content = deviceComPort,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(50, verticalPosition, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                        });
                        verticalPosition += 20;

                        Button button = new Button()
                        {
                            Content = "",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(30, verticalPosition - 80, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Width = 200,
                            Height = 95,
                            Tag = deviceName,
                        };
                        button.Template = (ControlTemplate)this.FindResource("NoMouseOverButtonTemplate");
                        button.Click += (sender, e) =>
                        {
                            MainWindowDisplayDeviceType = deviceType;
                            MainWindowDisplayDeviceName = deviceName;
                            MainWindowDisplayDeviceLEDCount = ledCount;
                            MainWindowDisplayDeviceComPort = deviceComPort;
                            MainWindowDisplay();
                        };
                        MainGrid.Children.Add(button);

                        verticalPosition += 20;
                    }
                }
            }
        }

        private void MainWindowDisplay()
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
                "MainWindowDisplayDeviceNameButton",
                "Connect",
                new Thickness(852, 75, 0, 0),
                100,
                20,
                MainWindowDisplayDeviceComPort,
                MainWindowDisplayDeviceLEDCount
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
        }

        private void UpdateOrCreateLabel(string labelName, string content, ref string currentContent, Thickness margin, double fontSize)
        {
            if (content != currentContent)
            {

                Label foundLabel = (Label)MainGrid.FindName(labelName);

                if (foundLabel != null)
                {
                    MainGrid.Children.Remove(foundLabel);
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
                MainGrid.Children.Add(newLabel);
                currentContent = content;
            }
        }
        private void UpdateOrCreateConnectButton(string buttonName, string content, Thickness margin, double width, double height, string deviceComPort, string ledCount)
        {

            Button foundButton = (Button)MainGrid.FindName(buttonName);

            if (foundButton != null)
            {
                MainGrid.Children.Remove(foundButton);
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
            MainGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateRainbowWaveButton(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)MainGrid.FindName(buttonName);

            if (foundButton != null)
            {
                MainGrid.Children.Remove(foundButton);
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
            newButton.Click += RainbowWaveButton_Click;
            RegisterName(newButton.Name, newButton);
            MainGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateSendColorButton(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)MainGrid.FindName(buttonName);

            if (foundButton != null)
            {
                MainGrid.Children.Remove(foundButton);
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
            newButton.Click += SendColorButton_Click;
            RegisterName(newButton.Name, newButton);
            MainGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateBorder(string borderName, Thickness margin, double width, double height, Brush borderBrush, double borderThickness)
        {

            Border foundBorder = (Border)MainGrid.FindName(borderName);

            if (foundBorder != null)
            {
                MainGrid.Children.Remove(foundBorder);
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
            MainGrid.Children.Add(newBorder);
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
                MessageBox.Show($"Connected to Com Port: {deviceComPort}\nLED Count: {ledCount}");
                AddColorPicker();
                UpdateOrCreateRainbowWaveButton(
                "MainWindowDisplayRainbowWaveButton",
                "Rainbow Wave",
                new Thickness(700, 140, 0, 0),
                200,
                20,
                "RainbowWave"
                );
                UpdateOrCreateSendColorButton(
                "MainWindowDisplaySendColorButton",
                "Send Color",
                new Thickness(500, 140, 0, 0),
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
        private void RainbowWaveButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            var buttonData = (dynamic)clickedButton.Tag;
            string command = buttonData.command;
            // Open the selected COM port
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.WriteLine("rainbow_wave"); // Send the command to Arduino to start rainbow wave
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
        private void SendColorButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (colorPicker != null)
            {
                
                if (colorPicker.SelectedColor.HasValue)
                {
                    var selectedColor = colorPicker.SelectedColor.Value;
                    byte red = selectedColor.R;
                    byte green = selectedColor.G;
                    byte blue = selectedColor.B;

                    string colorString = $"red: {red}, green: {green}, blue: {blue},";

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
            NewDevice NewDeviceWindow = new NewDevice();
            NewDeviceWindow.Show();
        }
        private void AddColorPicker()
        {
            // Create and initialize the ColorPicker
            colorPicker = new ColorPicker()
            {
                Name = "colorPicker",
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

            // Add the ColorPicker to the MainGrid
            MainGrid.Children.Add(colorPicker);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            base.OnClosed(e);
        }

    }
}