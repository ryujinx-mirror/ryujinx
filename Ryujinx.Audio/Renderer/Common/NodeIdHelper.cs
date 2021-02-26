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
    /// Helper for manipulating node ids.
    /// </summary>
    public static class NodeIdHelper
    {
        /// <summary>
        /// Get the type of a node from a given node id.
        /// </summary>
        /// <param name="nodeId">Id of the node.</param>
        /// <returns>The type of the node.</returns>
        public static NodeIdType GetType(int nodeId)
        {
            return (NodeIdType)(nodeId >> 28);
        }

        /// <summary>
        /// Get the base of a node from a given node id.
        /// </summary>
        /// <param name="nodeId">Id of the node.</param>
        /// <returns>The base of the node.</returns>
        public static int GetBase(int nodeId)
        {
            return (nodeId >> 16) & 0xFFF;
        }
    }
}
