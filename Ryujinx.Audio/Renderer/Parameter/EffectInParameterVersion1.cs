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
using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for an effect version 1.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EffectInParameterVersion1 : IEffectInParameter
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
        private byte _reserved1;

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
        private uint _reserved2;

        /// <summary>
        /// Specific data storage.
        /// </summary>
        private SpecificDataStruct _specificDataStart;

        [StructLayout(LayoutKind.Sequential, Size = 0xA0, Pack = 1)]
        private struct SpecificDataStruct { }

        public Span<byte> SpecificData => SpanHelpers.AsSpan<SpecificDataStruct, byte>(ref _specificDataStart);

        EffectType IEffectInParameter.Type => Type;

        bool IEffectInParameter.IsNew => IsNew;

        bool IEffectInParameter.IsEnabled => IsEnabled;

        int IEffectInParameter.MixId => MixId;

        ulong IEffectInParameter.BufferBase => BufferBase;

        ulong IEffectInParameter.BufferSize => BufferSize;

        uint IEffectInParameter.ProcessingOrder => ProcessingOrder;

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
