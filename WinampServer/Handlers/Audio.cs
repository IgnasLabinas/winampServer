using AudioSwitcher.AudioApi.CoreAudio;

namespace WinampServer.Handlers
{
    public class Audio
    {
        private static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

        public static double Volume
        {
            get { return defaultPlaybackDevice.Volume; }
            set { defaultPlaybackDevice.Volume = value; }
        }

        public static bool Mute
        {
            get { return defaultPlaybackDevice.IsMuted; }
            set { defaultPlaybackDevice.Mute(value); }
        }
    }
}
