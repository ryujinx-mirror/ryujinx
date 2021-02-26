//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;

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
            return (ulong)BackendHelper.GetSampleCount(RequestedSampleFormat, (int)RequestedChannelCount, (int)buffer.DataSize);
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

            if (buffer.Data == null)
            {
                buffer.Data = samples;
            }

            return true;
        }

        public virtual void UnregisterBuffer(AudioBuffer buffer) { }
    }
}
