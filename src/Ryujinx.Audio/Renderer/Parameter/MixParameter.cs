using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a mix.
    /// </summary>
    /// <remarks>Also used on the client side for mix tracking.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MixParameter
    {
        /// <summary>
        /// Base volume of the mix.
        /// </summary>
        public float Volume;

        /// <summary>
        /// Target sample rate of the mix.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Target buffer count.
        /// </summary>
        public uint BufferCount;

        /// <summary>
        /// Set to true if in use.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;

        /// <summary>
        /// Set to true if it was changed.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsDirty;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly ushort _reserved1;

        /// <summary>
        /// The id of the mix.
        /// </summary>
        public int MixId;

        /// <summary>
        /// The effect count. (client side)
        /// </summary>
        public uint EffectCount;

        /// <summary>
        /// The mix node id.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly ulong _reserved2;

        /// <summary>
        /// Mix buffer volumes storage.
        /// </summary>
        private MixVolumeArray _mixBufferVolumeArray;

        /// <summary>
        /// The mix to output the result of this mix.
        /// </summary>
        public int DestinationMixId;

        /// <summary>
        /// The splitter to output the result of this mix.
        /// </summary>
        public uint DestinationSplitterId;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly uint _reserved3;

        [StructLayout(LayoutKind.Sequential, Size = 4 * Constants.MixBufferCountMax * Constants.MixBufferCountMax, Pack = 1)]
        private struct MixVolumeArray { }

        /// <summary>
        /// Mix buffer volumes.
        /// </summary>
        /// <remarks>Used when no splitter id is specified.</remarks>
        public Span<float> MixBufferVolume => SpanHelpers.AsSpan<MixVolumeArray, float>(ref _mixBufferVolumeArray);
    }
}
