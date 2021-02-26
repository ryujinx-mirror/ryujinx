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
using System.Threading;

using static Ryujinx.Audio.Integration.IHardwareDeviceDriver;

namespace Ryujinx.Audio.Backends.Dummy
{
    public class DummyHardwareDeviceDriver : IHardwareDeviceDriver
    {
        private ManualResetEvent _updateRequiredEvent;

        public DummyHardwareDeviceDriver()
        {
            _updateRequiredEvent = new ManualResetEvent(false);
        }

        public IHardwareDeviceSession OpenDeviceSession(Direction direction, IVirtualMemoryManager memoryManager, SampleFormat sampleFormat, uint sampleRate, uint channelCount)
        {
            if (sampleRate == 0)
            {
                sampleRate = Constants.TargetSampleRate;
            }

            if (channelCount == 0)
            {
                channelCount = 2;
            }

            if (direction == Direction.Output)
            {
                return new DummyHardwareDeviceSessionOutput(this, memoryManager, sampleFormat, sampleRate, channelCount);
            }
            else
            {
                return new DummyHardwareDeviceSessionInput(this, memoryManager, sampleFormat, sampleRate, channelCount);
            }
        }

        public ManualResetEvent GetUpdateRequiredEvent()
        {
            return _updateRequiredEvent;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // NOTE: The _updateRequiredEvent will be disposed somewhere else.
            }
        }

        public bool SupportsSampleRate(uint sampleRate)
        {
            return true;
        }

        public bool SupportsSampleFormat(SampleFormat sampleFormat)
        {
            return true;
        }

        public bool SupportsDirection(Direction direction)
        {
            return direction == Direction.Output || direction == Direction.Input;
        }

        public bool SupportsChannelCount(uint channelCount)
        {
            return channelCount == 1 || channelCount == 2 || channelCount == 6;
        }
    }
}
