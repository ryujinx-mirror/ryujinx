using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class ShaderProperties
    {
        private readonly Dictionary<int, BufferDefinition> _constantBuffers;

        public IReadOnlyDictionary<int, BufferDefinition> ConstantBuffers => _constantBuffers;

        public ShaderProperties()
        {
            _constantBuffers = new Dictionary<int, BufferDefinition>();
        }

        public void AddConstantBuffer(int binding, BufferDefinition definition)
        {
            _constantBuffers[binding] = definition;
        }
    }
}