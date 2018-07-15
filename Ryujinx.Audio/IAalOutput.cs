namespace Ryujinx.Audio
{
    public interface IAalOutput
    {
        int OpenTrack(int SampleRate, int Channels, ReleaseCallback Callback);

        void CloseTrack(int Track);

        bool ContainsBuffer(int Track, long Tag);

        long[] GetReleasedBuffers(int Track, int MaxCount);

        void AppendBuffer<T>(int Track, long Tag, T[] Buffer)  where T : struct;

        void Start(int Track);
        void Stop(int Track);

        PlaybackState GetState(int Track);
    }
}