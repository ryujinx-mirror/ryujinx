namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Audio device state.
    /// </summary>
    public enum AudioDeviceState : uint
    {
        /// <summary>
        /// The audio device is started.
        /// </summary>
        Started,

        /// <summary>
        /// The audio device is stopped.
        /// </summary>
        Stopped,
    }
}
