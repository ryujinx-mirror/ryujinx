using Ryujinx.Audio.Renderer.Server.Effect;
using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// <see cref="IEffectInParameter.SpecificData"/> for <see cref="Common.EffectType.Delay"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DelayParameter
    {
        /// <summary>
        /// The input channel indices that will be used by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public Array6<byte> Input;

        /// <summary>
        /// The output channel indices that will be used by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public Array6<byte> Output;

        /// <summary>
        /// The maximum number of channels supported.
        /// </summary>
        public ushort ChannelCountMax;

        /// <summary>
        /// The total channel count used.
        /// </summary>
        public ushort ChannelCount;

        /// <summary>
        /// The maximum delay time in milliseconds.
        /// </summary>
        public uint DelayTimeMax;

        /// <summary>
        /// The delay time in milliseconds.
        /// </summary>
        public uint DelayTime;

        /// <summary>
        /// The target sample rate. (Q15)
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The input gain. (Q15)
        /// </summary>
        public uint InGain;

        /// <summary>
        /// The feedback gain. (Q15)
        /// </summary>
        public uint FeedbackGain;

        /// <summary>
        /// The output gain. (Q15)
        /// </summary>
        public uint OutGain;

        /// <summary>
        /// The dry gain. (Q15)
        /// </summary>
        public uint DryGain;

        /// <summary>
        /// The channel spread of the <see cref="FeedbackGain"/>. (Q15)
        /// </summary>
        public uint ChannelSpread;

        /// <summary>
        /// The low pass amount. (Q15)
        /// </summary>
        public uint LowPassAmount;

        /// <summary>
        /// The current usage status of the effect on the client side.
        /// </summary>
        public UsageState Status;

        /// <summary>
        /// Check if the <see cref="ChannelCount"/> is valid.
        /// </summary>
        /// <returns>Returns true if the <see cref="ChannelCount"/> is valid.</returns>
        public readonly bool IsChannelCountValid()
        {
            return EffectInParameterVersion1.IsChannelCountValid(ChannelCount);
        }

        /// <summary>
        /// Check if the <see cref="ChannelCountMax"/> is valid.
        /// </summary>
        /// <returns>Returns true if the <see cref="ChannelCountMax"/> is valid.</returns>
        public readonly bool IsChannelCountMaxValid()
        {
            return EffectInParameterVersion1.IsChannelCountValid(ChannelCountMax);
        }
    }
}
