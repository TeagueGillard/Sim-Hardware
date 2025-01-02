using Microsoft.Win32;
using SharpDX.DirectInput;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;

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
        private ColorPicker colorPicker;
        private DirectInput directInput;
        private Joystick joystick;
        private JoystickState joystickState;

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
        public static Frame MainFrameInstance { get; private set; }

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
            VersionNoLabel.Content = "v0.0.5-alpha";
            MainFrameInstance = MainFrame;
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

            DevicesLabel.Content = $"Devices: {totalDevices}";

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
                    (child is Button button && button.Tag != null) ||
                    (child is Image image))
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
                    string deviceID = device.Attribute("id")?.Value;
                    string deviceName = device.Element("DeviceName")?.Value;
                    string ledCount = device.Element("LEDCount")?.Value;
                    string devicePID = device.Element("DevicePID")?.Value;
                    string deviceVID = device.Element("DeviceVID")?.Value;
                    string deviceComPort = GetComPortFromVIDPID(deviceVID, devicePID);

                    // RGB Strip Page Setup
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

                        Image image = new Image()
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Height = 120,
                            Width = 120,
                            Margin = new Thickness(130, verticalPosition - 70, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/RGB-STRIP.png"))
                        };
                        MainGrid.Children.Add(image);

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
                            DisconnectComPort();
                            string deviceStatus = "Not Connected";
                            MainWindowDisplayDeviceType = deviceType;
                            MainWindowDisplayDeviceID = deviceID;
                            MainWindowDisplayDeviceName = deviceName;
                            MainWindowDisplayDeviceLEDCount = ledCount;
                            MainWindowDisplayDeviceComPort = deviceComPort;
                            MainFrame.Navigate(new RGBStripPage(deviceType, deviceID, deviceName, ledCount, deviceComPort));
                        };
                        MainGrid.Children.Add(button);

                        verticalPosition += 20;
                    }

                    // Wheel Page Setup
                    if (deviceType == "Wheel")
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
                            Content = "Wheel",
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

                        Image image = new Image()
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Height = 100,
                            Width = 100,
                            Margin = new Thickness(130, verticalPosition - 55, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Alpine_Wheel.png"))
                        };
                        MainGrid.Children.Add(image);

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
                            DisconnectComPort();
                            string deviceStatus = "Not Connected";
                            MainWindowDisplayDeviceType = deviceType;
                            MainWindowDisplayDeviceID = deviceID;
                            MainWindowDisplayDeviceName = deviceName;
                            MainWindowDisplayDeviceLEDCount = ledCount;
                            MainWindowDisplayDeviceComPort = deviceComPort;
                            MainFrame.Navigate(new WheelPage(deviceType, deviceID, deviceName, ledCount, deviceComPort));
                        };
                        MainGrid.Children.Add(button);

                        verticalPosition += 20;

                    }

                    if (deviceType == "TGU1")
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
                            Content = "TG - U1 Device",
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

                        Image image = new Image()
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Height = 120,
                            Width = 120,
                            Margin = new Thickness(130, verticalPosition - 70, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/RGB-STRIP.png"))
                        };
                        MainGrid.Children.Add(image);

                        string LED1 = device.Element("LED1")?.Value;
                        string LED2 = device.Element("LED2")?.Value;
                        string LED3 = device.Element("LED3")?.Value;
                        string LED4 = device.Element("LED4")?.Value;

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
                            DisconnectComPort();
                            string deviceStatus = "Not Connected";
                            MainWindowDisplayDeviceType = deviceType;
                            MainWindowDisplayDeviceID = deviceID;
                            MainWindowDisplayDeviceName = deviceName;
                            MainWindowDisplayDeviceLEDCount = ledCount;
                            MainWindowDisplayDeviceComPort = deviceComPort;
                            MainFrame.Navigate(new TGU1Page(deviceType, deviceID, deviceName, ledCount, deviceComPort, LED1, LED2, LED3, LED4));
                        };
                        MainGrid.Children.Add(button);
                        verticalPosition += 20;

                    }
                }
            }
        }

        private void AddNewDevice_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AddNewDevice.NewDevicePage());
        }

        public static string GetComPortFromVIDPID(string vid, string pid)
        {
            string comPort = null;
            string searchPattern = $"VID_{vid}&PID_{pid}";

            RegistryKey usbDevicesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");

            if (usbDevicesKey != null)
            {
                foreach (string subKeyName in usbDevicesKey.GetSubKeyNames())
                {
                    if (subKeyName.Contains(searchPattern))
                    {
                        RegistryKey deviceKey = usbDevicesKey.OpenSubKey(subKeyName);
                        if (deviceKey != null)
                        {
                            foreach (string deviceInstance in deviceKey.GetSubKeyNames())
                            {
                                RegistryKey instanceKey = deviceKey.OpenSubKey(deviceInstance);
                                if (instanceKey != null)
                                {
                                    RegistryKey deviceParamsKey = instanceKey.OpenSubKey("Device Parameters");
                                    if (deviceParamsKey != null)
                                    {
                                        comPort = deviceParamsKey.GetValue("PortName") as string;
                                        if (!string.IsNullOrEmpty(comPort))
                                        {
                                            return comPort;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return comPort;
        }

        private void DisconnectComPort()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            base.OnClosed(e);
        }

    }
}