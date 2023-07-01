namespace Ryujinx.Audio.Renderer.Server.Types
{
    /// <summary>
    /// The internal play state of a <see cref="Voice.VoiceState"/>
    /// </summary>
    public enum PlayState
    {
        /// <summary>
        /// The voice has been started and is playing.
        /// </summary>
        Started,

        /// <summary>
        /// The voice has been stopped.
        /// </summary>
        /// <remarks>
        /// This cannot be directly set by user.
        /// See <see cref="Stopping"/> for correct usage.
        /// </remarks>
        Stopped,

        /// <summary>
        /// The user asked the voice to be stopped.
        /// </summary>
        /// <remarks>
        /// This is changed to the <see cref="Stopped"/> state after command generation.
        /// <seealso cref="Voice.VoiceState.UpdateForCommandGeneration(Voice.VoiceContext)"/>
        /// </remarks>
        Stopping,

        /// <summary>
        /// The voice has been paused by user request.
        /// </summary>
        /// <remarks>
        /// The user can resume to the <see cref="Started"/> state.
        /// </remarks>
        Paused,
    }
}
