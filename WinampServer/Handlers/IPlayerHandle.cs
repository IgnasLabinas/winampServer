namespace WinampServer.Handlers
{
    public interface IPlayerHandle
    {
        void Play();
        void Pause();
        void Stop();
        void Next();

        bool IsPlaying { get; }
    }
}
