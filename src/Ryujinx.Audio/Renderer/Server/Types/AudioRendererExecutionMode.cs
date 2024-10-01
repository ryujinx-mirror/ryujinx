namespace Ryujinx.Audio.Renderer.Server.Types
{
    /// <summary>
    /// The execution mode of an <see cref="AudioRenderSystem"/>.
    /// </summary>
    public enum AudioRendererExecutionMode : byte
    {
        /// <summary>
        /// Automatically send commands to the DSP at a fixed rate (see <see cref="AudioRenderSystem.SendCommands"/>
        /// </summary>
        Auto,

        /// <summary>
        /// Audio renderer operation needs to be done manually via ExecuteAudioRenderer.
        /// </summary>
        /// <remarks>This is not supported on the DSP and is as such stubbed.</remarks>
        Manual,
    }
}
