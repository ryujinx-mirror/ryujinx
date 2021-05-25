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

using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// Effect result state for <seealso cref="Common.EffectType.Limiter"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LimiterStatistics
    {
        /// <summary>
        /// The max input sample value recorded by the limiter.
        /// </summary>
        public Array6<float> InputMax;

        /// <summary>
        /// Compression gain min value.
        /// </summary>
        public Array6<float> CompressionGainMin;

        /// <summary>
        /// Reset the statistics.
        /// </summary>
        public void Reset()
        {
            InputMax.ToSpan().Fill(0.0f);
            CompressionGainMin.ToSpan().Fill(1.0f);
        }
    }
}
