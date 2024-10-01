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
        Performance = 15,
    }
}
