namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// The type of a sink.
    /// </summary>
    public enum SinkType : byte
    {
        /// <summary>
        /// The sink is in an invalid state.
        /// </summary>
        Invalid,

        /// <summary>
        /// The sink is a device.
        /// </summary>
        Device,

        /// <summary>
        /// The sink is a circular buffer.
        /// </summary>
        CircularBuffer,
    }
}
