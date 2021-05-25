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
