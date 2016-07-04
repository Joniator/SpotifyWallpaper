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
        private readonly SpotifyLocalAPI _spotifyLocalApi;
        private readonly Uri _albumArtUri = new Uri(@"F:\Pictures\Saved Pictures\Hintergrund\spotifyalbumart.bmp");
        private readonly Uri _defaultBackgroundUri = new Uri(@"F:\Pictures\Saved Pictures\Hintergrund\Background.png");
        private readonly NotifyIcon _notifyIcon;
        private bool _close = false;
        public MainWindow()
        {
            InitializeComponent();

            do
            {
                _spotifyLocalApi = new SpotifyLocalAPI();
                _spotifyLocalApi.ListenForEvents = _spotifyLocalApi.Connect();
                _spotifyLocalApi.OnPlayStateChange += OnPlayStateChange;
                _spotifyLocalApi.OnTrackChange += OnTrackChange;
                
            } while (!SpotifyLocalAPI.IsSpotifyRunning());

            Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("SpotifyWallpaper.Sirubico-Movie-Genre-Music.ico");
            Icon icon = new Icon(myStream);
             

            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = icon
            };
            _notifyIcon.Click += SwitcHWindowState;
        }

        private void SwitcHWindowState(object sender, EventArgs e)
        {
            if (Visibility == Visibility.Hidden) Visibility = Visibility.Visible;
            else Visibility = Visibility.Hidden;
        }

        private void OnTrackChange(object sender, TrackChangeEventArgs e)
        {
            SetAlbumArtWallpaper();
        }

        private void OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            if (e.Playing)
            {
                SetAlbumArtWallpaper();
            }
            else
            {
                SetDefaultWallpaper();
            }
        }

        private void SetDefaultWallpaper()
        {
            Wallpaper.Set(_defaultBackgroundUri, Wallpaper.Style.Centered);
        }

        private void SetAlbumArtWallpaper()
        {
            Thread.Sleep(1000);
            retry:
            try
            {
                byte[] albumBytes = _spotifyLocalApi.GetStatus().Track.GetAlbumArtAsByteArray(AlbumArtSize.Size640);
                using (FileStream fileStream = new FileStream(_albumArtUri.OriginalString, FileMode.Create)) fileStream.Write(albumBytes, 0, albumBytes.Length);
            }
            catch
            {
                goto retry;
            }

            Wallpaper.Set(_albumArtUri, Wallpaper.Style.Centered);
        }

        private void MainWindow_OnClosed(object sender, CancelEventArgs e)
        {
            if (_close)
            {
                SetDefaultWallpaper();
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
            Close();
        }
    }
}