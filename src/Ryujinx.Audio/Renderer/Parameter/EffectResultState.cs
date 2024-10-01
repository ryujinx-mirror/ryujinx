using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Effect result state (added in REV9).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EffectResultState
    {
        /// <summary>
        /// Specific data storage.
        /// </summary>
        private SpecificDataStruct _specificDataStart;

        [StructLayout(LayoutKind.Sequential, Size = 0x80, Pack = 1)]
        private struct SpecificDataStruct { }

        /// <summary>
        /// Specific data changing depending of the type of effect. See also the <see cref="Effect"/> namespace.
        /// </summary>
        public Span<byte> SpecificData => SpanHelpers.AsSpan<SpecificDataStruct, byte>(ref _specificDataStart);
    }
}
