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
using System;

namespace Ryujinx.Audio.Backends.Dummy
{
    class DummyHardwareDeviceSessionInput : IHardwareDeviceSession
    {
        private float _volume;
        private IHardwareDeviceDriver _manager;
        private IVirtualMemoryManager _memoryManager;

        public DummyHardwareDeviceSessionInput(IHardwareDeviceDriver manager, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount)
        {
            _volume = 1.0f;
            _manager = manager;
            _memoryManager = memoryManager;
        }

        public void Dispose()
        {
            // Nothing to do.
        }

        public ulong GetPlayedSampleCount()
        {
            // Not implemented for input.
            throw new NotSupportedException();
        }

        public float GetVolume()
        {
            return _volume;
        }

        public void PrepareToClose() { }

        public void QueueBuffer(AudioBuffer buffer)
        {
            _memoryManager.Fill(buffer.DataPointer, buffer.DataSize, 0);

            _manager.GetUpdateRequiredEvent().Set();
        }

        public bool RegisterBuffer(AudioBuffer buffer)
        {
            return buffer.DataPointer != 0;
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
        }

        public void Start() { }

        public void Stop() { }

        public void UnregisterBuffer(AudioBuffer buffer) { }

        public bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            return true;
        }
    }
}
