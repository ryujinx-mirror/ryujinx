namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Common play state.
    /// </summary>
    public enum PlayState : byte
    {
        /// <summary>
        /// The user request the voice to be started.
        /// </summary>
        Start,

        /// <summary>
        /// The user request the voice to be stopped.
        /// </summary>
        Stop,

        /// <summary>
        /// The user request the voice to be paused.
        /// </summary>
        Pause,
    }
}
