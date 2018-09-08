using Ryujinx.Graphics.Gal;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Texture
{
    static class ImageUtils
    {
        struct ImageDescriptor
        {
            public TextureReaderDelegate Reader;

            public bool HasColor;
            public bool HasDepth;
            public bool HasStencil;

            public bool Compressed;

            public ImageDescriptor(
                TextureReaderDelegate Reader,
                bool                  HasColor,
                bool                  HasDepth,
                bool                  HasStencil,
                bool                  Compressed)
            {
                this.Reader     = Reader;
                this.HasColor   = HasColor;
                this.HasDepth   = HasDepth;
                this.HasStencil = HasStencil;
                this.Compressed = Compressed;
            }
        }

        private const GalImageFormat Snorm  = GalImageFormat.Snorm;
        private const GalImageFormat Unorm  = GalImageFormat.Unorm;
        private const GalImageFormat Sint   = GalImageFormat.Sint;
        private const GalImageFormat Uint   = GalImageFormat.Uint;
        private const GalImageFormat Sfloat = GalImageFormat.Sfloat;

        private static readonly Dictionary<GalTextureFormat, GalImageFormat> s_TextureTable =
                            new Dictionary<GalTextureFormat, GalImageFormat>()
            {
                { GalTextureFormat.R32G32B32A32, GalImageFormat.R32G32B32A32                 | Sint | Uint | Sfloat },
                { GalTextureFormat.R16G16B16A16, GalImageFormat.R16G16B16A16 | Snorm | Unorm | Sint | Uint | Sfloat },
                { GalTextureFormat.R32G32,       GalImageFormat.R32G32                       | Sint | Uint | Sfloat },
                { GalTextureFormat.A8B8G8R8,     GalImageFormat.A8B8G8R8     | Snorm | Unorm | Sint | Uint          },
                { GalTextureFormat.A2B10G10R10,  GalImageFormat.A2B10G10R10  | Snorm | Unorm | Sint | Uint          },
                { GalTextureFormat.G8R8,         GalImageFormat.G8R8         | Snorm | Unorm | Sint | Uint          },
                { GalTextureFormat.R16,          GalImageFormat.R16          | Snorm | Unorm | Sint | Uint | Sfloat },
                { GalTextureFormat.R8,           GalImageFormat.R8           | Snorm | Unorm | Sint | Uint          },
                { GalTextureFormat.R32,          GalImageFormat.R32                          | Sint | Uint | Sfloat },
                { GalTextureFormat.A4B4G4R4,     GalImageFormat.A4B4G4R4             | Unorm                        },
                { GalTextureFormat.A1B5G5R5,     GalImageFormat.A1R5G5B5             | Unorm                        },
                { GalTextureFormat.B5G6R5,       GalImageFormat.B5G6R5               | Unorm                        },
                { GalTextureFormat.BF10GF11RF11, GalImageFormat.B10G11R11                                  | Sfloat },
                { GalTextureFormat.Z24S8,        GalImageFormat.D24_S8               | Unorm                        },
                { GalTextureFormat.ZF32,         GalImageFormat.D32                                        | Sfloat },
                { GalTextureFormat.ZF32_X24S8,   GalImageFormat.D32_S8               | Unorm                        },

                //Compressed formats
                { GalTextureFormat.BC6H_SF16,   GalImageFormat.BC6H_SF16  | Unorm         },
                { GalTextureFormat.BC6H_UF16,   GalImageFormat.BC6H_UF16  | Unorm         },
                { GalTextureFormat.BC7U,        GalImageFormat.BC7        | Unorm         },
                { GalTextureFormat.BC1,         GalImageFormat.BC1_RGBA   | Unorm         },
                { GalTextureFormat.BC2,         GalImageFormat.BC2        | Unorm         },
                { GalTextureFormat.BC3,         GalImageFormat.BC3        | Unorm         },
                { GalTextureFormat.BC4,         GalImageFormat.BC4        | Unorm | Snorm },
                { GalTextureFormat.BC5,         GalImageFormat.BC5        | Unorm | Snorm },
                { GalTextureFormat.Astc2D4x4,   GalImageFormat.ASTC_4x4   | Unorm         },
                { GalTextureFormat.Astc2D5x5,   GalImageFormat.ASTC_5x5   | Unorm         },
                { GalTextureFormat.Astc2D6x6,   GalImageFormat.ASTC_6x6   | Unorm         },
                { GalTextureFormat.Astc2D8x8,   GalImageFormat.ASTC_8x8   | Unorm         },
                { GalTextureFormat.Astc2D10x10, GalImageFormat.ASTC_10x10 | Unorm         },
                { GalTextureFormat.Astc2D12x12, GalImageFormat.ASTC_12x12 | Unorm         },
                { GalTextureFormat.Astc2D5x4,   GalImageFormat.ASTC_5x4   | Unorm         },
                { GalTextureFormat.Astc2D6x5,   GalImageFormat.ASTC_6x5   | Unorm         },
                { GalTextureFormat.Astc2D8x6,   GalImageFormat.ASTC_8x6   | Unorm         },
                { GalTextureFormat.Astc2D10x8,  GalImageFormat.ASTC_10x8  | Unorm         },
                { GalTextureFormat.Astc2D12x10, GalImageFormat.ASTC_12x10 | Unorm         },
                { GalTextureFormat.Astc2D8x5,   GalImageFormat.ASTC_8x5   | Unorm         },
                { GalTextureFormat.Astc2D10x5,  GalImageFormat.ASTC_10x5  | Unorm         },
                { GalTextureFormat.Astc2D10x6,  GalImageFormat.ASTC_10x6  | Unorm         }
            };

        private static readonly Dictionary<GalImageFormat, ImageDescriptor> s_ImageTable =
                            new Dictionary<GalImageFormat, ImageDescriptor>()
            {
                { GalImageFormat.R32G32B32A32,  new ImageDescriptor(TextureReader.Read16Bpp,                       true, false, false, false) },
                { GalImageFormat.R16G16B16A16,  new ImageDescriptor(TextureReader.Read8Bpp,                        true, false, false, false) },
                { GalImageFormat.R32G32,        new ImageDescriptor(TextureReader.Read8Bpp,                        true, false, false, false) },
                { GalImageFormat.A8B8G8R8,      new ImageDescriptor(TextureReader.Read4Bpp,                        true, false, false, false) },
                { GalImageFormat.A2B10G10R10,   new ImageDescriptor(TextureReader.Read4Bpp,                        true, false, false, false) },
                { GalImageFormat.R32,           new ImageDescriptor(TextureReader.Read4Bpp,                        true, false, false, false) },
                { GalImageFormat.A4B4G4R4,      new ImageDescriptor(TextureReader.Read2Bpp,                        true, false, false, false) },
                { GalImageFormat.BC6H_SF16,     new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.BC6H_UF16,     new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.A1R5G5B5,      new ImageDescriptor(TextureReader.Read5551,                        true, false, false, false) },
                { GalImageFormat.B5G6R5,        new ImageDescriptor(TextureReader.Read565,                         true, false, false, false) },
                { GalImageFormat.BC7,           new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.R16G16,        new ImageDescriptor(TextureReader.Read4Bpp,                        true, false, false, false) },
                { GalImageFormat.R8G8,          new ImageDescriptor(TextureReader.Read2Bpp,                        true, false, false, false) },
                { GalImageFormat.G8R8,          new ImageDescriptor(TextureReader.Read2Bpp,                        true, false, false, false) },
                { GalImageFormat.R16,           new ImageDescriptor(TextureReader.Read2Bpp,                        true, false, false, false) },
                { GalImageFormat.R8,            new ImageDescriptor(TextureReader.Read1Bpp,                        true, false, false, false) },
                { GalImageFormat.B10G11R11,     new ImageDescriptor(TextureReader.Read4Bpp,                        true, false, false, false) },
                { GalImageFormat.A8B8G8R8_SRGB, new ImageDescriptor(TextureReader.Read4Bpp,                        true, false, false, false) },
                { GalImageFormat.BC1_RGBA,      new ImageDescriptor(TextureReader.Read8Bpt4x4,                     true, false, false, true)  },
                { GalImageFormat.BC2,           new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.BC3,           new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.BC4,           new ImageDescriptor(TextureReader.Read8Bpt4x4,                     true, false, false, true)  },
                { GalImageFormat.BC5,           new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.ASTC_4x4,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture4x4,   true, false, false, true)  },
                { GalImageFormat.ASTC_5x5,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture5x5,   true, false, false, true)  },
                { GalImageFormat.ASTC_6x6,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture6x6,   true, false, false, true)  },
                { GalImageFormat.ASTC_8x8,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture8x8,   true, false, false, true)  },
                { GalImageFormat.ASTC_10x10,    new ImageDescriptor(TextureReader.Read16BptCompressedTexture10x10, true, false, false, true)  },
                { GalImageFormat.ASTC_12x12,    new ImageDescriptor(TextureReader.Read16BptCompressedTexture12x12, true, false, false, true)  },
                { GalImageFormat.ASTC_5x4,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture5x4,   true, false, false, true)  },
                { GalImageFormat.ASTC_6x5,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture6x5,   true, false, false, true)  },
                { GalImageFormat.ASTC_8x6,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture8x6,   true, false, false, true)  },
                { GalImageFormat.ASTC_10x8,     new ImageDescriptor(TextureReader.Read16BptCompressedTexture10x8,  true, false, false, true)  },
                { GalImageFormat.ASTC_12x10,    new ImageDescriptor(TextureReader.Read16BptCompressedTexture12x10, true, false, false, true)  },
                { GalImageFormat.ASTC_8x5,      new ImageDescriptor(TextureReader.Read16BptCompressedTexture8x5,   true, false, false, true)  },
                { GalImageFormat.ASTC_10x5,     new ImageDescriptor(TextureReader.Read16BptCompressedTexture10x5,  true, false, false, true)  },
                { GalImageFormat.ASTC_10x6,     new ImageDescriptor(TextureReader.Read16BptCompressedTexture10x6,  true, false, false, true)  },

                { GalImageFormat.D24_S8, new ImageDescriptor(TextureReader.Read4Bpp, false, true, true,  false)  },
                { GalImageFormat.D32,    new ImageDescriptor(TextureReader.Read4Bpp, false, true, false, false) },
                { GalImageFormat.D16,    new ImageDescriptor(TextureReader.Read2Bpp, false, true, false, false) },
                { GalImageFormat.D32_S8, new ImageDescriptor(TextureReader.Read8Bpp, false, true, true,  false)  },
            };

        public static GalImageFormat ConvertTexture(
            GalTextureFormat Format,
            GalTextureType RType,
            GalTextureType GType,
            GalTextureType BType,
            GalTextureType AType)
        {
            if (RType != GType || RType != BType || RType != AType)
            {
                throw new NotImplementedException("Per component types are not implemented");
            }

            if (!s_TextureTable.TryGetValue(Format, out GalImageFormat ImageFormat))
            {
                throw new NotImplementedException("Texture with format " + ((int)Format).ToString("x2") + " not implemented");
            }

            GalTextureType Type = RType;

            GalImageFormat FormatType = GetFormatType(RType);

            if (ImageFormat.HasFlag(FormatType))
            {
                return (ImageFormat & GalImageFormat.FormatMask) | FormatType;
            }
            else
            {
                throw new NotImplementedException("Texture with format " + Format +
                                                  " and component type " + Type + " is not implemented");
            }
        }

        public static GalImageFormat ConvertSurface(GalSurfaceFormat Format)
        {
            switch (Format)
            {
                case GalSurfaceFormat.RGBA32Float:    return GalImageFormat.R32G32B32A32   | Sfloat;
                case GalSurfaceFormat.RGBA16Float:    return GalImageFormat.R16G16B16A16   | Sfloat;
                case GalSurfaceFormat.RG32Float:      return GalImageFormat.R32G32         | Sfloat;
                case GalSurfaceFormat.RG32Sint:       return GalImageFormat.R32G32         | Sint;
                case GalSurfaceFormat.RG32Uint:       return GalImageFormat.R32G32         | Uint;
                case GalSurfaceFormat.BGRA8Unorm:     return GalImageFormat.R8G8B8A8       | Unorm; //Is this right?
                case GalSurfaceFormat.BGRA8Srgb:      return GalImageFormat.A8B8G8R8_SRGB;          //This one might be wrong
                case GalSurfaceFormat.RGB10A2Unorm:   return GalImageFormat.A2B10G10R10    | Unorm;
                case GalSurfaceFormat.RGBA8Unorm:     return GalImageFormat.A8B8G8R8       | Unorm;
                case GalSurfaceFormat.RGBA8Srgb:      return GalImageFormat.A8B8G8R8_SRGB;
                case GalSurfaceFormat.RGBA8Snorm:     return GalImageFormat.A8B8G8R8       | Snorm;
                case GalSurfaceFormat.RG16Snorm:      return GalImageFormat.R16G16         | Snorm;
                case GalSurfaceFormat.RG16Float:      return GalImageFormat.R16G16         | Sfloat;
                case GalSurfaceFormat.R11G11B10Float: return GalImageFormat.B10G11R11      | Sfloat;
                case GalSurfaceFormat.R32Float:       return GalImageFormat.R32            | Sfloat;
                case GalSurfaceFormat.RG8Unorm:       return GalImageFormat.R8G8           | Unorm;
                case GalSurfaceFormat.RG8Snorm:       return GalImageFormat.R8             | Snorm;
                case GalSurfaceFormat.R16Float:       return GalImageFormat.R16            | Sfloat;
                case GalSurfaceFormat.R8Unorm:        return GalImageFormat.R8             | Unorm;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static GalImageFormat ConvertZeta(GalZetaFormat Format)
        {
            switch (Format)
            {
                case GalZetaFormat.Z32Float:      return GalImageFormat.D32    | Sfloat;
                case GalZetaFormat.S8Z24Unorm:    return GalImageFormat.D24_S8 | Unorm;
                case GalZetaFormat.Z16Unorm:      return GalImageFormat.D16    | Unorm;
                //This one might not be Uint, change when a texture uses this format
                case GalZetaFormat.Z32S8X24Float: return GalImageFormat.D32_S8 | Uint;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static TextureReaderDelegate GetReader(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).Reader;
        }

        public static int GetSize(GalImage Image)
        {
            switch (Image.Format & GalImageFormat.FormatMask)
            {
                case GalImageFormat.R32G32B32A32:
                    return Image.Width * Image.Height * 16;

                case GalImageFormat.R16G16B16A16:
                case GalImageFormat.D32_S8:
                case GalImageFormat.R32G32:
                    return Image.Width * Image.Height * 8;

                case GalImageFormat.A8B8G8R8:
                case GalImageFormat.A8B8G8R8_SRGB:
                case GalImageFormat.A2B10G10R10:
                case GalImageFormat.R16G16:
                case GalImageFormat.R32:
                case GalImageFormat.D32:
                case GalImageFormat.B10G11R11:
                case GalImageFormat.D24_S8:
                    return Image.Width * Image.Height * 4;

                case GalImageFormat.B4G4R4A4:
                case GalImageFormat.A1R5G5B5:
                case GalImageFormat.B5G6R5:
                case GalImageFormat.R8G8:
                case GalImageFormat.R16:
                case GalImageFormat.D16:
                    return Image.Width * Image.Height * 2;

                case GalImageFormat.R8:
                    return Image.Width * Image.Height;

                case GalImageFormat.BC1_RGBA:
                case GalImageFormat.BC4:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 4, 4, 8);
                }

                case GalImageFormat.BC6H_SF16:
                case GalImageFormat.BC6H_UF16:
                case GalImageFormat.BC7:
                case GalImageFormat.BC2:
                case GalImageFormat.BC3:
                case GalImageFormat.BC5:
                case GalImageFormat.ASTC_4x4:
                    return CompressedTextureSize(Image.Width, Image.Height, 4, 4, 16);

                case GalImageFormat.ASTC_5x5:
                    return CompressedTextureSize(Image.Width, Image.Height, 5, 5, 16);

                case GalImageFormat.ASTC_6x6:
                    return CompressedTextureSize(Image.Width, Image.Height, 6, 6, 16);

                case GalImageFormat.ASTC_8x8:
                    return CompressedTextureSize(Image.Width, Image.Height, 8, 8, 16);

                case GalImageFormat.ASTC_10x10:
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 10, 16);

                case GalImageFormat.ASTC_12x12:
                    return CompressedTextureSize(Image.Width, Image.Height, 12, 12, 16);

                case GalImageFormat.ASTC_5x4:
                    return CompressedTextureSize(Image.Width, Image.Height, 5, 4, 16);

                case GalImageFormat.ASTC_6x5:
                    return CompressedTextureSize(Image.Width, Image.Height, 6, 5, 16);

                case GalImageFormat.ASTC_8x6:
                    return CompressedTextureSize(Image.Width, Image.Height, 8, 6, 16);

                case GalImageFormat.ASTC_10x8:
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 8, 16);

                case GalImageFormat.ASTC_12x10:
                    return CompressedTextureSize(Image.Width, Image.Height, 12, 10, 16);

                case GalImageFormat.ASTC_8x5:
                    return CompressedTextureSize(Image.Width, Image.Height, 8, 5, 16);

                case GalImageFormat.ASTC_10x5:
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 5, 16);

                case GalImageFormat.ASTC_10x6:
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 6, 16);
            }

            throw new NotImplementedException((Image.Format & GalImageFormat.FormatMask).ToString());
        }

        public static bool HasColor(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).HasColor;
        }

        public static bool HasDepth(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).HasDepth;
        }

        public static bool HasStencil(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).HasStencil;
        }

        public static bool IsCompressed(GalImageFormat Format)
        {
            return GetImageDescriptor(Format).Compressed;
        }

        private static ImageDescriptor GetImageDescriptor(GalImageFormat Format)
        {
            GalImageFormat TypeLess = (Format & GalImageFormat.FormatMask);

            if (s_ImageTable.TryGetValue(TypeLess, out ImageDescriptor Descriptor))
            {
                return Descriptor;
            }

            throw new NotImplementedException("Image with format " + TypeLess.ToString() + " not implemented");
        }

        private static GalImageFormat GetFormatType(GalTextureType Type)
        {
            switch (Type)
            {
                case GalTextureType.Snorm: return Snorm;
                case GalTextureType.Unorm: return Unorm;
                case GalTextureType.Sint:  return Sint;
                case GalTextureType.Uint:  return Uint;
                case GalTextureType.Float: return Sfloat;

                default: throw new NotImplementedException(((int)Type).ToString());
            }
        }

        private static int CompressedTextureSize(int TextureWidth, int TextureHeight, int BlockWidth, int BlockHeight, int Bpb)
        {
            int W = (TextureWidth + (BlockWidth - 1)) / BlockWidth;
            int H = (TextureHeight + (BlockHeight - 1)) / BlockHeight;

            return W * H * Bpb;
        }
    }
}