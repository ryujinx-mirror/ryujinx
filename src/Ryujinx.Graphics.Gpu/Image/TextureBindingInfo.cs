using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture binding information.
    /// This is used for textures that needs to be accessed from shaders.
    /// </summary>
    readonly struct TextureBindingInfo
    {
        /// <summary>
        /// Shader sampler target type.
        /// </summary>
        public Target Target { get; }

        /// <summary>
        /// For images, indicates the format specified on the shader.
        /// </summary>
        public FormatInfo FormatInfo { get; }

        /// <summary>
        /// Shader texture host set index.
        /// </summary>
        public int Set { get; }

        /// <summary>
        /// Shader texture host binding point.
        /// </summary>
        public int Binding { get; }

        /// <summary>
        /// For array of textures, this indicates the length of the array. A value of one indicates it is not an array.
        /// </summary>
        public int ArrayLength { get; }

        /// <summary>
        /// Constant buffer slot with the texture handle.
        /// </summary>
        public int CbufSlot { get; }

        /// <summary>
        /// Index of the texture handle on the constant buffer at slot <see cref="CbufSlot"/>.
        /// </summary>
        public int Handle { get; }

        /// <summary>
        /// Flags from the texture descriptor that indicate how the texture is used.
        /// </summary>
        public TextureUsageFlags Flags { get; }

        /// <summary>
        /// Indicates that the binding is for a sampler.
        /// </summary>
        public bool IsSamplerOnly { get; }

        /// <summary>
        /// Constructs the texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="formatInfo">Format of the image as declared on the shader</param>
        /// <param name="set">Shader texture host set index</param>
        /// <param name="binding">The shader texture binding point</param>
        /// <param name="arrayLength">For array of textures, this indicates the length of the array. A value of one indicates it is not an array</param>
        /// <param name="cbufSlot">Constant buffer slot where the texture handle is located</param>
        /// <param name="handle">The shader texture handle (read index into the texture constant buffer)</param>
        /// <param name="flags">The texture's usage flags, indicating how it is used in the shader</param>
        public TextureBindingInfo(Target target, FormatInfo formatInfo, int set, int binding, int arrayLength, int cbufSlot, int handle, TextureUsageFlags flags)
        {
            Target = target;
            FormatInfo = formatInfo;
            Set = set;
            Binding = binding;
            ArrayLength = arrayLength;
            CbufSlot = cbufSlot;
            Handle = handle;
            Flags = flags;
        }

        /// <summary>
        /// Constructs the texture binding information structure.
        /// </summary>
        /// <param name="target">The shader sampler target type</param>
        /// <param name="set">Shader texture host set index</param>
        /// <param name="binding">The shader texture binding point</param>
        /// <param name="arrayLength">For array of textures, this indicates the length of the array. A value of one indicates it is not an array</param>
        /// <param name="cbufSlot">Constant buffer slot where the texture handle is located</param>
        /// <param name="handle">The shader texture handle (read index into the texture constant buffer)</param>
        /// <param name="flags">The texture's usage flags, indicating how it is used in the shader</param>
        /// <param name="isSamplerOnly">Indicates that the binding is for a sampler</param>
        public TextureBindingInfo(
            Target target,
            int set,
            int binding,
            int arrayLength,
            int cbufSlot,
            int handle,
            TextureUsageFlags flags,
            bool isSamplerOnly) : this(target, FormatInfo.Invalid, set, binding, arrayLength, cbufSlot, handle, flags)
        {
            IsSamplerOnly = isSamplerOnly;
        }
    }
}
