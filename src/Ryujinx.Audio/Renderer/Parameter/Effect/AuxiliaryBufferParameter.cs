using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// <see cref="IEffectInParameter.SpecificData"/> for <see cref="Common.EffectType.AuxiliaryBuffer"/> and <see cref="Common.EffectType.CaptureBuffer"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AuxiliaryBufferParameter
    {
        /// <summary>
        /// The input channel indices that will be used by the <see cref="Dsp.AudioProcessor"/> to write data to <see cref="SendBufferInfoAddress"/>.
        /// </summary>
        public Array24<byte> Input;

        /// <summary>
        /// The output channel indices that will be used by the <see cref="Dsp.AudioProcessor"/> to read data from <see cref="ReturnBufferInfoAddress"/>.
        /// </summary>
        public Array24<byte> Output;

        /// <summary>
        /// The total channel count used.
        /// </summary>
        public uint ChannelCount;

        /// <summary>
        /// The target sample rate.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The buffer storage total size.
        /// </summary>
        public uint BufferStorageSize;

        /// <summary>
        /// The maximum number of channels supported.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint ChannelCountMax;

        /// <summary>
        /// The address of the start of the region containing two <see cref="Dsp.State.AuxiliaryBufferHeader"/> followed by the data that will be written by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public ulong SendBufferInfoAddress;

        /// <summary>
        /// The address of the start of the region containling data that will be written by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public ulong SendBufferStorageAddress;

        /// <summary>
        /// The address of the start of the region containing two <see cref="Dsp.State.AuxiliaryBufferHeader"/> followed by the data that will be read by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        /// <remarks>Unused with <see cref="Common.EffectType.CaptureBuffer"/>.</remarks>
        public ulong ReturnBufferInfoAddress;

        /// <summary>
        /// The address of the start of the region containling data that will be read by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public ulong ReturnBufferStorageAddress;

        /// <summary>
        /// Size of a sample of the mix buffer.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint MixBufferSampleSize;

        /// <summary>
        /// The total count of sample that can be stored.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint TotalSampleCount;

        /// <summary>
        /// The count of sample of the mix buffer.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint MixBufferSampleCount;
    }
}
