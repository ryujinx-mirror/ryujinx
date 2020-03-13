namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// Shadow RAM Control setting.
    /// </summary>
    enum ShadowRamControl
    {
        /// <summary>
        /// Track data writes and store them on shadow RAM.
        /// </summary>
        Track = 0,

        /// <summary>
        /// Track data writes and store them on shadow RAM, with filtering.
        /// </summary>
        TrackWithFilter = 1,

        /// <summary>
        /// Writes data directly without storing on shadow RAM.
        /// </summary>
        Passthrough = 2,

        /// <summary>
        /// Ignore data being written and replace with data on shadow RAM instead.
        /// </summary>
        Replay = 3
    }
}
