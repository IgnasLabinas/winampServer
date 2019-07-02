using System;
using System.Configuration;
using System.IO;
using Vlc.DotNet.Core;
using WinampServer.Models;

namespace WinampServer.Handlers
{
    public class VLC : IDisposable, IPlayerHandle
    {
        private Player _player;
        private VlcMediaPlayer _mediaPlayer;

        public PlayerStatus Status
        {
            get
            {
                return _player.Status;
            }
        }

        public VLC(Player player)
        {
            _player = player;

            _mediaPlayer = null;
            try
            {
                
                var libDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
                _mediaPlayer = new VlcMediaPlayer(libDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect to VLC! Radio mode is disabled.");
                Status.Mode = PlayerMode.Youtube;
            }
        }

        #region IDisposable members

        public void Dispose()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Dispose();
            }
        }

        #endregion

        #region IPlayerHandle members

        public bool IsPlaying
        {
            get
            {
                return _mediaPlayer != null ? _mediaPlayer.IsPlaying() : false;
            }
        }

        public void Play()
        {
            // if player and current radio exists, set it and start playing
            if (_mediaPlayer != null && Status.CurrentRadio != null)
            {
                _mediaPlayer.SetMedia(Status.CurrentRadio.Url);
                _mediaPlayer.Play();
            }
        }

        public void Pause()
        {
            // if player exists, pause it
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Pause();
            }
        }

        public void Stop()
        {
            // if player exists, stop it
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
            }
        }

        public void Next()
        {
            // this handler does not have next funtion
        }

        #endregion
    }
}
