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
    /// Early reverb reflection.
    /// </summary>
    public enum ReverbEarlyMode : uint
    {
        /// <summary>
        /// Room early reflection. (small acoustic space, fast reflection)
        /// </summary>
        Room,

        /// <summary>
        /// Chamber early reflection. (bigger than <see cref="Room"/>'s acoustic space, short reflection)
        /// </summary>
        Chamber,

        /// <summary>
        /// Hall early reflection. (large acoustic space, warm reflection)
        /// </summary>
        Hall,

        /// <summary>
        /// Cathedral early reflection. (very large acoustic space, pronounced bright reflection)
        /// </summary>
        Cathedral,

        /// <summary>
        /// No early reflection.
        /// </summary>
        Disabled
    }
}
