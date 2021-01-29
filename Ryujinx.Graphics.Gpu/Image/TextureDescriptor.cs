using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Maxwell texture descriptor, as stored on the GPU texture pool memory region.
    /// </summary>
    struct TextureDescriptor : ITextureDescriptor
    {
#pragma warning disable CS0649
        public uint Word0;
        public uint Word1;
        public uint Word2;
        public uint Word3;
        public uint Word4;
        public uint Word5;
        public uint Word6;
        public uint Word7;
#pragma warning restore CS0649

        /// <summary>
        /// Unpacks Maxwell texture format integer.
        /// </summary>
        /// <returns>The texture format integer</returns>
        public uint UnpackFormat()
        {
            return Word0 & 0x8007ffff;
        }

        /// <summary>
        /// Unpacks the swizzle component for the texture red color channel.
        /// </summary>
        /// <returns>The swizzle component</returns>
        public TextureComponent UnpackSwizzleR()
        {
            return(TextureComponent)((Word0 >> 19) & 7);
        }

        /// <summary>
        /// Unpacks the swizzle component for the texture green color channel.
        /// </summary>
        /// <returns>The swizzle component</returns>
        public TextureComponent UnpackSwizzleG()
        {
            return(TextureComponent)((Word0 >> 22) & 7);
        }

        /// <summary>
        /// Unpacks the swizzle component for the texture blue color channel.
        /// </summary>
        /// <returns>The swizzle component</returns>
        public TextureComponent UnpackSwizzleB()
        {
            return(TextureComponent)((Word0 >> 25) & 7);
        }

        /// <summary>
        /// Unpacks the swizzle component for the texture alpha color channel.
        /// </summary>
        /// <returns>The swizzle component</returns>
        public TextureComponent UnpackSwizzleA()
        {
            return(TextureComponent)((Word0 >> 28) & 7);
        }

        /// <summary>
        /// Unpacks the 40-bits texture GPU virtual address.
        /// </summary>
        /// <returns>The GPU virtual address</returns>
        public ulong UnpackAddress()
        {
            return Word1 | ((ulong)(Word2 & 0xffff) << 32);
        }

        /// <summary>
        /// Unpacks texture descriptor type for this texture descriptor.
        /// This defines the texture layout, among other things.
        /// </summary>
        /// <returns>The texture descriptor type</returns>
        public TextureDescriptorType UnpackTextureDescriptorType()
        {
            return (TextureDescriptorType)((Word2 >> 21) & 7);
        }

        /// <summary>
        /// Unpacks the texture stride (bytes per line) for linear textures only.
        /// Always 32-bytes aligned.
        /// </summary>
        /// <returns>The linear texture stride</returns>
        public int UnpackStride()
        {
            return (int)(Word3 & 0xffff) << 5;
        }

        /// <summary>
        /// Unpacks the GOB block size in X (width) for block linear textures.
        /// Must be always 1, ignored by the GPU.
        /// </summary>
        /// <returns>THe GOB block X size</returns>
        public int UnpackGobBlocksInX()
        {
            return 1 << (int)(Word3 & 7);
        }

        /// <summary>
        /// Unpacks the GOB block size in Y (height) for block linear textures.
        /// Must be always a power of 2, with a maximum value of 32.
        /// </summary>
        /// <returns>THe GOB block Y size</returns>
        public int UnpackGobBlocksInY()
        {
            return 1 << (int)((Word3 >> 3) & 7);
        }

        /// <summary>
        /// Unpacks the GOB block size in Z (depth) for block linear textures.
        /// Must be always a power of 2, with a maximum value of 32.
        /// Must be 1 for any texture target other than 3D textures.
        /// </summary>
        /// <returns>The GOB block Z size</returns>
        public int UnpackGobBlocksInZ()
        {
            return 1 << (int)((Word3 >> 6) & 7);
        }

        /// <summary>
        /// Number of GOB blocks per tile in the X direction.
        /// This is only used for sparse textures, should be 1 otherwise.
        /// </summary>
        /// <returns>The number of GOB blocks per tile</returns>
        public int UnpackGobBlocksInTileX()
        {
            return 1 << (int)((Word3 >> 10) & 7);
        }

        /// <summary>
        /// Unpacks the number of mipmap levels of the texture.
        /// </summary>
        /// <returns>The number of mipmap levels</returns>
        public int UnpackLevels()
        {
            return (int)(Word3 >> 28) + 1;
        }

        /// <summary>
        /// Unpack the base level texture width size.
        /// </summary>
        /// <returns>The texture width</returns>
        public int UnpackWidth()
        {
            return (int)(Word4 & 0xffff) + 1;
        }

        /// <summary>
        /// Unpacks the texture sRGB format flag.
        /// </summary>
        /// <returns>True if the texture is sRGB, false otherwise</returns>
        public bool UnpackSrgb()
        {
            return (Word4 & (1 << 22)) != 0;
        }

        /// <summary>
        /// Unpacks the texture target.
        /// </summary>
        /// <returns>The texture target</returns>
        public TextureTarget UnpackTextureTarget()
        {
            return (TextureTarget)((Word4 >> 23) & 0xf);
        }

        /// <summary>
        /// Unpack the base level texture height size, or array layers for 1D array textures.
        /// Should be ignored for 1D or buffer textures.
        /// </summary>
        /// <returns>The texture height or layers count</returns>
        public int UnpackHeight()
        {
            return (int)(Word5 & 0xffff) + 1;
        }

        /// <summary>
        /// Unpack the base level texture depth size, number of array layers or cubemap faces.
        /// The meaning of this value depends on the texture target.
        /// </summary>
        /// <returns>The texture depth, layer or faces count</returns>
        public int UnpackDepth()
        {
            return (int)((Word5 >> 16) & 0x3fff) + 1;
        }

        /// <summary>
        /// Unpacks the texture coordinates normalized flag.
        /// When this is true, texture coordinates are expected to be in the [0, 1] range on the shader.
        /// When this is false, texture coordinates are expected to be in the [0, W], [0, H] and [0, D] range.
        /// It must be set to false (by the guest driver) for rectangle textures.
        /// </summary>
        /// <returns>The texture coordinates normalized flag</returns>
        public bool UnpackTextureCoordNormalized()
        {
            return (Word5 & (1 << 31)) != 0;
        }

        /// <summary>
        /// Unpacks the base mipmap level of the texture.
        /// </summary>
        /// <returns>The base mipmap level of the texture</returns>
        public int UnpackBaseLevel()
        {
            return (int)(Word7 & 0xf);
        }

        /// <summary>
        /// Unpacks the maximum mipmap level (inclusive) of the texture.
        /// Usually equal to Levels minus 1.
        /// </summary>
        /// <returns>The maximum mipmap level (inclusive) of the texture</returns>
        public int UnpackMaxLevelInclusive()
        {
            return (int)((Word7 >> 4) & 0xf);
        }

        /// <summary>
        /// Unpacks the multisampled texture samples count in each direction.
        /// Must be ignored for non-multisample textures.
        /// </summary>
        /// <returns>The multisample counts enum</returns>
        public TextureMsaaMode UnpackTextureMsaaMode()
        {
            return (TextureMsaaMode)((Word7 >> 8) & 0xf);
        }

        /// <summary>
        /// Create the equivalent of this TextureDescriptor for the shader cache.
        /// </summary>
        /// <returns>The equivalent of this TextureDescriptor for the shader cache.</returns>
        public GuestTextureDescriptor ToCache()
        {
            GuestTextureDescriptor result = new GuestTextureDescriptor
            {
                Handle = uint.MaxValue,
                Format = UnpackFormat(),
                Target = UnpackTextureTarget(),
                IsSrgb = UnpackSrgb(),
                IsTextureCoordNormalized = UnpackTextureCoordNormalized(),

            };

            return result;
        }

        /// <summary>
        /// Check if two descriptors are equal.
        /// </summary>
        /// <param name="other">The descriptor to compare against</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public bool Equals(ref TextureDescriptor other)
        {
            return Unsafe.As<TextureDescriptor, Vector256<byte>>(ref this).Equals(Unsafe.As<TextureDescriptor, Vector256<byte>>(ref other));
        }
    }
}
