namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Viewport extents for viewport clipping, also includes depth range.
    /// </summary>
    struct ViewportExtents
    {
        public ushort X;
        public ushort Width;
        public ushort Y;
        public ushort Height;
        public float  DepthNear;
        public float  DepthFar;
    }
}
