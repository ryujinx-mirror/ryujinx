using Ryujinx.Graphics.GAL;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Contains format tables, for texture and vertex attribute formats.
    /// </summary>
    static class FormatTable
    {
        private static Dictionary<uint, FormatInfo> _textureFormats = new Dictionary<uint, FormatInfo>()
        {
            { 0x2491d, new FormatInfo(Format.R8Unorm,           1,  1,  1,  1) },
            { 0x1249d, new FormatInfo(Format.R8Snorm,           1,  1,  1,  1) },
            { 0x4921d, new FormatInfo(Format.R8Uint,            1,  1,  1,  1) },
            { 0x36d9d, new FormatInfo(Format.R8Sint,            1,  1,  1,  1) },
            { 0x7ff9b, new FormatInfo(Format.R16Float,          1,  1,  2,  1) },
            { 0x2491b, new FormatInfo(Format.R16Unorm,          1,  1,  2,  1) },
            { 0x1249b, new FormatInfo(Format.R16Snorm,          1,  1,  2,  1) },
            { 0x4921b, new FormatInfo(Format.R16Uint,           1,  1,  2,  1) },
            { 0x36d9b, new FormatInfo(Format.R16Sint,           1,  1,  2,  1) },
            { 0x7ff8f, new FormatInfo(Format.R32Float,          1,  1,  4,  1) },
            { 0x4920f, new FormatInfo(Format.R32Uint,           1,  1,  4,  1) },
            { 0x36d8f, new FormatInfo(Format.R32Sint,           1,  1,  4,  1) },
            { 0x24918, new FormatInfo(Format.R8G8Unorm,         1,  1,  2,  2) },
            { 0x12498, new FormatInfo(Format.R8G8Snorm,         1,  1,  2,  2) },
            { 0x49218, new FormatInfo(Format.R8G8Uint,          1,  1,  2,  2) },
            { 0x36d98, new FormatInfo(Format.R8G8Sint,          1,  1,  2,  2) },
            { 0x7ff8c, new FormatInfo(Format.R16G16Float,       1,  1,  4,  2) },
            { 0x2490c, new FormatInfo(Format.R16G16Unorm,       1,  1,  4,  2) },
            { 0x1248c, new FormatInfo(Format.R16G16Snorm,       1,  1,  4,  2) },
            { 0x4920c, new FormatInfo(Format.R16G16Uint,        1,  1,  4,  2) },
            { 0x36d8c, new FormatInfo(Format.R16G16Sint,        1,  1,  4,  2) },
            { 0x7ff84, new FormatInfo(Format.R32G32Float,       1,  1,  8,  2) },
            { 0x49204, new FormatInfo(Format.R32G32Uint,        1,  1,  8,  2) },
            { 0x36d84, new FormatInfo(Format.R32G32Sint,        1,  1,  8,  2) },
            { 0x7ff82, new FormatInfo(Format.R32G32B32Float,    1,  1,  12, 3) },
            { 0x49202, new FormatInfo(Format.R32G32B32Uint,     1,  1,  12, 3) },
            { 0x36d82, new FormatInfo(Format.R32G32B32Sint,     1,  1,  12, 3) },
            { 0x24908, new FormatInfo(Format.R8G8B8A8Unorm,     1,  1,  4,  4) },
            { 0x12488, new FormatInfo(Format.R8G8B8A8Snorm,     1,  1,  4,  4) },
            { 0x49208, new FormatInfo(Format.R8G8B8A8Uint,      1,  1,  4,  4) },
            { 0x36d88, new FormatInfo(Format.R8G8B8A8Sint,      1,  1,  4,  4) },
            { 0x7ff83, new FormatInfo(Format.R16G16B16A16Float, 1,  1,  8,  4) },
            { 0x24903, new FormatInfo(Format.R16G16B16A16Unorm, 1,  1,  8,  4) },
            { 0x12483, new FormatInfo(Format.R16G16B16A16Snorm, 1,  1,  8,  4) },
            { 0x49203, new FormatInfo(Format.R16G16B16A16Uint,  1,  1,  8,  4) },
            { 0x36d83, new FormatInfo(Format.R16G16B16A16Sint,  1,  1,  8,  4) },
            { 0x7ff81, new FormatInfo(Format.R32G32B32A32Float, 1,  1,  16, 4) },
            { 0x49201, new FormatInfo(Format.R32G32B32A32Uint,  1,  1,  16, 4) },
            { 0x36d81, new FormatInfo(Format.R32G32B32A32Sint,  1,  1,  16, 4) },
            { 0x2493a, new FormatInfo(Format.D16Unorm,          1,  1,  2,  1) },
            { 0x7ffaf, new FormatInfo(Format.D32Float,          1,  1,  4,  1) },
            { 0x24a0e, new FormatInfo(Format.D24UnormS8Uint,    1,  1,  4,  2) },
            { 0x24a29, new FormatInfo(Format.D24UnormS8Uint,    1,  1,  4,  2) },
            { 0x25385, new FormatInfo(Format.D32FloatS8Uint,    1,  1,  8,  2) },
            { 0x253b0, new FormatInfo(Format.D32FloatS8Uint,    1,  1,  8,  2) },
            { 0xa4908, new FormatInfo(Format.R8G8B8A8Srgb,      1,  1,  4,  4) },
            { 0x24912, new FormatInfo(Format.R4G4B4A4Unorm,     1,  1,  2,  4) },
            { 0x24914, new FormatInfo(Format.R5G5B5A1Unorm,     1,  1,  2,  4) },
            { 0x24915, new FormatInfo(Format.R5G6B5Unorm,       1,  1,  2,  3) },
            { 0x24909, new FormatInfo(Format.R10G10B10A2Unorm,  1,  1,  4,  4) },
            { 0x49209, new FormatInfo(Format.R10G10B10A2Uint,   1,  1,  4,  4) },
            { 0x7ffa1, new FormatInfo(Format.R11G11B10Float,    1,  1,  4,  3) },
            { 0x7ffa0, new FormatInfo(Format.R9G9B9E5Float,     1,  1,  4,  4) },
            { 0x24924, new FormatInfo(Format.Bc1RgbaUnorm,      4,  4,  8,  4) },
            { 0x24925, new FormatInfo(Format.Bc2Unorm,          4,  4,  16, 4) },
            { 0x24926, new FormatInfo(Format.Bc3Unorm,          4,  4,  16, 4) },
            { 0xa4924, new FormatInfo(Format.Bc1RgbaSrgb,       4,  4,  8,  4) },
            { 0xa4925, new FormatInfo(Format.Bc2Srgb,           4,  4,  16, 4) },
            { 0xa4926, new FormatInfo(Format.Bc3Srgb,           4,  4,  16, 4) },
            { 0x24927, new FormatInfo(Format.Bc4Unorm,          4,  4,  8,  1) },
            { 0x124a7, new FormatInfo(Format.Bc4Snorm,          4,  4,  8,  1) },
            { 0x24928, new FormatInfo(Format.Bc5Unorm,          4,  4,  16, 2) },
            { 0x124a8, new FormatInfo(Format.Bc5Snorm,          4,  4,  16, 2) },
            { 0x24917, new FormatInfo(Format.Bc7Unorm,          4,  4,  16, 4) },
            { 0xa4917, new FormatInfo(Format.Bc7Srgb,           4,  4,  16, 4) },
            { 0x7ff90, new FormatInfo(Format.Bc6HSfloat,        4,  4,  16, 4) },
            { 0x7ff91, new FormatInfo(Format.Bc6HUfloat,        4,  4,  16, 4) },
            { 0x24940, new FormatInfo(Format.Astc4x4Unorm,      4,  4,  16, 4) },
            { 0x24950, new FormatInfo(Format.Astc5x4Unorm,      5,  4,  16, 4) },
            { 0x24941, new FormatInfo(Format.Astc5x5Unorm,      5,  5,  16, 4) },
            { 0x24951, new FormatInfo(Format.Astc6x5Unorm,      6,  5,  16, 4) },
            { 0x24942, new FormatInfo(Format.Astc6x6Unorm,      6,  6,  16, 4) },
            { 0x24955, new FormatInfo(Format.Astc8x5Unorm,      8,  5,  16, 4) },
            { 0x24952, new FormatInfo(Format.Astc8x6Unorm,      8,  6,  16, 4) },
            { 0x24944, new FormatInfo(Format.Astc8x8Unorm,      8,  8,  16, 4) },
            { 0x24956, new FormatInfo(Format.Astc10x5Unorm,     10, 5,  16, 4) },
            { 0x24957, new FormatInfo(Format.Astc10x6Unorm,     10, 6,  16, 4) },
            { 0x24953, new FormatInfo(Format.Astc10x8Unorm,     10, 8,  16, 4) },
            { 0x24945, new FormatInfo(Format.Astc10x10Unorm,    10, 10, 16, 4) },
            { 0x24954, new FormatInfo(Format.Astc12x10Unorm,    12, 10, 16, 4) },
            { 0x24946, new FormatInfo(Format.Astc12x12Unorm,    12, 12, 16, 4) },
            { 0xa4940, new FormatInfo(Format.Astc4x4Srgb,       4,  4,  16, 4) },
            { 0xa4950, new FormatInfo(Format.Astc5x4Srgb,       5,  4,  16, 4) },
            { 0xa4941, new FormatInfo(Format.Astc5x5Srgb,       5,  5,  16, 4) },
            { 0xa4951, new FormatInfo(Format.Astc6x5Srgb,       6,  5,  16, 4) },
            { 0xa4942, new FormatInfo(Format.Astc6x6Srgb,       6,  6,  16, 4) },
            { 0xa4955, new FormatInfo(Format.Astc8x5Srgb,       8,  5,  16, 4) },
            { 0xa4952, new FormatInfo(Format.Astc8x6Srgb,       8,  6,  16, 4) },
            { 0xa4944, new FormatInfo(Format.Astc8x8Srgb,       8,  8,  16, 4) },
            { 0xa4956, new FormatInfo(Format.Astc10x5Srgb,      10, 5,  16, 4) },
            { 0xa4957, new FormatInfo(Format.Astc10x6Srgb,      10, 6,  16, 4) },
            { 0xa4953, new FormatInfo(Format.Astc10x8Srgb,      10, 8,  16, 4) },
            { 0xa4945, new FormatInfo(Format.Astc10x10Srgb,     10, 10, 16, 4) },
            { 0xa4954, new FormatInfo(Format.Astc12x10Srgb,     12, 10, 16, 4) },
            { 0xa4946, new FormatInfo(Format.Astc12x12Srgb,     12, 12, 16, 4) },
            { 0x24913, new FormatInfo(Format.A1B5G5R5Unorm,     1,  1,  2,  4) }
        };

        private static Dictionary<ulong, Format> _attribFormats = new Dictionary<ulong, Format>()
        {
            { 0x13a00000, Format.R8Unorm             },
            { 0x0ba00000, Format.R8Snorm             },
            { 0x23a00000, Format.R8Uint              },
            { 0x1ba00000, Format.R8Sint              },
            { 0x3b600000, Format.R16Float            },
            { 0x13600000, Format.R16Unorm            },
            { 0x0b600000, Format.R16Snorm            },
            { 0x23600000, Format.R16Uint             },
            { 0x1b600000, Format.R16Sint             },
            { 0x3a400000, Format.R32Float            },
            { 0x22400000, Format.R32Uint             },
            { 0x1a400000, Format.R32Sint             },
            { 0x13000000, Format.R8G8Unorm           },
            { 0x0b000000, Format.R8G8Snorm           },
            { 0x23000000, Format.R8G8Uint            },
            { 0x1b000000, Format.R8G8Sint            },
            { 0x39e00000, Format.R16G16Float         },
            { 0x11e00000, Format.R16G16Unorm         },
            { 0x09e00000, Format.R16G16Snorm         },
            { 0x21e00000, Format.R16G16Uint          },
            { 0x19e00000, Format.R16G16Sint          },
            { 0x38800000, Format.R32G32Float         },
            { 0x20800000, Format.R32G32Uint          },
            { 0x18800000, Format.R32G32Sint          },
            { 0x12600000, Format.R8G8B8Unorm         },
            { 0x0a600000, Format.R8G8B8Snorm         },
            { 0x22600000, Format.R8G8B8Uint          },
            { 0x1a600000, Format.R8G8B8Sint          },
            { 0x38a00000, Format.R16G16B16Float      },
            { 0x10a00000, Format.R16G16B16Unorm      },
            { 0x08a00000, Format.R16G16B16Snorm      },
            { 0x20a00000, Format.R16G16B16Uint       },
            { 0x18a00000, Format.R16G16B16Sint       },
            { 0x38400000, Format.R32G32B32Float      },
            { 0x20400000, Format.R32G32B32Uint       },
            { 0x18400000, Format.R32G32B32Sint       },
            { 0x11400000, Format.R8G8B8A8Unorm       },
            { 0x09400000, Format.R8G8B8A8Snorm       },
            { 0x21400000, Format.R8G8B8A8Uint        },
            { 0x19400000, Format.R8G8B8A8Sint        },
            { 0x38600000, Format.R16G16B16A16Float   },
            { 0x10600000, Format.R16G16B16A16Unorm   },
            { 0x08600000, Format.R16G16B16A16Snorm   },
            { 0x20600000, Format.R16G16B16A16Uint    },
            { 0x18600000, Format.R16G16B16A16Sint    },
            { 0x38200000, Format.R32G32B32A32Float   },
            { 0x20200000, Format.R32G32B32A32Uint    },
            { 0x18200000, Format.R32G32B32A32Sint    },
            { 0x16000000, Format.R10G10B10A2Unorm    },
            { 0x26000000, Format.R10G10B10A2Uint     },
            { 0x3e200000, Format.R11G11B10Float      },
            { 0x2ba00000, Format.R8Uscaled           },
            { 0x33a00000, Format.R8Sscaled           },
            { 0x2b600000, Format.R16Uscaled          },
            { 0x33600000, Format.R16Sscaled          },
            { 0x2a400000, Format.R32Uscaled          },
            { 0x32400000, Format.R32Sscaled          },
            { 0x2b000000, Format.R8G8Uscaled         },
            { 0x33000000, Format.R8G8Sscaled         },
            { 0x29e00000, Format.R16G16Uscaled       },
            { 0x31e00000, Format.R16G16Sscaled       },
            { 0x28800000, Format.R32G32Uscaled       },
            { 0x30800000, Format.R32G32Sscaled       },
            { 0x2a600000, Format.R8G8B8Uscaled       },
            { 0x32600000, Format.R8G8B8Sscaled       },
            { 0x28a00000, Format.R16G16B16Uscaled    },
            { 0x30a00000, Format.R16G16B16Sscaled    },
            { 0x28400000, Format.R32G32B32Uscaled    },
            { 0x30400000, Format.R32G32B32Sscaled    },
            { 0x29400000, Format.R8G8B8A8Uscaled     },
            { 0x31400000, Format.R8G8B8A8Sscaled     },
            { 0x28600000, Format.R16G16B16A16Uscaled },
            { 0x30600000, Format.R16G16B16A16Sscaled },
            { 0x28200000, Format.R32G32B32A32Uscaled },
            { 0x30200000, Format.R32G32B32A32Sscaled },
            { 0x0e000000, Format.R10G10B10A2Snorm    },
            { 0x1e000000, Format.R10G10B10A2Sint     },
            { 0x2e000000, Format.R10G10B10A2Uscaled  },
            { 0x36000000, Format.R10G10B10A2Sscaled  }
        };

        /// <summary>
        /// Try getting the texture format from an encoded format integer from the Maxwell texture descriptor.
        /// </summary>
        /// <param name="encoded">The encoded format integer from the texture descriptor</param>
        /// <param name="isSrgb">Indicates if the format is a sRGB format</param>
        /// <param name="format">The output texture format</param>
        /// <returns>True if the format is valid, false otherwise</returns>
        public static bool TryGetTextureFormat(uint encoded, bool isSrgb, out FormatInfo format)
        {
            encoded |= (isSrgb ? 1u << 19 : 0u);

            return _textureFormats.TryGetValue(encoded, out format);
        }

        /// <summary>
        /// Try getting the vertex attribute format from an encoded format integer from Maxwell attribute registers.
        /// </summary>
        /// <param name="encoded">The encoded format integer from the attribute registers</param>
        /// <param name="format">The output vertex attribute format</param>
        /// <returns>True if the format is valid, false otherwise</returns>
        public static bool TryGetAttribFormat(uint encoded, out Format format)
        {
            return _attribFormats.TryGetValue(encoded, out format);
        }
    }
}