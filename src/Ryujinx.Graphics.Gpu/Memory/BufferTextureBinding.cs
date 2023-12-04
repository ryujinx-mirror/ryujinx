using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;
using Ryujinx.Memory.Range;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// A buffer binding to apply to a buffer texture.
    /// </summary>
    readonly struct BufferTextureBinding
    {
        /// <summary>
        /// Shader stage accessing the texture.
        /// </summary>
        public ShaderStage Stage { get; }

        /// <summary>
        /// The buffer texture.
        /// </summary>
        public ITexture Texture { get; }

        /// <summary>
        /// Physical ranges of memory where the buffer texture data is located.
        /// </summary>
        public MultiRange Range { get; }

        /// <summary>
        /// The image or sampler binding info for the buffer texture.
        /// </summary>
        public TextureBindingInfo BindingInfo { get; }

        /// <summary>
        /// The image format for the binding.
        /// </summary>
        public Format Format { get; }

        /// <summary>
        /// Whether the binding is for an image or a sampler.
        /// </summary>
        public bool IsImage { get; }

        /// <summary>
        /// Create a new buffer texture binding.
        /// </summary>
        /// <param name="stage">Shader stage accessing the texture</param>
        /// <param name="texture">Buffer texture</param>
        /// <param name="range">Physical ranges of memory where the buffer texture data is located</param>
        /// <param name="bindingInfo">Binding info</param>
        /// <param name="format">Binding format</param>
        /// <param name="isImage">Whether the binding is for an image or a sampler</param>
        public BufferTextureBinding(
            ShaderStage stage,
            ITexture texture,
            MultiRange range,
            TextureBindingInfo bindingInfo,
            Format format,
            bool isImage)
        {
            Stage = stage;
            Texture = texture;
            Range = range;
            BindingInfo = bindingInfo;
            Format = format;
            IsImage = isImage;
        }
    }
}
