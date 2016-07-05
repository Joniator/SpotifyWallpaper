using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;

namespace SpotifyWallpaper
{
    internal class Helper : IDisposable
    {
        private SpotifyLocalAPI _spotifyLocalApi;
        private readonly Uri _albumArtUri = new Uri(Application.StartupPath + @"\background.bmp");
        private Uri _defaultBackgroundUri = new Uri(@"F:\Pictures\Saved Pictures\Hintergrund\Background.png");
        private bool _disposed = false;
        public bool Connected = false;

        public Helper()
        {
                Task t = new Task(() =>
                {
                    while (!SpotifyLocalAPI.IsSpotifyRunning() && !_disposed) { }
                    while (!Connected && !_disposed)
                    {
                        try
                        {
                            _spotifyLocalApi = new SpotifyLocalAPI();
                            Connected = _spotifyLocalApi.Connect();
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
            get
            {
                return _spotifyLocalApi.ListenForEvents;
            }
            set
            {
                _spotifyLocalApi.ListenForEvents = value;
            }
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
            }
            catch
            {
                goto retry;
            }

            Wallpaper.Set(_albumArtUri, Wallpaper.Style.Centered);
        }
    }
}