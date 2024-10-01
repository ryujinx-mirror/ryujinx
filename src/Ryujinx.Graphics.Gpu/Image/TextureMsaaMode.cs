namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Multisampled texture samples count.
    /// </summary>
    enum TextureMsaaMode
    {
        Ms1x1 = 0,
        Ms2x2 = 2,
        Ms4x2 = 4,
        Ms2x1 = 5,
        Ms4x4 = 6,
    }

    static class TextureMsaaModeConverter
    {
        /// <summary>
        /// Returns the total number of samples from the MSAA mode.
        /// </summary>
        /// <param name="msaaMode">The MSAA mode</param>
        /// <returns>The total number of samples</returns>
        public static int SamplesCount(this TextureMsaaMode msaaMode)
        {
            return msaaMode switch
            {
                TextureMsaaMode.Ms2x1 => 2,
                TextureMsaaMode.Ms2x2 => 4,
                TextureMsaaMode.Ms4x2 => 8,
                TextureMsaaMode.Ms4x4 => 16,
                _ => 1,
            };
        }

        /// <summary>
        /// Returns the number of samples in the X direction from the MSAA mode.
        /// </summary>
        /// <param name="msaaMode">The MSAA mode</param>
        /// <returns>The number of samples in the X direction</returns>
        public static int SamplesInX(this TextureMsaaMode msaaMode)
        {
            return msaaMode switch
            {
                TextureMsaaMode.Ms2x1 => 2,
                TextureMsaaMode.Ms2x2 => 2,
                TextureMsaaMode.Ms4x2 => 4,
                TextureMsaaMode.Ms4x4 => 4,
                _ => 1,
            };
        }

        /// <summary>
        /// Returns the number of samples in the Y direction from the MSAA mode.
        /// </summary>
        /// <param name="msaaMode">The MSAA mode</param>
        /// <returns>The number of samples in the Y direction</returns>
        public static int SamplesInY(this TextureMsaaMode msaaMode)
        {
            return msaaMode switch
            {
                TextureMsaaMode.Ms2x1 => 1,
                TextureMsaaMode.Ms2x2 => 2,
                TextureMsaaMode.Ms4x2 => 2,
                TextureMsaaMode.Ms4x4 => 4,
                _ => 1,
            };
        }
    }
}
