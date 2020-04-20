namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Render target color buffer mask.
    /// This defines which color channels are written to the color buffer.
    /// </summary>
    struct RtColorMask
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks red channel enable.
        /// </summary>
        /// <returns>True to write the new red channel color, false to keep the old value</returns>
        public bool UnpackRed()
        {
            return (Packed & 0x1) != 0;
        }

        /// <summary>
        /// Unpacks green channel enable.
        /// </summary>
        /// <returns>True to write the new green channel color, false to keep the old value</returns>
        public bool UnpackGreen()
        {
            return (Packed & 0x10) != 0;
        }

        /// <summary>
        /// Unpacks blue channel enable.
        /// </summary>
        /// <returns>True to write the new blue channel color, false to keep the old value</returns>
        public bool UnpackBlue()
        {
            return (Packed & 0x100) != 0;
        }

        /// <summary>
        /// Unpacks alpha channel enable.
        /// </summary>
        /// <returns>True to write the new alpha channel color, false to keep the old value</returns>
        public bool UnpackAlpha()
        {
            return (Packed & 0x1000) != 0;
        }
    }
}
