namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Render target draw buffers control.
    /// </summary>
    struct RtControl
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks the number of active draw buffers.
        /// </summary>
        /// <returns>Number of active draw buffers</returns>
        public int UnpackCount()
        {
            return (int)(Packed & 0xf);
        }

        /// <summary>
        /// Unpacks the color attachment index for a given draw buffer.
        /// </summary>
        /// <param name="index">Index of the draw buffer</param>
        /// <returns>Attachment index</returns>
        public int UnpackPermutationIndex(int index)
        {
            return (int)((Packed >> (4 + index * 3)) & 7);
        }
    }
}
