using Ryujinx.HLE.HOS.Services.SurfaceFlinger;

namespace Ryujinx.HLE.Ui
{
    /// <summary>
    /// Information about the indirect layer that is being drawn to.
    /// </summary>
    class RenderingSurfaceInfo
    {
        public ColorFormat ColorFormat { get; }
        public uint Width { get; }
        public uint Height { get; }
        public uint Pitch { get; }
        public uint Size { get; }

        public RenderingSurfaceInfo(ColorFormat colorFormat, uint width, uint height, uint pitch, uint size)
        {
            ColorFormat = colorFormat;
            Width = width;
            Height = height;
            Pitch = pitch;
            Size = size;
        }

        public bool Equals(RenderingSurfaceInfo other)
        {
            return ColorFormat == other.ColorFormat &&
                   Width       == other.Width       &&
                   Height      == other.Height      &&
                   Pitch       == other.Pitch       &&
                   Size        == other.Size;
        }
    }
}
