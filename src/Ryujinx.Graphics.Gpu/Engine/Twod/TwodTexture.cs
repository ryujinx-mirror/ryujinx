using Ryujinx.Graphics.Gpu.Engine.Types;

namespace Ryujinx.Graphics.Gpu.Engine.Twod
{
    /// <summary>
    /// Texture to texture (with optional resizing) copy parameters.
    /// </summary>
    struct TwodTexture
    {
#pragma warning disable CS0649 // Field is never assigned to
        public ColorFormat Format;
        public Boolean32 LinearLayout;
        public MemoryLayout MemoryLayout;
        public int Depth;
        public int Layer;
        public int Stride;
        public int Width;
        public int Height;
        public GpuVa Address;
#pragma warning restore CS0649
    }
}
