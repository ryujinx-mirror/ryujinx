namespace Ryujinx.Audio
{
    public interface IAalOutput
    {
        int OpenTrack(int SampleRate, int Channels, out AudioFormat Format);
        void CloseTrack(int Track);

        void AppendBuffer(int Track, long Tag, byte[] Buffer);        
        bool ContainsBuffer(int Track, long Tag);

        long[] GetReleasedBuffers(int Track);

        void Start(int Track);
        void Stop(int Track);

        PlaybackState GetState(int Track);
    }
}