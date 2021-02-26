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

using Ryujinx.Audio.Backends.Common;
using Ryujinx.Audio.Common;
using Ryujinx.Audio.Integration;
using Ryujinx.Memory;
using System.Threading;

namespace Ryujinx.Audio.Backends.Dummy
{
    internal class DummyHardwareDeviceSessionOutput : HardwareDeviceSessionOutputBase
    {
        private float _volume;
        private IHardwareDeviceDriver _manager;

        private ulong _playedSampleCount;

        public DummyHardwareDeviceSessionOutput(IHardwareDeviceDriver manager, IVirtualMemoryManager memoryManager, SampleFormat requestedSampleFormat, uint requestedSampleRate, uint requestedChannelCount) : base(memoryManager, requestedSampleFormat, requestedSampleRate, requestedChannelCount)
        {
            _volume = 1.0f;
            _manager = manager;
        }

        public override void Dispose()
        {
            // Nothing to do.
        }

        public override ulong GetPlayedSampleCount()
        {
            return Interlocked.Read(ref _playedSampleCount);
        }

        public override float GetVolume()
        {
            return _volume;
        }

        public override void PrepareToClose() { }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            Interlocked.Add(ref _playedSampleCount, GetSampleCount(buffer));

            _manager.GetUpdateRequiredEvent().Set();
        }

        public override void SetVolume(float volume)
        {
            _volume = volume;
        }

        public override void Start() { }

        public override void Stop() { }

        public override void UnregisterBuffer(AudioBuffer buffer) { }

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            return true;
        }
    }
}