using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using SpotifyWallpaper.Properties;

namespace SpotifyWallpaper
{
    internal class Helper : IDisposable
    {
        private bool _connected;
        private Uri _defaultBackgroundUri;
        private bool _disposed = false;
        private SpotifyLocalAPI _spotifyLocalApi;
        private readonly MainWindow _mainWindow;

        public Helper(MainWindow window)
        {
            _mainWindow = window;
            PrintMessage("Background helper started!");
            _defaultBackgroundUri = GetDefaultWallpaperPath();
            Task t = new Task(() =>
            {
                while (SpotifyLocalAPI.IsSpotifyRunning() && !_connected && !_disposed)
                {
                    try
                    {
                        _spotifyLocalApi = new SpotifyLocalAPI();
                        _connected = _spotifyLocalApi.Connect();
                        PrintMessage("Connected: " + _connected);
                    }
                    catch (Exception e)
                    {
                        PrintMessage(e.Message);
                    }
                    _spotifyLocalApi.ListenForEvents = true;
                    _spotifyLocalApi.OnPlayStateChange += OnPlayStateChange;
                    _spotifyLocalApi.OnTrackChange += OnTrackChange;
                    SetAlbumArtWallpaper();
                }
            });
            t.Start();
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
            PrintMessage("Helper disposed!");
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
            PrintMessage("Set default wallpaper!");
        }

        private void SetAlbumArtWallpaper()
        {
            try
            {
                if (!Directory.Exists($"{Environment.CurrentDirectory}\\images")) Directory.CreateDirectory($"{Environment.CurrentDirectory}\\images");
                while (_spotifyLocalApi.GetStatus() == null) { }
                Track currentTrack = _spotifyLocalApi.GetStatus().Track;
                string fileName = $"{currentTrack.ArtistResource.Name} - {currentTrack.AlbumResource.Name}.bmp";

                foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
                {
                    fileName.Replace(invalidFileNameChar, ' ');
                }

                Uri wallpaperUri = new Uri($"{Environment.CurrentDirectory}\\images\\" + fileName);
                
                if (!File.Exists(wallpaperUri.LocalPath))
                {
                    PrintMessage($"Downloading: {fileName}");
                    byte[] albumBytes = currentTrack.GetAlbumArtAsByteArray(AlbumArtSize.Size640);

                    using (FileStream fileStream = new FileStream(wallpaperUri.LocalPath, FileMode.Create)) fileStream.Write(albumBytes, 0, albumBytes.Length);
                    PrintMessage($"Saved: {fileName}");
                }
                Wallpaper.Set(new Uri(wallpaperUri.LocalPath), Wallpaper.Style.Centered);
                PrintMessage($"Set wallpaper: {fileName}");
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
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

        private void PrintMessage(string message)
        {
            message = $"[{DateTime.Now}] {message}";
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.PrintMessage(message);
            });
            Debug.Print(message);
        }
    }
}