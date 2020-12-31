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
    /// The type of a node.
    /// </summary>
    public enum NodeIdType : byte
    {
        /// <summary>
        /// Invalid node id.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Voice related node id. (data source, biquad filter, ...)
        /// </summary>
        Voice = 1,

        /// <summary>
        /// Mix related node id. (mix, effects, splitters, ...)
        /// </summary>
        Mix = 2,

        /// <summary>
        /// Sink related node id. (device &amp; circular buffer sink)
        /// </summary>
        Sink = 3,

        /// <summary>
        /// Performance monitoring related node id (performance commands)
        /// </summary>
        Performance = 15
    }
}
