using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace SpotifyWallpaper
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon _notifyIcon;
        private bool _close;
        private Helper _helper;

        public MainWindow()
        {
            InitializeComponent();

            Assembly myAssembly = Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("SpotifyWallpaper.TrayIcon.ico");
            Icon icon = new Icon(myStream);

            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = icon
            };
            //_notifyIcon.Click += SwitcHWindowState;

            MenuItem[] menuItems = new MenuItem[2];
            menuItems[0] = new MenuItem("Open", OpenMenu_OnClick);
            menuItems[1] = new MenuItem("Close", CloseMenu_OnClick);
            _notifyIcon.ContextMenu = new ContextMenu(menuItems);
           

            AutostartButton.IsChecked = Autostart();
        }

        private void CloseMenu_OnClick(object sender, EventArgs e)
        {
            CloseButton_OnClick(this, new RoutedEventArgs());
        }

        private void OpenMenu_OnClick(object sender, EventArgs e)
        {
            SwitcHWindowState(this, e);
        }

        private void SwitcHWindowState(object sender, EventArgs e)
        {
            if (Visibility == Visibility.Hidden)
            {
                Topmost = true;
                Topmost = false;
                Visibility = Visibility.Visible;
            }
            else Visibility = Visibility.Hidden;
        }

        private void MainWindow_OnClosed(object sender, CancelEventArgs e)
        {
            if (_close) return;

            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            _close = true;

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            _helper?.SetDefaultWallpaper();
            _helper?.Dispose();

            Close();
        }

        private void SetBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            _helper.SetDefaultWallpaperPath();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            PrintMessage("Stopping background helper!");
            _helper.Dispose();
            _helper = new Helper(this);
        }

        public void PrintMessage(string message)
        {
            ErrorText.Text += message + Environment.NewLine;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _helper = new Helper(this);
        }

        private void AutostartButton_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (AutostartButton.IsChecked == true)
            {
                key.SetValue("SpotifyWallpaper", System.Windows.Application.ResourceAssembly.Location);
            }
            else if (AutostartButton.IsChecked == false)
            {
                key.DeleteValue("SpotifyWallpaper");
            }
        }

        private bool Autostart()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            return key.GetValueNames().Contains("SpotifyWallpaper");
        }
    }
}