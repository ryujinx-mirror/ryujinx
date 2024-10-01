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
