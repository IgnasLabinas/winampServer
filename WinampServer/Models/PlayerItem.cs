using System;

namespace WinampServer.Models
{
    public abstract class PlayerItem
    {
        public string Title { get; set; }
        public Client Author { get; set; }
        public DateTime Played { get; set; }
    }
}
