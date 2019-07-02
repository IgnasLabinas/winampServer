using System.Collections.Generic;

namespace WinampServer.Models
{
    public class PlayerStatus
    {
        public int Volume { get; set; }
        public PlayerMode Mode { get; set; }
        public Song CurrentSong { get; set; }
        public Queue<Song> Queue { get; set; }
        public Radio CurrentRadio { get; set; }
        public PlayerState State { get; set; }
        public Radio YouTubeEndRadio { get; set; }

        public PlayerStatus()
        {
            Mode = PlayerMode.Youtube;
            Queue = new Queue<Song>();
        }
    }
}
