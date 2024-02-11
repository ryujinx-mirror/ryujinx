using Ryujinx.HLE.HOS.Services.SurfaceFlinger;
using System;

namespace Ryujinx.HLE.UI
{
    /// <summary>
    /// Information about the indirect layer that is being drawn to.
    /// </summary>
    class RenderingSurfaceInfo : IEquatable<RenderingSurfaceInfo>
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
                   Width == other.Width &&
                   Height == other.Height &&
                   Pitch == other.Pitch &&
                   Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            return obj is RenderingSurfaceInfo info && Equals(info);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(((ulong)ColorFormat) ^ Width ^ Height ^ Pitch ^ Size));
        }
    }
}
