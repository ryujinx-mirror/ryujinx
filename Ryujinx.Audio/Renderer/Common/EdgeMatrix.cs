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

using Ryujinx.Audio.Renderer.Utils;
using Ryujinx.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Represents a adjacent matrix.
    /// </summary>
    /// <remarks>This is used for splitter routing.</remarks>
    public class EdgeMatrix
    {
        /// <summary>
        /// Backing <see cref="BitArray"/> used for node connections.
        /// </summary>
        private BitArray _storage;

        /// <summary>
        /// The count of nodes of the current instance.
        /// </summary>
        private int _nodeCount;

        /// <summary>
        /// Get the required work buffer size memory needed for the <see cref="EdgeMatrix"/>.
        /// </summary>
        /// <param name="nodeCount">The count of nodes.</param>
        /// <returns>The size required for the given <paramref name="nodeCount"/>.</returns>
        public static int GetWorkBufferSize(int nodeCount)
        {
            int size = BitUtils.AlignUp(nodeCount * nodeCount, Constants.BufferAlignment);

            return size / Unsafe.SizeOf<byte>();
        }

        /// <summary>
        /// Initializes the <see cref="EdgeMatrix"/> instance with backing memory.
        /// </summary>
        /// <param name="edgeMatrixWorkBuffer">The backing memory.</param>
        /// <param name="nodeCount">The count of nodes.</param>
        public void Initialize(Memory<byte> edgeMatrixWorkBuffer, int nodeCount)
        {
            Debug.Assert(edgeMatrixWorkBuffer.Length >= GetWorkBufferSize(nodeCount));

            _storage = new BitArray(edgeMatrixWorkBuffer);

            _nodeCount = nodeCount;

            _storage.Reset();
        }

        /// <summary>
        /// Test if the bit at the given index is set.
        /// </summary>
        /// <param name="index">A bit index.</param>
        /// <returns>Returns true if the bit at the given index is set</returns>
        public bool Test(int index)
        {
            return _storage.Test(index);
        }

        /// <summary>
        /// Reset all bits in the storage.
        /// </summary>
        public void Reset()
        {
            _storage.Reset();
        }

        /// <summary>
        /// Reset the bit at the given index.
        /// </summary>
        /// <param name="index">A bit index.</param>
        public void Reset(int index)
        {
            _storage.Reset(index);
        }

        /// <summary>
        /// Set the bit at the given index.
        /// </summary>
        /// <param name="index">A bit index.</param>
        public void Set(int index)
        {
            _storage.Set(index);
        }

        /// <summary>
        /// Connect a given source to a given destination.
        /// </summary>
        /// <param name="source">The source index.</param>
        /// <param name="destination">The destination index.</param>
        public void Connect(int source, int destination)
        {
            Debug.Assert(source < _nodeCount);
            Debug.Assert(destination < _nodeCount);

            _storage.Set(_nodeCount * source + destination);
        }

        /// <summary>
        /// Check if the given source is connected to the given destination.
        /// </summary>
        /// <param name="source">The source index.</param>
        /// <param name="destination">The destination index.</param>
        /// <returns>Returns true if the given source is connected to the given destination.</returns>
        public bool Connected(int source, int destination)
        {
            Debug.Assert(source < _nodeCount);
            Debug.Assert(destination < _nodeCount);

            return _storage.Test(_nodeCount * source + destination);
        }

        /// <summary>
        /// Disconnect a given source from a given destination.
        /// </summary>
        /// <param name="source">The source index.</param>
        /// <param name="destination">The destination index.</param>
        public void Disconnect(int source, int destination)
        {
            Debug.Assert(source < _nodeCount);
            Debug.Assert(destination < _nodeCount);

            _storage.Reset(_nodeCount * source + destination);
        }

        /// <summary>
        /// Remove all edges from a given source.
        /// </summary>
        /// <param name="source">The source index.</param>
        public void RemoveEdges(int source)
        {
            for (int i = 0; i < _nodeCount; i++)
            {
                Disconnect(source, i);
            }
        }

        /// <summary>
        /// Get the total node count.
        /// </summary>
        /// <returns>The total node count.</returns>
        public int GetNodeCount()
        {
            return _nodeCount;
        }
    }
}
