using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
using Path = System.IO.Path;

namespace Sim_Wheel_Config
{

    public partial class MainWindow : Window
    {
        // Disable the Maximise button because it looks bad full screen
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private IntPtr _windowHandle;
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;
            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX);
        }

        private DispatcherTimer _timer;
        private FileSystemWatcher _fileWatcher;
        private int totalDevices = 0;
        private int deviceNumber = 0;

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
            this.SourceInitialized += MainWindow_SourceInitialized;
        }

        private void InitializeFileWatcher()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = Path.Combine(documentsPath, "Sim Hardware");
            string filePath = Path.Combine(folderPath, "devices.txt");

            // Create a new FileSystemWatcher
            _fileWatcher = new FileSystemWatcher
            {
                Path = folderPath,
                Filter = "devices.txt",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
            };

            // Subscribe to the Changed event to reload devices when the file is modified
            _fileWatcher.Changed += (sender, e) => UpdateDevices();
            _fileWatcher.Created += (sender, e) => UpdateDevices();
            _fileWatcher.Deleted += (sender, e) => UpdateDevices();

            // Start watching
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDevicesConnected();
        }

        private void UpdateDevicesConnected()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
            string filePath = System.IO.Path.Combine(folderPath, "devices.txt");

            int totalDevices = 0;

            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("<device"))
                    {
                        totalDevices++;
                    }

                }
            }

            DevicesLabel.Content = $"Connected Devices: {totalDevices}";

        }

        private void UpdateDevices()
        {

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = System.IO.Path.Combine(documentsPath, "Sim Hardware");
            string filePath = System.IO.Path.Combine(folderPath, "devices.txt");

            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);
                int verticalPosition = 80;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.StartsWith("<device"))
                    {
                        totalDevices++;
                    }

                    if (totalDevices > deviceNumber)
                    {
                        if (line.StartsWith("<device"))
                        {
                            if (i + 1 < lines.Length)
                            {
                                string deviceType = lines[i + 1];
                                deviceType = deviceType.Substring(12);

                                if (deviceType == "RGBStrip")
                                {
                                    string deviceName = lines[i + 2].Substring(12);
                                    string ledcount = lines[i + 3].Substring(10);
                                    string devicecomport = lines[i + 4].Substring(15);

                                    MainGrid.Children.Add(new Label() { Content = "RGB Strip Device", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(50, verticalPosition, 0, 120), FontSize = 16, Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),});
                                    verticalPosition += 20;
                                    MainGrid.Children.Add(new Label() { Content = deviceName , HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(100, verticalPosition, 0, 80), FontSize = 16, Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), });
                                    verticalPosition += 20;
                                    MainGrid.Children.Add(new Label() { Content = ledcount , HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(100, verticalPosition, 0, 40), FontSize = 16, Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), });
                                    verticalPosition += 20;
                                    MainGrid.Children.Add(new Label() { Content = devicecomport , HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(100, verticalPosition, 0, 0), FontSize = 16, Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), });
                                    verticalPosition += 20;
                                }
                                deviceNumber++;
                                
                            }
                        }
                    }
                }
            }
        }
        // Loads the new device window when the "Add New Device" button is pressed
        private void AddNewDevice_Click(object sender, RoutedEventArgs e)
        {
            NewDevice NewDeviceWindow = new NewDevice();
            NewDeviceWindow.Show();
        }




    }
}