using System;
using WinampServer.Models;

namespace WinampServer.Handlers
{
    public class Timer : IDisposable
    {
        private Player _player;
        private System.Timers.Timer _timer;

        public Timer(Player player)
        {
            _player = player;

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (sender, e) => HandleTimer();
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void HandleTimer()
        {
            switch (_player.Status.Mode)
            {
                case PlayerMode.Youtube:
                    if (_player.Status.State == PlayerState.Playing)
                    {
                        if (_player.Status.CurrentSong != null)
                        {
                            // increment current song seconds
                            _player.Status.CurrentSong.CurrentSecond += 1;

                            // check if song is ended
                            if (_player.Status.CurrentSong.CurrentSecond > _player.Status.CurrentSong.Length)
                            {
                                // auto add related video if queue is empty
                                if (_player.Status.Queue.Count == 0)
                                {
                                    //check if user wants, to play selected radio after all youtube videos in queue runs out
                                    //if yes, then play selected radio. If no - find related video and play it
                                    if (_player.Status.YouTubeEndRadio != null)
                                    {
                                        _player.SetRadio(_player.Status.YouTubeEndRadio.Key, _player.ServerClient);
                                        _player.SetMode(PlayerMode.Radio);
                                        _player.Hub.RefreshClients();
                                        _player.Log("Started radio " + _player.Status.YouTubeEndRadio.Title, _player.ServerClient);
                                    }
                                    else
                                    {
                                        string relatedSongKey = Youtube.FindNextSongKey(_player.Status.CurrentSong.Key);
                                        if (relatedSongKey != null)
                                        {
                                            _player.QueueSong(relatedSongKey, _player.ServerClient);
                                            _player.Log("Added Youtube key " + relatedSongKey, _player.ServerClient);
                                        }
                                    }
                                }

                                // if any song in queue, play it
                                if (_player.Status.Queue.Count > 0)
                                {
                                    _player.Handler.Next();
                                    _player.Handler.Play();
                                    _player.Hub.RefreshClients();
                                }
                                else
                                {
                                    _player.Status.CurrentSong = null;
                                    _player.Handler.Stop();
                                    _player.Hub.RefreshClients();
                                }
                            }
                        }
                    }
                    break;
                case PlayerMode.Radio:
                    // bug fix for vlc not starting after switch to radio from youtube
                    // means: player is playing, but vlc not
                    if (_player.Status.State == PlayerState.Playing && !_player.Handler.IsPlaying)
                    {
                        _player.Handler.Play();
                    }
                    break;
            }
        }
    }
}
