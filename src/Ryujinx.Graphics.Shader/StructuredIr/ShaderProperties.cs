using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class ShaderProperties
    {
        private readonly Dictionary<int, BufferDefinition> _constantBuffers;
        private readonly Dictionary<int, BufferDefinition> _storageBuffers;
        private readonly Dictionary<int, MemoryDefinition> _localMemories;
        private readonly Dictionary<int, MemoryDefinition> _sharedMemories;

        public IReadOnlyDictionary<int, BufferDefinition> ConstantBuffers => _constantBuffers;
        public IReadOnlyDictionary<int, BufferDefinition> StorageBuffers => _storageBuffers;
        public IReadOnlyDictionary<int, MemoryDefinition> LocalMemories => _localMemories;
        public IReadOnlyDictionary<int, MemoryDefinition> SharedMemories => _sharedMemories;

        public ShaderProperties()
        {
            _constantBuffers = new Dictionary<int, BufferDefinition>();
            _storageBuffers = new Dictionary<int, BufferDefinition>();
            _localMemories = new Dictionary<int, MemoryDefinition>();
            _sharedMemories = new Dictionary<int, MemoryDefinition>();
        }

        public void AddConstantBuffer(int binding, BufferDefinition definition)
        {
            _constantBuffers[binding] = definition;
        }

        public void AddStorageBuffer(int binding, BufferDefinition definition)
        {
            _storageBuffers[binding] = definition;
        }

        public int AddLocalMemory(MemoryDefinition definition)
        {
            int id = _localMemories.Count;
            _localMemories.Add(id, definition);

            return id;
        }

        public int AddSharedMemory(MemoryDefinition definition)
        {
            int id = _sharedMemories.Count;
            _sharedMemories.Add(id, definition);

            return id;
        }
    }
}
