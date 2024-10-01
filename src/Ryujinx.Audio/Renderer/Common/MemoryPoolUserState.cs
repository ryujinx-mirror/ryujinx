namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// Represents the state of a memory pool.
    /// </summary>
    public enum MemoryPoolUserState : uint
    {
        /// <summary>
        /// Invalid state.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The memory pool is new. (client side only)
        /// </summary>
        New = 1,

        /// <summary>
        /// The user asked to detach the memory pool from the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        RequestDetach = 2,

        /// <summary>
        /// The memory pool is detached from the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        Detached = 3,

        /// <summary>
        /// The user asked to attach the memory pool to the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        RequestAttach = 4,

        /// <summary>
        /// The memory pool is attached to the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        Attached = 5,

        /// <summary>
        /// The memory pool is released. (client side only)
        /// </summary>
        Released = 6,
    }
}
