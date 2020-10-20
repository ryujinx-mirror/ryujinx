using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture binding information.
    /// This is used for textures that needs to be accessed from shaders.
    /// </summary>
    struct TextureBindingInfo
    {
        /// <summary>
        /// Shader sampler target type.
        /// </summary>
        public Target Target { get; }

        /// <summary>
        /// For images, indicates the format specified on the shader.
        /// </summary>
        public Format Format { get; }

        /// <summary>
        /// Shader texture handle.
        /// This is an index into the texture constant buffer.
        /// </summary>
        public int Handle { get; }

        /// <summary>
        /// Indicates if the texture is a bindless texture.
        /// </summary>
        /// <remarks>
        /// For those textures, Handle is ignored.
        /// </remarks>
        public bool IsBindless { get; }

        /// <summary>
        /// Constant buffer slot with the bindless texture handle, for bindless texture.
        /// </summary>
        public int CbufSlot { get; }

        /// <summary>
        /// Constant buffer offset of the bindless texture handle, for bindless texture.
        /// </summary>
        public int CbufOffset { get; }

        /// <summary>
        /// Flags from the texture descriptor that indicate how the texture is used.
        /// </summary>
        public TextureUsageFlags Flags { get; }

        /// <summary>
        /// Constructs the texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="format">Format of the image as declared on the shader</param>
        /// <param name="handle">The shader texture handle (read index into the texture constant buffer)</param>
        /// <param name="flags">The texture's usage flags, indicating how it is used in the shader</param>
        public TextureBindingInfo(Target target, Format format, int handle, TextureUsageFlags flags)
        {
            Target = target;
            Format = format;
            Handle = handle;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;

            Flags = flags;
        }

        /// <summary>
        /// Constructs the texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="handle">The shader texture handle (read index into the texture constant buffer)</param>
        /// <param name="flags">The texture's usage flags, indicating how it is used in the shader</param>
        public TextureBindingInfo(Target target, int handle, TextureUsageFlags flags) : this(target, (Format)0, handle, flags)
        {
        }

        /// <summary>
        /// Constructs the bindless texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="cbufSlot">Constant buffer slot where the bindless texture handle is located</param>
        /// <param name="cbufOffset">Constant buffer offset of the bindless texture handle</param>
        /// <param name="flags">The texture's usage flags, indicating how it is used in the shader</param>
        public TextureBindingInfo(Target target, int cbufSlot, int cbufOffset, TextureUsageFlags flags)
        {
            Target = target;
            Format = 0;
            Handle = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;

            Flags = flags;
        }
    }
}