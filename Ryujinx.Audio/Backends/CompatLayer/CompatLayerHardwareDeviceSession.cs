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
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Backends.CompatLayer
{
    class CompatLayerHardwareDeviceSession : HardwareDeviceSessionOutputBase
    {
        private HardwareDeviceSessionOutputBase _realSession;
        private uint _userChannelCount;

        public CompatLayerHardwareDeviceSession(HardwareDeviceSessionOutputBase realSession, uint userChannelCount) : base(realSession.MemoryManager, realSession.RequestedSampleFormat, realSession.RequestedSampleRate, userChannelCount)
        {
            _realSession = realSession;
            _userChannelCount = userChannelCount;
        }

        public override void Dispose()
        {
            _realSession.Dispose();
        }

        public override ulong GetPlayedSampleCount()
        {
            return _realSession.GetPlayedSampleCount();
        }

        public override float GetVolume()
        {
            return _realSession.GetVolume();
        }

        public override void PrepareToClose()
        {
            _realSession.PrepareToClose();
        }

        public override void QueueBuffer(AudioBuffer buffer)
        {
            _realSession.QueueBuffer(buffer);
        }

        public override bool RegisterBuffer(AudioBuffer buffer, byte[] samples)
        {
            if (RequestedSampleFormat != SampleFormat.PcmInt16)
            {
                throw new NotImplementedException("Downmixing formats other than PCM16 is not supported.");
            }

            if (samples == null)
            {
                return false;
            }

            short[] downmixedBufferPCM16;

            ReadOnlySpan<short> samplesPCM16 = MemoryMarshal.Cast<byte, short>(samples);

            if (_userChannelCount == 6)
            {
                downmixedBufferPCM16 = Downmixing.DownMixSurroundToStereo(samplesPCM16);

                if (_realSession.RequestedChannelCount == 1)
                {
                    downmixedBufferPCM16 = Downmixing.DownMixStereoToMono(downmixedBufferPCM16);
                }
            }
            else if (_userChannelCount == 2 && _realSession.RequestedChannelCount == 1)
            {
                downmixedBufferPCM16 = Downmixing.DownMixStereoToMono(samplesPCM16);
            }
            else
            {
                throw new NotImplementedException($"Downmixing from {_userChannelCount} to {_realSession.RequestedChannelCount} not implemented.");
            }

            byte[] downmixedBuffer = MemoryMarshal.Cast<short, byte>(downmixedBufferPCM16).ToArray();

            AudioBuffer fakeBuffer = new AudioBuffer
            {
                BufferTag = buffer.BufferTag,
                DataPointer = buffer.DataPointer,
                DataSize  = (ulong)downmixedBuffer.Length
            };

            bool result = _realSession.RegisterBuffer(fakeBuffer, downmixedBuffer);

            if (result)
            {
                buffer.Data = fakeBuffer.Data;
                buffer.DataSize = fakeBuffer.DataSize;
            }

            return result;
        }

        public override void SetVolume(float volume)
        {
            _realSession.SetVolume(volume);
        }

        public override void Start()
        {
            _realSession.Start();
        }

        public override void Stop()
        {
            _realSession.Stop();
        }

        public override void UnregisterBuffer(AudioBuffer buffer)
        {
            _realSession.UnregisterBuffer(buffer);
        }

        public override bool WasBufferFullyConsumed(AudioBuffer buffer)
        {
            return _realSession.WasBufferFullyConsumed(buffer);
        }
    }
}
