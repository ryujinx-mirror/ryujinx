using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Texture
{
    public static class ImageUtils
    {
        [Flags]
        private enum TargetBuffer
        {
            Color   = 1 << 0,
            Depth   = 1 << 1,
            Stencil = 1 << 2,

            DepthStencil = Depth | Stencil
        }

        private struct ImageDescriptor
        {
            public int BytesPerPixel { get; private set; }
            public int BlockWidth    { get; private set; }
            public int BlockHeight   { get; private set; }

            public TargetBuffer Target { get; private set; }

            public ImageDescriptor(int BytesPerPixel, int BlockWidth, int BlockHeight, TargetBuffer Target)
            {
                this.BytesPerPixel = BytesPerPixel;
                this.BlockWidth    = BlockWidth;
                this.BlockHeight   = BlockHeight;
                this.Target        = Target;
            }
        }

        private const GalImageFormat Snorm = GalImageFormat.Snorm;
        private const GalImageFormat Unorm = GalImageFormat.Unorm;
        private const GalImageFormat Sint  = GalImageFormat.Sint;
        private const GalImageFormat Uint  = GalImageFormat.Uint;
        private const GalImageFormat Float = GalImageFormat.Float;
        private const GalImageFormat Srgb  = GalImageFormat.Srgb;

        private static readonly Dictionary<GalTextureFormat, GalImageFormat> s_TextureTable =
                            new Dictionary<GalTextureFormat, GalImageFormat>()
        {
            { GalTextureFormat.RGBA32,     GalImageFormat.RGBA32                    | Sint | Uint | Float        },
            { GalTextureFormat.RGBA16,     GalImageFormat.RGBA16    | Snorm | Unorm | Sint | Uint | Float        },
            { GalTextureFormat.RG32,       GalImageFormat.RG32                      | Sint | Uint | Float        },
            { GalTextureFormat.RGBA8,      GalImageFormat.RGBA8     | Snorm | Unorm | Sint | Uint         | Srgb },
            { GalTextureFormat.RGB10A2,    GalImageFormat.RGB10A2   | Snorm | Unorm | Sint | Uint                },
            { GalTextureFormat.RG8,        GalImageFormat.RG8       | Snorm | Unorm | Sint | Uint                },
            { GalTextureFormat.R16,        GalImageFormat.R16       | Snorm | Unorm | Sint | Uint | Float        },
            { GalTextureFormat.R8,         GalImageFormat.R8        | Snorm | Unorm | Sint | Uint                },
            { GalTextureFormat.RG16,       GalImageFormat.RG16      | Snorm | Unorm               | Float        },
            { GalTextureFormat.R32,        GalImageFormat.R32                       | Sint | Uint | Float        },
            { GalTextureFormat.RGBA4,      GalImageFormat.RGBA4             | Unorm                              },
            { GalTextureFormat.RGB5A1,     GalImageFormat.RGB5A1            | Unorm                              },
            { GalTextureFormat.RGB565,     GalImageFormat.RGB565            | Unorm                              },
            { GalTextureFormat.R11G11B10F, GalImageFormat.R11G11B10                               | Float        },
            { GalTextureFormat.D24S8,      GalImageFormat.D24S8             | Unorm        | Uint                },
            { GalTextureFormat.D32F,       GalImageFormat.D32                                     | Float        },
            { GalTextureFormat.D32FX24S8,  GalImageFormat.D32S8                                   | Float        },
            { GalTextureFormat.D16,        GalImageFormat.D16               | Unorm                              },

            //Compressed formats
            { GalTextureFormat.BptcSfloat,  GalImageFormat.BptcSfloat                  | Float        },
            { GalTextureFormat.BptcUfloat,  GalImageFormat.BptcUfloat                  | Float        },
            { GalTextureFormat.BptcUnorm,   GalImageFormat.BptcUnorm   | Unorm                 | Srgb },
            { GalTextureFormat.BC1,         GalImageFormat.BC1         | Unorm                 | Srgb },
            { GalTextureFormat.BC2,         GalImageFormat.BC2         | Unorm                 | Srgb },
            { GalTextureFormat.BC3,         GalImageFormat.BC3         | Unorm                 | Srgb },
            { GalTextureFormat.BC4,         GalImageFormat.BC4         | Unorm | Snorm                },
            { GalTextureFormat.BC5,         GalImageFormat.BC5         | Unorm | Snorm                },
            { GalTextureFormat.Astc2D4x4,   GalImageFormat.Astc2D4x4   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D5x5,   GalImageFormat.Astc2D5x5   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D6x6,   GalImageFormat.Astc2D6x6   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D8x8,   GalImageFormat.Astc2D8x8   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D10x10, GalImageFormat.Astc2D10x10 | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D12x12, GalImageFormat.Astc2D12x12 | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D5x4,   GalImageFormat.Astc2D5x4   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D6x5,   GalImageFormat.Astc2D6x5   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D8x6,   GalImageFormat.Astc2D8x6   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D10x8,  GalImageFormat.Astc2D10x8  | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D12x10, GalImageFormat.Astc2D12x10 | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D8x5,   GalImageFormat.Astc2D8x5   | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D10x5,  GalImageFormat.Astc2D10x5  | Unorm                 | Srgb },
            { GalTextureFormat.Astc2D10x6,  GalImageFormat.Astc2D10x6  | Unorm                 | Srgb }
        };

        private static readonly Dictionary<GalImageFormat, ImageDescriptor> s_ImageTable =
                            new Dictionary<GalImageFormat, ImageDescriptor>()
        {
            { GalImageFormat.RGBA32,      new ImageDescriptor(16, 1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGBA16,      new ImageDescriptor(8,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RG32,        new ImageDescriptor(8,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGBX8,       new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGBA8,       new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.BGRA8,       new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGB10A2,     new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.R32,         new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGBA4,       new ImageDescriptor(2,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.BptcSfloat,  new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.BptcUfloat,  new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.BGR5A1,      new ImageDescriptor(2,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGB5A1,      new ImageDescriptor(2,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RGB565,      new ImageDescriptor(2,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.BptcUnorm,   new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.RG16,        new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.RG8,         new ImageDescriptor(2,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.R16,         new ImageDescriptor(2,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.R8,          new ImageDescriptor(1,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.R11G11B10,   new ImageDescriptor(4,  1,  1,  TargetBuffer.Color) },
            { GalImageFormat.BC1,         new ImageDescriptor(8,  4,  4,  TargetBuffer.Color) },
            { GalImageFormat.BC2,         new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.BC3,         new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.BC4,         new ImageDescriptor(8,  4,  4,  TargetBuffer.Color) },
            { GalImageFormat.BC5,         new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D4x4,   new ImageDescriptor(16, 4,  4,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D5x5,   new ImageDescriptor(16, 5,  5,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D6x6,   new ImageDescriptor(16, 6,  6,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D8x8,   new ImageDescriptor(16, 8,  8,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D10x10, new ImageDescriptor(16, 10, 10, TargetBuffer.Color) },
            { GalImageFormat.Astc2D12x12, new ImageDescriptor(16, 12, 12, TargetBuffer.Color) },
            { GalImageFormat.Astc2D5x4,   new ImageDescriptor(16, 5,  4,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D6x5,   new ImageDescriptor(16, 6,  5,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D8x6,   new ImageDescriptor(16, 8,  6,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D10x8,  new ImageDescriptor(16, 10, 8,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D12x10, new ImageDescriptor(16, 12, 10, TargetBuffer.Color) },
            { GalImageFormat.Astc2D8x5,   new ImageDescriptor(16, 8,  5,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D10x5,  new ImageDescriptor(16, 10, 5,  TargetBuffer.Color) },
            { GalImageFormat.Astc2D10x6,  new ImageDescriptor(16, 10, 6,  TargetBuffer.Color) },

            { GalImageFormat.D16,   new ImageDescriptor(2, 1, 1, TargetBuffer.Depth)        },
            { GalImageFormat.D24,   new ImageDescriptor(4, 1, 1, TargetBuffer.Depth)        },
            { GalImageFormat.D24S8, new ImageDescriptor(4, 1, 1, TargetBuffer.DepthStencil) },
            { GalImageFormat.D32,   new ImageDescriptor(4, 1, 1, TargetBuffer.Depth)        },
            { GalImageFormat.D32S8, new ImageDescriptor(8, 1, 1, TargetBuffer.DepthStencil) }
        };

        public static GalImageFormat ConvertTexture(
            GalTextureFormat Format,
            GalTextureType   RType,
            GalTextureType   GType,
            GalTextureType   BType,
            GalTextureType   AType,
            bool             ConvSrgb)
        {
            if (!s_TextureTable.TryGetValue(Format, out GalImageFormat ImageFormat))
            {
                throw new NotImplementedException($"Format 0x{((int)Format):x} not implemented!");
            }

            if (!HasDepth(ImageFormat) && (RType != GType || RType != BType || RType != AType))
            {
                throw new NotImplementedException($"Per component types are not implemented!");
            }

            GalImageFormat FormatType = ConvSrgb ? Srgb : GetFormatType(RType);

            GalImageFormat CombinedFormat = (ImageFormat & GalImageFormat.FormatMask) | FormatType;

            if (!ImageFormat.HasFlag(FormatType))
            {
                throw new NotImplementedException($"Format \"{CombinedFormat}\" not implemented!");
            }

            return CombinedFormat;
        }

        public static GalImageFormat ConvertSurface(GalSurfaceFormat Format)
        {
            switch (Format)
            {
                case GalSurfaceFormat.RGBA32Float:    return GalImageFormat.RGBA32    | Float;
                case GalSurfaceFormat.RGBA32Uint:     return GalImageFormat.RGBA32    | Uint;
                case GalSurfaceFormat.RGBA16Float:    return GalImageFormat.RGBA16    | Float;
                case GalSurfaceFormat.RG32Float:      return GalImageFormat.RG32      | Float;
                case GalSurfaceFormat.RG32Sint:       return GalImageFormat.RG32      | Sint;
                case GalSurfaceFormat.RG32Uint:       return GalImageFormat.RG32      | Uint;
                case GalSurfaceFormat.BGRA8Unorm:     return GalImageFormat.BGRA8     | Unorm;
                case GalSurfaceFormat.BGRA8Srgb:      return GalImageFormat.BGRA8     | Srgb;
                case GalSurfaceFormat.RGB10A2Unorm:   return GalImageFormat.RGB10A2   | Unorm;
                case GalSurfaceFormat.RGBA8Unorm:     return GalImageFormat.RGBA8     | Unorm;
                case GalSurfaceFormat.RGBA8Srgb:      return GalImageFormat.RGBA8     | Srgb;
                case GalSurfaceFormat.RGBA8Snorm:     return GalImageFormat.RGBA8     | Snorm;
                case GalSurfaceFormat.RG16Snorm:      return GalImageFormat.RG16      | Snorm;
                case GalSurfaceFormat.RG16Unorm:      return GalImageFormat.RG16      | Unorm;
                case GalSurfaceFormat.RG16Float:      return GalImageFormat.RG16      | Float;
                case GalSurfaceFormat.R11G11B10Float: return GalImageFormat.R11G11B10 | Float;
                case GalSurfaceFormat.R32Float:       return GalImageFormat.R32       | Float;
                case GalSurfaceFormat.R32Uint:        return GalImageFormat.R32       | Uint;
                case GalSurfaceFormat.RG8Unorm:       return GalImageFormat.RG8       | Unorm;
                case GalSurfaceFormat.RG8Snorm:       return GalImageFormat.RG8       | Snorm;
                case GalSurfaceFormat.R16Float:       return GalImageFormat.R16       | Float;
                case GalSurfaceFormat.R16Unorm:       return GalImageFormat.R16       | Unorm;
                case GalSurfaceFormat.R16Uint:        return GalImageFormat.R16       | Uint;
                case GalSurfaceFormat.R8Unorm:        return GalImageFormat.R8        | Unorm;
                case GalSurfaceFormat.R8Uint:         return GalImageFormat.R8        | Uint;
                case GalSurfaceFormat.B5G6R5Unorm:    return GalImageFormat.RGB565    | Unorm;
                case GalSurfaceFormat.BGR5A1Unorm:    return GalImageFormat.BGR5A1    | Unorm;
                case GalSurfaceFormat.RGBX8Unorm:     return GalImageFormat.RGBX8     | Unorm;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static GalImageFormat ConvertZeta(GalZetaFormat Format)
        {
            switch (Format)
            {
                case GalZetaFormat.D32Float:      return GalImageFormat.D32   | Float;
                case GalZetaFormat.S8D24Unorm:    return GalImageFormat.D24S8 | Unorm;
                case GalZetaFormat.D16Unorm:      return GalImageFormat.D16   | Unorm;
                case GalZetaFormat.D24X8Unorm:    return GalImageFormat.D24   | Unorm;
                case GalZetaFormat.D24S8Unorm:    return GalImageFormat.D24S8 | Unorm;
                case GalZetaFormat.D32S8X24Float: return GalImageFormat.D32S8 | Float;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static byte[] ReadTexture(IMemory Memory, GalImage Image, long Position)
        {
            MemoryManager CpuMemory;

            if (Memory is NvGpuVmm Vmm)
            {
                CpuMemory = Vmm.Memory;
            }
            else
            {
                CpuMemory = (MemoryManager)Memory;
            }

            ISwizzle Swizzle = TextureHelper.GetSwizzle(Image);

            ImageDescriptor Desc = GetImageDescriptor(Image.Format);

            (int Width, int Height) = GetImageSizeInBlocks(Image);

            int BytesPerPixel = Desc.BytesPerPixel;

            //Note: Each row of the texture needs to be aligned to 4 bytes.
            int Pitch = (Width * BytesPerPixel + 3) & ~3;

            byte[] Data = new byte[Height * Pitch];

            for (int Y = 0; Y < Height; Y++)
            {
                int OutOffs = Y * Pitch;

                for (int X = 0; X < Width; X++)
                {
                    long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                    CpuMemory.ReadBytes(Position + Offset, Data, OutOffs, BytesPerPixel);

                    OutOffs += BytesPerPixel;
                }
            }

            return Data;
        }

        public static void WriteTexture(NvGpuVmm Vmm, GalImage Image, long Position, byte[] Data)
        {
            ISwizzle Swizzle = TextureHelper.GetSwizzle(Image);

            ImageDescriptor Desc = GetImageDescriptor(Image.Format);

            (int Width, int Height) = ImageUtils.GetImageSizeInBlocks(Image);

            int BytesPerPixel = Desc.BytesPerPixel;

            int InOffs = 0;

            for (int Y = 0; Y < Height; Y++)
            for (int X = 0; X < Width;  X++)
            {
                long Offset = (uint)Swizzle.GetSwizzleOffset(X, Y);

                Vmm.Memory.WriteBytes(Position + Offset, Data, InOffs, BytesPerPixel);

                InOffs += BytesPerPixel;
            }
        }

        public static bool CopyTexture(
            NvGpuVmm Vmm,
            GalImage SrcImage,
            GalImage DstImage,
            long     SrcAddress,
            long     DstAddress,
            int      SrcX,
            int      SrcY,
            int      DstX,
            int      DstY,
            int      Width,
            int      Height)
        {
            ISwizzle SrcSwizzle = TextureHelper.GetSwizzle(SrcImage);
            ISwizzle DstSwizzle = TextureHelper.GetSwizzle(DstImage);

            ImageDescriptor Desc = GetImageDescriptor(SrcImage.Format);

            if (GetImageDescriptor(DstImage.Format).BytesPerPixel != Desc.BytesPerPixel)
            {
                return false;
            }

            int BytesPerPixel = Desc.BytesPerPixel;

            for (int Y = 0; Y < Height; Y++)
            for (int X = 0; X < Width;  X++)
            {
                long SrcOffset = (uint)SrcSwizzle.GetSwizzleOffset(SrcX + X, SrcY + Y);
                long DstOffset = (uint)DstSwizzle.GetSwizzleOffset(DstX + X, DstY + Y);

                byte[] Texel = Vmm.ReadBytes(SrcAddress + SrcOffset, BytesPerPixel);

                Vmm.WriteBytes(DstAddress + DstOffset, Texel);
            }

            return true;
        }

        public static int GetSize(GalImage Image)
        {
            ImageDescriptor Desc = GetImageDescriptor(Image.Format);

            int Width  = DivRoundUp(Image.Width,  Desc.BlockWidth);
            int Height = DivRoundUp(Image.Height, Desc.BlockHeight);

            return Desc.BytesPerPixel * Width * Height;
        }

        public static int GetPitch(GalImageFormat Format, int Width)
        {
            ImageDescriptor Desc = GetImageDescriptor(Format);

            int Pitch = Desc.BytesPerPixel * DivRoundUp(Width, Desc.BlockWidth);

            Pitch = (Pitch + 0x1f) & ~0x1f;

            return Pitch;
        }

        public static int GetBlockWidth(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).BlockWidth;
        }

        public static int GetBlockHeight(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).BlockHeight;
        }

        public static int GetAlignedWidth(GalImage Image)
        {
            ImageDescriptor Desc = GetImageDescriptor(Image.Format);

             int AlignMask;

            if (Image.Layout == GalMemoryLayout.BlockLinear)
            {
                AlignMask = Image.TileWidth * (64 / Desc.BytesPerPixel) - 1;
            }
            else
            {
                AlignMask = (32 / Desc.BytesPerPixel) - 1;
            }

            return (Image.Width + AlignMask) & ~AlignMask;
        }

        public static (int Width, int Height) GetImageSizeInBlocks(GalImage Image)
        {
            ImageDescriptor Desc = GetImageDescriptor(Image.Format);

            return (DivRoundUp(Image.Width,  Desc.BlockWidth),
                    DivRoundUp(Image.Height, Desc.BlockHeight));
        }

        public static int GetBytesPerPixel(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).BytesPerPixel;
        }

        private static int DivRoundUp(int LHS, int RHS)
        {
            return (LHS + (RHS - 1)) / RHS;
        }

        public static bool HasColor(GalImageFormat Format)
        {
            return (GetImageDescriptor(Format).Target & TargetBuffer.Color) != 0;
        }

        public static bool HasDepth(GalImageFormat Format)
        {
            return (GetImageDescriptor(Format).Target & TargetBuffer.Depth) != 0;
        }

        public static bool HasStencil(GalImageFormat Format)
        {
            return (GetImageDescriptor(Format).Target & TargetBuffer.Stencil) != 0;
        }

        public static bool IsCompressed(GalImageFormat Format)
        {
            ImageDescriptor Desc = GetImageDescriptor(Format);

            return (Desc.BlockWidth | Desc.BlockHeight) != 1;
        }

        private static ImageDescriptor GetImageDescriptor(GalImageFormat Format)
        {
            GalImageFormat PixelFormat = Format & GalImageFormat.FormatMask;

            if (s_ImageTable.TryGetValue(PixelFormat, out ImageDescriptor Descriptor))
            {
                return Descriptor;
            }

            throw new NotImplementedException($"Format \"{PixelFormat}\" not implemented!");
        }

        private static GalImageFormat GetFormatType(GalTextureType Type)
        {
            switch (Type)
            {
                case GalTextureType.Snorm: return Snorm;
                case GalTextureType.Unorm: return Unorm;
                case GalTextureType.Sint:  return Sint;
                case GalTextureType.Uint:  return Uint;
                case GalTextureType.Float: return Float;

                default: throw new NotImplementedException(((int)Type).ToString());
            }
        }
    }
}
