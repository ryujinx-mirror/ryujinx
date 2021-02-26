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

using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Sink;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Sink
{
    /// <summary>
    /// Server information for a circular buffer sink.
    /// </summary>
    public class CircularBufferSink : BaseSink
    {
        /// <summary>
        /// The circular buffer parameter.
        /// </summary>
        public CircularBufferParameter Parameter;

        /// <summary>
        /// The last written data offset on the circular buffer.
        /// </summary>
        private uint _lastWrittenOffset;

        /// <summary>
        /// THe previous written offset of the circular buffer.
        /// </summary>
        private uint _oldWrittenOffset;

        /// <summary>
        /// The current offset to write data on the circular buffer.
        /// </summary>
        public uint CurrentWriteOffset { get; private set; }

        /// <summary>
        /// The <see cref="AddressInfo"/> of the circular buffer.
        /// </summary>
        public AddressInfo CircularBufferAddressInfo;

        public CircularBufferSink()
        {
            CircularBufferAddressInfo = AddressInfo.Create();
        }

        public override SinkType TargetSinkType => SinkType.CircularBuffer;

        public override void Update(out BehaviourParameter.ErrorInfo errorInfo, ref SinkInParameter parameter, ref SinkOutStatus outStatus, PoolMapper mapper)
        {
            errorInfo = new BehaviourParameter.ErrorInfo();
            outStatus = new SinkOutStatus();

            Debug.Assert(IsTypeValid(ref parameter));

            ref CircularBufferParameter inputDeviceParameter = ref MemoryMarshal.Cast<byte, CircularBufferParameter>(parameter.SpecificData)[0];

            if (parameter.IsUsed != IsUsed || ShouldSkip)
            {
                UpdateStandardParameter(ref parameter);

                if (parameter.IsUsed)
                {
                    Debug.Assert(CircularBufferAddressInfo.CpuAddress == 0);
                    Debug.Assert(CircularBufferAddressInfo.GetReference(false) == 0);

                    ShouldSkip = !mapper.TryAttachBuffer(out errorInfo, ref CircularBufferAddressInfo, inputDeviceParameter.BufferAddress, inputDeviceParameter.BufferSize);
                }
                else
                {
                    Debug.Assert(CircularBufferAddressInfo.CpuAddress != 0);
                    Debug.Assert(CircularBufferAddressInfo.GetReference(false) != 0);
                }

                Parameter = inputDeviceParameter;
            }

            outStatus.LastWrittenOffset = _lastWrittenOffset;
        }

        public override void UpdateForCommandGeneration()
        {
            Debug.Assert(Type == TargetSinkType);

            if (IsUsed)
            {
                uint frameSize = Constants.TargetSampleSize * Parameter.SampleCount * Parameter.InputCount;

                _lastWrittenOffset = _oldWrittenOffset;

                _oldWrittenOffset = CurrentWriteOffset;

                CurrentWriteOffset += frameSize;

                if (Parameter.BufferSize > 0)
                {
                    CurrentWriteOffset %= Parameter.BufferSize;
                }
            }
        }

        public override void CleanUp()
        {
            CircularBufferAddressInfo = AddressInfo.Create();
            _lastWrittenOffset = 0;
            _oldWrittenOffset = 0;
            CurrentWriteOffset = 0;
            base.CleanUp();
        }
    }
}
