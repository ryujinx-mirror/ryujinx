namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Viewport extents for viewport clipping, also includes depth range.
    /// </summary>
    struct ViewportExtents
    {
#pragma warning disable CS0649
        public ushort X;
        public ushort Width;
        public ushort Y;
        public ushort Height;
        public float  DepthNear;
        public float  DepthFar;
#pragma warning restore CS0649
    }
}
