using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinampServer.Handlers;
using WinampServer.Models;

namespace WinampServer
{
    public class Player : IDisposable
    {
        private string _statusFileName;
        private string _youtubeHistoryFileName;
        private string _usersNamesFileName;

        private PlayerStatus _status;
        private List<string> _youtubeHistory;
        private Dictionary<string, string> _usersNames;
        private Dictionary<string, Client> _clients;
        private Client _serverClient = new Client { Name = "Server (auto)" };

        private List<Radio> _radios;

        // handlers
        private Timer _timer;
        private VLC _vlc;
        private Youtube _youtube;

        private IPlayerHandle _currentHandler;

        public Client ServerClient { get { return _serverClient; } }
        public SignalRHub Hub { get; set; }
        public PlayerStatus Status
        {
            get
            {
                return _status;
            }
        }
        public List<Radio> Radios
        {
            get
            {
                return _radios;
            }
        }
        public IPlayerHandle Handler
        {
            get
            {
                return _currentHandler;
            }
        }

        public Player()
        {
            _status = new PlayerStatus();
            _clients = new Dictionary<string, Client>();
            
            // init handlers
            _youtube = new Youtube(this);
            _vlc = new VLC(this);
            _timer = new Timer(this);

            _currentHandler = _youtube;
        }


        public void Dispose()
        {
            _timer.Dispose();
            _vlc.Dispose();
            _youtube.Dispose();
        }

        #region public actions

        public void HandleCommand(string commandString, Client client = null)
        {
            try
            {
                // create log entry and send them to clients and server console
                Log(commandString, client);

                // get command and its params
                string[] commandStringSplitted = commandString.Split(new[] { ":" }, StringSplitOptions.None);
                string command = commandStringSplitted[0];
                string commandArgument = commandStringSplitted.Length > 1 ? commandStringSplitted[1] : "";

                // make actions based on command
                switch (command)
                {
                    case "m":
                        _currentHandler.Pause();
                        switch (commandArgument)
                        {
                            case "y":
                                Status.Mode = PlayerMode.Youtube;
                                _currentHandler = _youtube;
                                break;
                            case "r":
                                Status.Mode = PlayerMode.Radio;
                                _currentHandler = _vlc;
                                break;
                        }
                        _currentHandler.Play();
                        break;
                    case "v":
                        {
                            int volumeArg;
                            if (int.TryParse(commandArgument, out volumeArg))
                            {
                                Status.Volume = volumeArg;
                                Audio.Volume = volumeArg;
                            }
                            break;
                        }
                    case "w":
                        QueueSong(commandArgument, client);
                        break;
                    case "r":
                        SetRadio(commandArgument, client);
                        _currentHandler.Play();
                        break;
                    case "play":
                    case "start":
                        Status.State = PlayerState.Playing;
                        _currentHandler.Play();
                        break;
                    case "stop":
                        Status.State = PlayerState.Stopped;
                        _currentHandler.Stop();
                        break;
                    case "pause":
                        Status.State = PlayerState.Paused;
                        _currentHandler.Pause();
                        break;
                    case "next":
                        _currentHandler.Next();
                        _currentHandler.Play();
                        break;
                    case "show ie":
                        _youtube._browser.Visible = true;
                        break;
                    case "hide ie":
                        _youtube._browser.Visible = false;
                        break;
                    case "register user":
                        client.Name = commandArgument.Trim();
                        _usersNames[client.IP] = client.Name;
                        SaveUserConfig();
                        break;
                    case "yer":
                        Status.YouTubeEndRadio = FindRadio(commandArgument);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Command {0} not available.", commandString));
                }
            }
            catch (Exception ex)
            {

            }

            // refresh clients
            Hub.RefreshClients();
        }

        public void Log(string text, Client client = null)
        {
            string logEntry = $"[{client.Name}]> {text}";
            Hub.Clients.All.ConsoleLog(logEntry);
            Program.ConsoleLog(logEntry);
        }
        
        public void SetMode(PlayerMode mode, Client client = null)
        {
            Status.Mode = mode;
            _currentHandler.Pause();
            switch (mode)
            {
                case PlayerMode.Youtube: _currentHandler = _youtube;
                    break;
                case PlayerMode.Radio: _currentHandler = _vlc;
                    break;
            }
            _currentHandler.Play();
        }

        public void QueueSong(string key, Client client = null)
        {
            Song song = Youtube.FetchSong(key);
            if (song != null)
            {
                song.Author = client;
                Status.Queue.Enqueue(song);
            }
        }

        public void AddSongToHistory(Song song)
        {
            if (song != null)
            {
                _youtubeHistory.Add(string.Format("{0} - [{1}] {2} {3} (by {4})",
                    song.Played.ToString("yyyy-MM-dd HH:mm:ss"), song.Key, song.SongLength, song.Title, song.Author.Name));
                SaveYoutubeHistory();
            }
        }

        public void SetRadio(string key, Client client = null)
        {
            Radio radio = FindRadio(key);
            if (radio != null)
            {
                radio.Author = client;
                Status.CurrentRadio = radio;
            }
        }

        #endregion

        private Radio FindRadio(string key)
        {
            if (_radios != null && _radios.Count > 0)
            {
                return _radios.FirstOrDefault(r => r.Key == key);
            }
            return null;
        }

        #region Configs

        public void LoadUserConfig(string userConfigFile)
        {
            _usersNamesFileName = userConfigFile;
            _usersNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(userConfigFile));
        }

        public void LoadRadioConfig(string radiosConfigFile)
        {
            _radios = JsonConvert.DeserializeObject<List<Radio>>(File.ReadAllText(radiosConfigFile));
        }

        public void LoadStatus(string statusFile)
        {
            _statusFileName = statusFile;
            _status = JsonConvert.DeserializeObject<PlayerStatus>(File.ReadAllText(statusFile));
        }

        public void LoadYoutubeHistory(string youtubeHistoryFile)
        {
            _youtubeHistoryFileName = youtubeHistoryFile;
            _youtubeHistory = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(youtubeHistoryFile));
        }

        public void SaveUserConfig()
        {
            File.WriteAllText(_usersNamesFileName, JsonConvert.SerializeObject(_usersNames, Formatting.Indented));
        }

        public void SaveYoutubeHistory()
        {
            File.WriteAllText(_youtubeHistoryFileName, JsonConvert.SerializeObject(_youtubeHistory, Formatting.Indented));
        }

        public void SaveStatus()
        {
            File.WriteAllText(_statusFileName, JsonConvert.SerializeObject(Status, Formatting.Indented));
        }

        #endregion

        internal void AddClient(HubCallerContext context)
        {
            if (!_clients.ContainsKey(context.ConnectionId))
            {
                string ip = (context.Request.Environment["server.RemoteIpAddress"] ?? "undefined-ip").ToString();
                _clients.Add(context.ConnectionId, new Client
                {
                    IP = ip,
                    Name = _usersNames.ContainsKey(ip) ? _usersNames[ip] : ip
                });
            }
        }

        internal void RemoveClient(HubCallerContext context)
        {
            if (_clients.ContainsKey(context.ConnectionId))
            {
                _clients.Remove(context.ConnectionId);
            }
        }

        internal Client GetClient(HubCallerContext context)
        {
            if (_clients.ContainsKey(context.ConnectionId))
            {
                return _clients[context.ConnectionId];
            }
            return null;
        }
    }
}
