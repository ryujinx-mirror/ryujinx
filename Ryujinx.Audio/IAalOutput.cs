using System;

namespace Ryujinx.Audio
{
    public interface IAalOutput : IDisposable
    {
        int OpenTrack(int sampleRate, int channels, ReleaseCallback callback);

        void CloseTrack(int trackId);

        bool ContainsBuffer(int trackId, long bufferTag);

        long[] GetReleasedBuffers(int trackId, int maxCount);

        void AppendBuffer<T>(int trackId, long bufferTag, T[] buffer)  where T : struct;

        void Start(int trackId);
        void Stop(int trackId);

        PlaybackState GetState(int trackId);
    }
}