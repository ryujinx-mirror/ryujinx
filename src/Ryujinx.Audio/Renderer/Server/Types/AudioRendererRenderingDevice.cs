namespace Ryujinx.Audio.Renderer.Server.Types
{
    /// <summary>
    /// The rendering device of an <see cref="AudioRenderSystem"/>.
    /// </summary>
    public enum AudioRendererRenderingDevice : byte
    {
        /// <summary>
        /// Rendering is performed on the DSP.
        /// </summary>
        /// <remarks>
        /// Only supports <see cref="AudioRendererExecutionMode.Auto"/>.
        /// </remarks>
        Dsp,

        /// <summary>
        /// Rendering is performed on the CPU.
        /// </summary>
        /// <remarks>
        /// Only supports <see cref="AudioRendererExecutionMode.Manual"/>.
        /// </remarks>
        Cpu,
    }
}
