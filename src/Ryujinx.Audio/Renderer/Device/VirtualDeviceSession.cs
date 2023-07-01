namespace Ryujinx.Audio.Renderer.Device
{
    /// <summary>
    /// Represents a virtual device session used by IAudioDevice.
    /// </summary>
    public class VirtualDeviceSession
    {
        /// <summary>
        /// The <see cref="VirtualDevice"/> associated to this session.
        /// </summary>
        public VirtualDevice Device { get; }

        /// <summary>
        /// The user volume of this session.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Create a new <see cref="VirtualDeviceSession"/> instance.
        /// </summary>
        /// <param name="virtualDevice">The <see cref="VirtualDevice"/> associated to this session.</param>
        public VirtualDeviceSession(VirtualDevice virtualDevice)
        {
            Device = virtualDevice;
        }
    }
}
