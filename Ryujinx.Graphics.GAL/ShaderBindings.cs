using System.Collections.Generic;

namespace Ryujinx.Graphics.GAL
{
    public struct ShaderBindings
    {
        public IReadOnlyCollection<int> UniformBufferBindings { get; }
        public IReadOnlyCollection<int> StorageBufferBindings { get; }
        public IReadOnlyCollection<int> TextureBindings { get; }
        public IReadOnlyCollection<int> ImageBindings { get; }

        public ShaderBindings(
            IReadOnlyCollection<int> uniformBufferBindings,
            IReadOnlyCollection<int> storageBufferBindings,
            IReadOnlyCollection<int> textureBindings,
            IReadOnlyCollection<int> imageBindings)
        {
            UniformBufferBindings = uniformBufferBindings;
            StorageBufferBindings = storageBufferBindings;
            TextureBindings = textureBindings;
            ImageBindings = imageBindings;
        }
    }
}
