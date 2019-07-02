using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using SHDocVw;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using WinampServer.Models;

namespace WinampServer.Handlers
{
    public class Youtube : IDisposable, IPlayerHandle
    {
        private Player _player;
        public InternetExplorer _browser;

        public PlayerStatus Status
        {
            get
            {
                return _player.Status;
            }
        }
        
        public Youtube(Player player)
        {
            _player = player;

            _browser = new InternetExplorer();
            _browser.Visible = false;
        }

        #region IDisposable members

        public void Dispose()
        {
            if (_browser != null)
                _browser.Quit();
        }

        #endregion

        #region IPlayerHandle members

        public bool IsPlaying
        {
            get
            {
                return true;
            }
        }

        public void Play()
        {
            // take next song if non is current
            if (Status.CurrentSong == null)
            {
                Next();
            }

            // now if current exists, navigate browser to it to play
            if (Status.CurrentSong != null)
            {
                Status.CurrentSong.Played = DateTime.Now;
                _browser.Navigate(string.Format("https://www.youtube.com/watch?v={0}&t={1}",
                    Status.CurrentSong.Key, Status.CurrentSong.CurrentSecond
                ));
            }
        }

        public void Pause()
        {
            // navigate browser to blank, so its stops playing youtube
            _browser.Navigate("about:blank");
        }

        public void Stop()
        {
            // clear current song time ant stop, by navigating to blank
            if (Status.CurrentSong != null)
            {
                Status.CurrentSong.CurrentSecond = 0;
            }
            _browser.Navigate("about:blank");
        }

        public void Next()
        {
            // add current song to history
            _player.AddSongToHistory(Status.CurrentSong);

            // if any exists in queue, take it and set as current
            if (Status.Queue.Count > 0)
            {
                Status.CurrentSong = Status.Queue.Dequeue();
            }
            else
            {
                Status.CurrentSong = null;
            }
        }

        #endregion

        private static YouTubeService GetService()
        {
            // create youtube api service
            return new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCR5In4DZaTP6IEZQ0r1JceuvluJRzQNLE",
                ApplicationName = "WinampServer.Youtube"
            });
        }

        public static Song FetchSong(string key)
        {
            // call youtube api and fetch song with details by its key
            var searchListRequest = GetService().Videos.List("snippet,contentDetails");
            searchListRequest.Id = key;
            var searchListResponse = searchListRequest.ExecuteAsync();

            Task.WaitAll(searchListResponse);
            var video = searchListResponse.Result.Items.FirstOrDefault();

            if (video != null)
            {
                var songTime = XmlConvert.ToTimeSpan(video.ContentDetails.Duration);
                return new Song
                {
                    Title = video.Snippet.Title,
                    Key = key,
                    Length = (int)songTime.TotalSeconds,
                    SongLength = songTime.TotalHours >= 1 ? songTime.ToString(@"h\:m\:ss") : songTime.ToString(@"mm\:ss"),
                    CurrentSecond = 0
                };
            }
            return null;
        }

        public static string FindNextSongKey(string key)
        {
            // call youtube api and find related video key to current key
            var searchListRequest = GetService().Search.List("snippet");
            searchListRequest.RelatedToVideoId = key;
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = 10;

            var searchListResponse = searchListRequest.ExecuteAsync();
            Task.WaitAll(searchListResponse);

            // take random one from all 10 got
            return searchListResponse.Result.Items.ElementAtOrDefault(new Random().Next(1, 10))?.Id.VideoId;
        }

    }
}
