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

using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Server.Sink
{
    /// <summary>
    /// Sink context.
    /// </summary>
    public class SinkContext
    {
        /// <summary>
        /// Storage for <see cref="BaseSink"/>.
        /// </summary>
        private BaseSink[] _sinks;

        /// <summary>
        /// The total sink count.
        /// </summary>
        private uint _sinkCount;

        /// <summary>
        /// Initialize the <see cref="SinkContext"/>.
        /// </summary>
        /// <param name="sinksCount">The total sink count.</param>
        public void Initialize(uint sinksCount)
        {
            _sinkCount = sinksCount;
            _sinks = new BaseSink[_sinkCount];

            for (int i = 0; i < _sinkCount; i++)
            {
                _sinks[i] = new BaseSink();
            }
        }

        /// <summary>
        /// Get the total sink count.
        /// </summary>
        /// <returns>The total sink count.</returns>
        public uint GetCount()
        {
            return _sinkCount;
        }

        /// <summary>
        /// Get a reference to a <see cref="BaseSink"/> at the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The index to use.</param>
        /// <returns>A reference to a <see cref="BaseSink"/> at the given <paramref name="id"/>.</returns>
        public ref BaseSink GetSink(int id)
        {
            Debug.Assert(id >= 0 && id < _sinkCount);

            return ref _sinks[id];
        }
    }
}
