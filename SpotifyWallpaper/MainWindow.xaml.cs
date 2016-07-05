using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyWallpaper.Properties;

namespace SpotifyWallpaper
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _close = false;
        private readonly NotifyIcon _notifyIcon;
        private readonly Helper _helper = new Helper();
        
        public MainWindow()
        {
            InitializeComponent();

            Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("SpotifyWallpaper.Sirubico-Movie-Genre-Music.ico");
            Icon icon = new Icon(myStream);

            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = icon
            };
            _notifyIcon.Click += SwitcHWindowState;

            _helper.Dispose();
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
            if (_close)
            {
                _helper.SetDefaultWallpaper();
                _notifyIcon.Visible = false;
            }
            else
            {
                Visibility = Visibility.Hidden;
                e.Cancel = true;
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            _close = true;
            _helper.Dispose();
            Close();
        }
    }
}