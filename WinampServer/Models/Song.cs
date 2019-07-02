namespace WinampServer.Models
{
    public class Song : PlayerItem
    {
        public string Key { get; set; }
        public int Length { get; set; }
        public string SongLength { get; set; }
        public int CurrentSecond { get; set; }
    }
}
