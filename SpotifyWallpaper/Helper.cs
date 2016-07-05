using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyWallpaper.Properties;

namespace SpotifyWallpaper
{
    internal class Helper : IDisposable
    {
        private readonly Uri _albumArtUri = new Uri(Application.StartupPath + @"\background.bmp");
        private bool _connected;
        private Uri _defaultBackgroundUri;
        private bool _disposed;
        private SpotifyLocalAPI _spotifyLocalApi;

        public Helper()
        {
            _defaultBackgroundUri = GetDefaultWallpaperPath();

            Task t = new Task(() =>
            {
                while (!SpotifyLocalAPI.IsSpotifyRunning() && !_disposed) { }
                while (!_connected && !_disposed)
                {
                    try
                    {
                        _spotifyLocalApi = new SpotifyLocalAPI();
                        _connected = _spotifyLocalApi.Connect();
                        _spotifyLocalApi.ListenForEvents = true;
                        _spotifyLocalApi.OnPlayStateChange += OnPlayStateChange;
                        _spotifyLocalApi.OnTrackChange += OnTrackChange;
                        SetAlbumArtWallpaper();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });

            t.Start();
        }

        public bool Running
        {
            get { return _spotifyLocalApi.ListenForEvents; }
            set { _spotifyLocalApi.ListenForEvents = value; }
        }

        public void Dispose()
        {
            _disposed = true;
            if (_spotifyLocalApi != null)
            {
                _spotifyLocalApi.OnPlayStateChange -= OnPlayStateChange;
                _spotifyLocalApi.OnTrackChange -= OnTrackChange;
                _spotifyLocalApi.ListenForEvents = false;
            }
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

        public void SetDefaultWallpaper()
        {
            Wallpaper.Set(_defaultBackgroundUri, Wallpaper.Style.Centered);
        }

        private void SetAlbumArtWallpaper()
        {
            byte[] albumBytes = _spotifyLocalApi.GetStatus().Track.GetAlbumArtAsByteArray(AlbumArtSize.Size640);
            retry:
            Thread.Sleep(10);
            try
            {
                if (!File.Exists(_albumArtUri.LocalPath)) File.Create(_albumArtUri.LocalPath).Close();
                using (FileStream fileStream = new FileStream(_albumArtUri.OriginalString, FileMode.Create)) fileStream.Write(albumBytes, 0, albumBytes.Length);
                Wallpaper.Set(_albumArtUri, Wallpaper.Style.Centered);
            }
            catch
            {
                using (FileStream fileStream = new FileStream(_albumArtUri.OriginalString+".bmp", FileMode.Create)) fileStream.Write(albumBytes, 0, albumBytes.Length);
                Wallpaper.Set(new Uri(_albumArtUri.AbsolutePath+ ".bmp"), Wallpaper.Style.Centered);
            }
        }

        public void SetDefaultWallpaperPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetDefaultWallpaperPath(new Uri(openFileDialog.FileName));
            }
        }

        private void SetDefaultWallpaperPath(Uri path)
        {
            _defaultBackgroundUri = path;
            Settings.Default.Background = path;
            Settings.Default.Save();
        }

        private Uri GetDefaultWallpaperPath()
        {
            if (Settings.Default.Background != null)
            {
                return Settings.Default.Background;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetDefaultWallpaperPath(new Uri(openFileDialog.FileName));
            }
            return GetDefaultWallpaperPath();
        }
    }
}