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
using Ryujinx.Audio.Renderer.Server.Upsampler;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Sink
{
    /// <summary>
    /// Server information for a device sink.
    /// </summary>
    public class DeviceSink : BaseSink
    {
        /// <summary>
        /// The downmix coefficients.
        /// </summary>
        public float[] DownMixCoefficients;

        /// <summary>
        /// The device parameters.
        /// </summary>
        public DeviceParameter Parameter;

        /// <summary>
        /// The upsampler instance used by this sink.
        /// </summary>
        /// <remarks>Null if no upsampling is needed.</remarks>
        public UpsamplerState UpsamplerState;

        /// <summary>
        /// Create a new <see cref="DeviceSink"/>.
        /// </summary>
        public DeviceSink()
        {
            DownMixCoefficients = new float[4];
        }

        public override void CleanUp()
        {
            UpsamplerState?.Release();

            UpsamplerState = null;

            base.CleanUp();
        }

        public override SinkType TargetSinkType => SinkType.Device;

        public override void Update(out BehaviourParameter.ErrorInfo errorInfo, ref SinkInParameter parameter, ref SinkOutStatus outStatus, PoolMapper mapper)
        {
            Debug.Assert(IsTypeValid(ref parameter));

            ref DeviceParameter inputDeviceParameter = ref MemoryMarshal.Cast<byte, DeviceParameter>(parameter.SpecificData)[0];

            if (parameter.IsUsed != IsUsed)
            {
                UpdateStandardParameter(ref parameter);
                Parameter = inputDeviceParameter;
            }
            else
            {
                Parameter.DownMixParameterEnabled = inputDeviceParameter.DownMixParameterEnabled;
                inputDeviceParameter.DownMixParameter.ToSpan().CopyTo(Parameter.DownMixParameter.ToSpan());
            }

            Parameter.DownMixParameter.ToSpan().CopyTo(DownMixCoefficients.AsSpan());

            errorInfo = new BehaviourParameter.ErrorInfo();
            outStatus = new SinkOutStatus();
        }
    }
}
