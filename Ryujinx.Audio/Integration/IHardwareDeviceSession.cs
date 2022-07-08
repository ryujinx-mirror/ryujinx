using Ryujinx.Audio.Common;
using System;

namespace Ryujinx.Audio.Integration
{
    public interface IHardwareDeviceSession : IDisposable
    {
        bool RegisterBuffer(AudioBuffer buffer);

        void UnregisterBuffer(AudioBuffer buffer);

        void QueueBuffer(AudioBuffer buffer);

        bool WasBufferFullyConsumed(AudioBuffer buffer);

        void SetVolume(float volume);

        float GetVolume();

        ulong GetPlayedSampleCount();

        void Start();

        void Stop();

        void PrepareToClose();
    }
}
