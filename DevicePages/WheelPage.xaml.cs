﻿using SharpDX.DirectInput;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
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
    public partial class WheelPage : Page
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

        public WheelPage(string deviceType, string deviceID, string deviceName, string ledCount, string deviceComPort)
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

                Label foundLabel = (Label)WheelPageGrid.FindName(labelName);

                if (foundLabel != null)
                {
                    WheelPageGrid.Children.Remove(foundLabel);
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
                WheelPageGrid.Children.Add(newLabel);
                currentContent = content;
            }
        }
        private void UpdateOrCreateStatusLabel(string labelName, string content, ref string currentContent, Thickness margin, double fontSize, string deviceStatus)
        {
            if (content != currentContent)
            {
                Label foundLabel = (Label)WheelPageGrid.FindName(labelName);
                if (foundLabel != null)
                {
                    WheelPageGrid.Children.Remove(foundLabel);
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
                    WheelPageGrid.Children.Add(newLabel);
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
                    WheelPageGrid.Children.Add(newLabel);
                    currentContent = content;
                }
            }
        }
        private void UpdateOrCreateConnectButton(string buttonName, string content, Thickness margin, double width, double height, string deviceComPort, string ledCount)
        {

            Button foundButton = (Button)WheelPageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                WheelPageGrid.Children.Remove(foundButton);
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
            WheelPageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateDeleteButton(string buttonName, string content, Thickness margin, double width, double height, string device, string deviceName)
        {

            Button foundButton = (Button)WheelPageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                WheelPageGrid.Children.Remove(foundButton);
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
            WheelPageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateRainbowWaveButton(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)WheelPageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                WheelPageGrid.Children.Remove(foundButton);
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
            WheelPageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateSendColorButton(string buttonName, string content, Thickness margin, double width, double height, string command)
        {

            Button foundButton = (Button)WheelPageGrid.FindName(buttonName);

            if (foundButton != null)
            {
                WheelPageGrid.Children.Remove(foundButton);
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
            WheelPageGrid.Children.Add(newButton);
        }
        private void UpdateOrCreateBorder(string borderName, Thickness margin, double width, double height, Brush borderBrush, double borderThickness)
        {

            Border foundBorder = (Border)WheelPageGrid.FindName(borderName);

            if (foundBorder != null)
            {
                WheelPageGrid.Children.Remove(foundBorder);
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
            WheelPageGrid.Children.Add(newBorder);
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
            NewDevice_Legacy NewDeviceWindow = new NewDevice_Legacy();
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

            // Add the ColorPicker to the WheelPageGrid
            WheelPageGrid.Children.Add(colorPicker);
        }

        private void InitializeDirectInput()
        {
            directInput = new DirectInput();

            // Find the first connected joystick
            var joysticks = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
            if (joysticks.Any())
            {
                joystick = new Joystick(directInput, joysticks.First().InstanceGuid);
                joystick.Acquire();
            }
            else
            {
                MessageBox.Show("No gamepad connected!");
            }
        }
        private void PollGamepad()
        {
            if (joystick == null) return;

            joystickState = joystick.GetCurrentState();

            // Check if the 'A' button is pressed
            if (joystickState.Buttons[0]) // Button index 0 is 'A' button
            {
                MessageBox.Show("A Button Pressed!");
            }

            // Check if the 'B' button is pressed
            if (joystickState.Buttons[1]) // Button index 1 is 'B' button
            {
                MessageBox.Show("B Button Pressed!");
            }

            // Check if the 'X' button is pressed
            if (joystickState.Buttons[2]) // Button index 2 is 'X' button
            {
                MessageBox.Show("X Button Pressed!");
            }

            // Check if the 'Y' button is pressed
            if (joystickState.Buttons[3]) // Button index 3 is 'Y' button
            {
                MessageBox.Show("Y Button Pressed!");
            }
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
