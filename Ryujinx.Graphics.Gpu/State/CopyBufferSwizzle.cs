namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Buffer to buffer copy vector swizzle parameters.
    /// </summary>
    struct CopyBufferSwizzle
    {
#pragma warning disable CS0649
        public uint Swizzle;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks the source for the buffer destination vector X component.
        /// </summary>
        /// <returns>Destination component</returns>
        public BufferSwizzleComponent UnpackDstX()
        {
            return (BufferSwizzleComponent)(Swizzle & 7);
        }

        /// <summary>
        /// Unpacks the source for the buffer destination vector Y component.
        /// </summary>
        /// <returns>Destination component</returns>
        public BufferSwizzleComponent UnpackDstY()
        {
            return (BufferSwizzleComponent)((Swizzle >> 4) & 7);
        }

        /// <summary>
        /// Unpacks the source for the buffer destination vector Z component.
        /// </summary>
        /// <returns>Destination component</returns>
        public BufferSwizzleComponent UnpackDstZ()
        {
            return (BufferSwizzleComponent)((Swizzle >> 8) & 7);
        }

        /// <summary>
        /// Unpacks the source for the buffer destination vector W component.
        /// </summary>
        /// <returns>Destination component</returns>
        public BufferSwizzleComponent UnpackDstW()
        {
            return (BufferSwizzleComponent)((Swizzle >> 12) & 7);
        }

        /// <summary>
        /// Unpacks the size of each vector component of the copy.
        /// </summary>
        /// <returns>Vector component size</returns>
        public int UnpackComponentSize()
        {
            return (int)((Swizzle >> 16) & 3) + 1;
        }

        /// <summary>
        /// Unpacks the number of components of the source vector of the copy.
        /// </summary>
        /// <returns>Number of vector components</returns>
        public int UnpackSrcComponentsCount()
        {
            return (int)((Swizzle >> 20) & 7) + 1;
        }

        /// <summary>
        /// Unpacks the number of components of the destination vector of the copy.
        /// </summary>
        /// <returns>Number of vector components</returns>
        public int UnpackDstComponentsCount()
        {
            return (int)((Swizzle >> 24) & 7) + 1;
        }
    }
}