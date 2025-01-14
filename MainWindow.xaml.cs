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
        private string MainWindowDisplayDeviceInputDevice = "0";
        private string MainWindowDisplayCurrentDeviceInputDevice = "No Device";
        private FileSystemWatcher fileWatcher;
        private ColorPicker colorPicker;
        private DirectInput directInput;
        private Joystick joystick;
        private JoystickState joystickState;

        private void InitializeFileWatcher()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
            string filePath = System.IO.Path.Combine(folderPath, "devices.xml");    // Sets file path to the devices.xml

            fileWatcher = new FileSystemWatcher(folderPath, "devices.xml")  // Creatges a filewatcher to check when devices.xml is updated
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            fileWatcher.Changed += (sender, e) =>   // Runs UpadateDevices() when devices.xml is changed
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
            VersionNoLabel.Content = "v1.0-release";                                        // ---------- UPDATE VERSION NUMBER HERE ----------
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

            ScrollViewer scrollViewer = MainGrid.Children.OfType<ScrollViewer>().FirstOrDefault();
            StackPanel stackPanel = scrollViewer.Content as StackPanel;

            List<UIElement> elementsToRemove = stackPanel.Children
                .OfType<UIElement>()
                .Where(child =>
                    (child is Grid grid && grid.Tag?.ToString() == "DeviceItem"))
                .ToList();

            foreach (UIElement element in elementsToRemove)
            {
                stackPanel.Children.Remove(element);    // Removes current devices from the stackpanel so they can be redrawn without duplicating
            }


            if (System.IO.File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);
                int verticalPosition = 70;

                foreach (XElement device in doc.Descendants("Device"))  // Retrieves device info for each device from devices.xml
                {
                    string deviceType = device.Element("DeviceType")?.Value;
                    string deviceID = device.Attribute("id")?.Value;
                    string deviceName = device.Element("DeviceName")?.Value;
                    string ledCount = device.Element("LEDCount")?.Value;
                    string devicePID = device.Element("DevicePID")?.Value;
                    string deviceVID = device.Element("DeviceVID")?.Value;
                    string deviceComPort = GetComPortFromVIDPID(deviceVID, devicePID);
                    string deviceInputDevice = device.Element("InputDevice")?.Value;
                    string LED1 = device.Element("LED1")?.Value;
                    string LED2 = device.Element("LED2")?.Value;
                    string LED3 = device.Element("LED3")?.Value;
                    string LED4 = device.Element("LED4")?.Value;


                    Grid DeviceItemGrid = new Grid()
                    {
                        Tag = "DeviceItem",
                        Margin = new Thickness(20, 10, 10, 0),
                        Width = 200,
                        Height = 95
                    };
                    LinearGradientBrush gradientBrush = new LinearGradientBrush();  // Fancy gradient for the device buttons
                    gradientBrush.StartPoint = new Point(0, 0);
                    gradientBrush.EndPoint = new Point(1, 0);
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 0));
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(255, 60, 60, 60), 1));
                    DeviceItemGrid.Background = gradientBrush;


                    // RGB Strip Button Setup
                    if (deviceType == "RGBStrip")
                    {
                        Label deviceNameLabel = new Label()
                        {
                            Content = deviceName,
                            Margin = new Thickness(20, 0, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(deviceNameLabel);

                        Label deviceTypeLabel = new Label()
                        {
                            Content = "RGB Strip Device",
                            Margin = new Thickness(30, 20, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(deviceTypeLabel);

                        Label ledCountLabel = new Label()
                        {
                            Content = $"{ledCount} Leds",
                            Margin = new Thickness(30, 40, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(ledCountLabel);

                        Label comPortLabel = new Label()
                        {
                            Content = deviceComPort,
                            Margin = new Thickness(30, 60, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(comPortLabel);

                        Image image = new Image()
                        {
                            Height = 120,
                            Width = 120,
                            Margin = new Thickness(100, 10, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/RGB-STRIP.png"))
                        };
                        DeviceItemGrid.Children.Add(image);
                        
                        Button button = new Button()
                        {
                            Content = "",
                            Margin = new Thickness(0),
                            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Width = 200,
                            Height = 95,
                            Tag = deviceName
                        };
                        button.Template = (ControlTemplate)this.FindResource("NoMouseOverButtonTemplate");
                        button.Click += (sender, e) =>
                        {
                            DisconnectComPort();
                            MainWindowDisplayDeviceType = deviceType;
                            MainWindowDisplayDeviceID = deviceID;
                            MainWindowDisplayDeviceName = deviceName;
                            MainWindowDisplayDeviceLEDCount = ledCount;
                            MainWindowDisplayDeviceComPort = deviceComPort;
                            MainFrame.Navigate(new RGBStripPage(deviceType, deviceID, deviceName, ledCount, deviceComPort));
                        };

                        DeviceItemGrid.Children.Add(button);
                        stackPanel.Children.Add(DeviceItemGrid);
                        scrollViewer.Content = stackPanel;

                    }

                    // Wheel Button Setup
                    if (deviceType == "Wheel")
                    {
                        Label deviceNameLabel = new Label()
                        {
                            Content = deviceName,
                            Margin = new Thickness(20, 0, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(deviceNameLabel);

                        Label deviceTypeLabel = new Label()
                        {
                            Content = "Wheel",
                            Margin = new Thickness(30, 20, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(deviceTypeLabel);

                        Label ledCountLabel = new Label()
                        {
                            Content = $"{ledCount} Leds",
                            Margin = new Thickness(30, 40, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(ledCountLabel);

                        Label comPortLabel = new Label()
                        {
                            Content = deviceComPort,
                            Margin = new Thickness(30, 60, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(comPortLabel);

                        Image image = new Image()
                        {
                            Height = 100,
                            Width = 100,
                            Margin = new Thickness(105, 30, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Alpine_Wheel.png"))
                        };
                        DeviceItemGrid.Children.Add(image);

                        Button button = new Button()
                        {
                            Content = "",
                            Margin = new Thickness(0),
                            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Width = 200,
                            Height = 95,
                            Tag = deviceName
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

                        DeviceItemGrid.Children.Add(button);
                        stackPanel.Children.Add(DeviceItemGrid);
                        scrollViewer.Content = stackPanel;

                    }

                    // TG-U1 Button Setup
                    if (deviceType == "TGU1")
                    {
                        Label deviceNameLabel = new Label()
                        {
                            Content = deviceName,
                            Margin = new Thickness(20, 0, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(deviceNameLabel);

                        Label deviceTypeLabel = new Label()
                        {
                            Content = "TG - U1",
                            Margin = new Thickness(30, 20, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(deviceTypeLabel);

                        Label ledCountLabel = new Label()
                        {
                            Content = $"{ledCount} Leds",
                            Margin = new Thickness(30, 40, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(ledCountLabel);

                        Label comPortLabel = new Label()
                        {
                            Content = deviceComPort,
                            Margin = new Thickness(30, 60, 0, 0),
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                        };
                        DeviceItemGrid.Children.Add(comPortLabel);

                        Image image = new Image()
                        {
                            Height = 75,
                            Width = 75,
                            Margin = new Thickness(125, 10, 0, 0),
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/TG-U1.png"))
                        };
                        DeviceItemGrid.Children.Add(image);

                        Button button = new Button()
                        {
                            Content = "",
                            Margin = new Thickness(0),
                            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                            Width = 200,
                            Height = 95,
                            Tag = deviceName
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
                            MainWindowDisplayDeviceInputDevice = deviceInputDevice;
                            MainFrame.Navigate(new TGU1Page(deviceType, deviceID, deviceName, ledCount, deviceComPort, deviceInputDevice, LED1, LED2, LED3, LED4));
                        };

                        DeviceItemGrid.Children.Add(button);
                        stackPanel.Children.Add(DeviceItemGrid);
                        scrollViewer.Content = stackPanel;

                    }



                }
            }
        }

        private void AddNewDevice_Click(object sender, RoutedEventArgs e)   // Opens the Add New Device Page when the Add New Device button is clicked
        {
            MainFrame.Navigate(new AddNewDevice.NewDevicePage());
        }

        public static string GetComPortFromVIDPID(string vid, string pid)   // Gets the COM port from the VID and PID of the device
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


        protected override void OnClosed(EventArgs e)   // Closes the serial port and joystick if needed when the program is closed
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