using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class ShaderProperties
    {
        private readonly Dictionary<int, BufferDefinition> _constantBuffers;
        private readonly Dictionary<int, BufferDefinition> _storageBuffers;

        public IReadOnlyDictionary<int, BufferDefinition> ConstantBuffers => _constantBuffers;
        public IReadOnlyDictionary<int, BufferDefinition> StorageBuffers => _storageBuffers;

        public ShaderProperties()
        {
            _constantBuffers = new Dictionary<int, BufferDefinition>();
            _storageBuffers = new Dictionary<int, BufferDefinition>();
        }

        public void AddConstantBuffer(int binding, BufferDefinition definition)
        {
            _constantBuffers[binding] = definition;
        }

        public void AddStorageBuffer(int binding, BufferDefinition definition)
        {
            _storageBuffers[binding] = definition;
        }
    }
}