using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Renderer output information on REV5 and later.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RendererInfoOutStatus
    {
        /// <summary>
        /// The count of updates sent to the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public ulong ElapsedFrameCount;

        /// <summary>
        /// Reserved/Unused.
        /// </summary>
        private readonly ulong _reserved;
    }
}
