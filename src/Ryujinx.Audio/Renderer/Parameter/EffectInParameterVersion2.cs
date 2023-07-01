using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for an effect version 2. (added with REV9)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EffectInParameterVersion2 : IEffectInParameter
    {
        /// <summary>
        /// Type of the effect.
        /// </summary>
        public EffectType Type;

        /// <summary>
        /// Set to true if the effect is new.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsNew;

        /// <summary>
        /// Set to true if the effect must be active.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsEnabled;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly byte _reserved1;

        /// <summary>
        /// The target mix id of the effect.
        /// </summary>
        public int MixId;

        /// <summary>
        /// Address of the processing workbuffer.
        /// </summary>
        /// <remarks>This is additional data that could be required by the effect processing.</remarks>
        public ulong BufferBase;

        /// <summary>
        /// Size of the processing workbuffer.
        /// </summary>
        /// <remarks>This is additional data that could be required by the effect processing.</remarks>
        public ulong BufferSize;

        /// <summary>
        /// Position of the effect while processing effects.
        /// </summary>
        public uint ProcessingOrder;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly uint _reserved2;

        /// <summary>
        /// Specific data storage.
        /// </summary>
        private SpecificDataStruct _specificDataStart;

        [StructLayout(LayoutKind.Sequential, Size = 0xA0, Pack = 1)]
        private struct SpecificDataStruct { }

        public Span<byte> SpecificData => SpanHelpers.AsSpan<SpecificDataStruct, byte>(ref _specificDataStart);

        readonly EffectType IEffectInParameter.Type => Type;

        readonly bool IEffectInParameter.IsNew => IsNew;

        readonly bool IEffectInParameter.IsEnabled => IsEnabled;

        readonly int IEffectInParameter.MixId => MixId;

        readonly ulong IEffectInParameter.BufferBase => BufferBase;

        readonly ulong IEffectInParameter.BufferSize => BufferSize;

        readonly uint IEffectInParameter.ProcessingOrder => ProcessingOrder;

        /// <summary>
        ///  Check if the given channel count is valid.
        /// </summary>
        /// <param name="channelCount">The channel count to check</param>
        /// <returns>Returns true if the channel count is valid.</returns>
        public static bool IsChannelCountValid(int channelCount)
        {
            return channelCount == 1 || channelCount == 2 || channelCount == 4 || channelCount == 6;
        }
    }
}
