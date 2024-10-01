using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Backends.Common
{
    public abstract class HardwareDeviceSessionOutputBase : IHardwareDeviceSession
    {
        public IVirtualMemoryManager MemoryManager { get; }
        public SampleFormat RequestedSampleFormat { get; }
        public uint RequestedSampleRate { get; }
        public uint RequestedChannelCount { get; }

        public HardwareDeviceSessionOutputBase(IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount)
        {
            MemoryManager = memoryManager;
            RequestedSampleFormat = requestedSampleFormat;
            RequestedSampleRate = requestedSampleRate;
            RequestedChannelCount = requestedChannelCount;
        }

        private byte[] GetBufferSamples(AudioBuffer buffer)
        {
            if (buffer.DataPointer == 0)
            {
                return null;
            }

            byte[] data = new byte[buffer.DataSize];

            MemoryManager.Read(buffer.DataPointer, data);

            return data;
        }

        protected ulong GetSampleCount(AudioBuffer buffer)
        {
            return GetSampleCount((int)buffer.DataSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong GetSampleCount(int dataSize)
        {
            return (ulong)BackendHelper.GetSampleCount(RequestedSampleFormat, (int)RequestedChannelCount, dataSize);
        }

        public abstract void Dispose();
        public abstract void PrepareToClose();
        public abstract void QueueBuffer(AudioBuffer buffer);
        public abstract void SetVolume(float volume);
        public abstract float GetVolume();
        public abstract void Start();
        public abstract void Stop();
        public abstract ulong GetPlayedSampleCount();
        public abstract bool WasBufferFullyConsumed(AudioBuffer buffer);
        public virtual bool RegisterBuffer(AudioBuffer buffer)
        {
            return RegisterBuffer(buffer, GetBufferSamples(buffer));
        }

        public virtual bool RegisterBuffer(AudioBuffer buffer, byte[] samples)
        {
            if (samples == null)
            {
                return false;
            }

            buffer.Data ??= samples;

            return true;
        }

        public virtual void UnregisterBuffer(AudioBuffer buffer) { }
    }
}
