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

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Late reverb reflection.
    /// </summary>
    public enum ReverbLateMode : uint
    {
        /// <summary>
        /// Room late reflection. (small acoustic space, fast reflection)
        /// </summary>
        Room,

        /// <summary>
        /// Hall late reflection. (large acoustic space, warm reflection)
        /// </summary>
        Hall,

        /// <summary>
        /// Classic plate late reflection. (clean distinctive reverb)
        /// </summary>
        Plate,

        /// <summary>
        /// Cathedral late reflection. (very large acoustic space, pronounced bright reflection)
        /// </summary>
        Cathedral,

        /// <summary>
        /// Do not apply any delay. (max delay)
        /// </summary>
        NoDelay,

        /// <summary>
        /// Max delay. (used for delay line limits)
        /// </summary>
        Limit = NoDelay
    }
}
