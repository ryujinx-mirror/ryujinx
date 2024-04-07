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

        public override void Update(out BehaviourParameter.ErrorInfo errorInfo, in SinkInParameter parameter, ref SinkOutStatus outStatus, PoolMapper mapper)
        {
            Debug.Assert(IsTypeValid(in parameter));

            ref DeviceParameter inputDeviceParameter = ref MemoryMarshal.Cast<byte, DeviceParameter>(parameter.SpecificData)[0];

            if (parameter.IsUsed != IsUsed)
            {
                UpdateStandardParameter(in parameter);
                Parameter = inputDeviceParameter;
            }
            else
            {
                Parameter.DownMixParameterEnabled = inputDeviceParameter.DownMixParameterEnabled;
                inputDeviceParameter.DownMixParameter.AsSpan().CopyTo(Parameter.DownMixParameter.AsSpan());
            }

            Parameter.DownMixParameter.AsSpan().CopyTo(DownMixCoefficients.AsSpan());

            errorInfo = new BehaviourParameter.ErrorInfo();
            outStatus = new SinkOutStatus();
        }
    }
}
