using Ryujinx.Graphics.Shader.StructuredIr;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ryujinx.Graphics.Shader.Translation
{
    class ResourceManager
    {
        private static readonly string[] _stagePrefixes = new string[] { "cp", "vp", "tcp", "tep", "gp", "fp" };

        private readonly IGpuAccessor _gpuAccessor;
        private readonly ShaderProperties _properties;
        private readonly string _stagePrefix;

        private readonly int[] _cbSlotToBindingMap;

        private readonly HashSet<int> _usedConstantBufferBindings;

        public ShaderProperties Properties => _properties;

        public ResourceManager(ShaderStage stage, IGpuAccessor gpuAccessor, ShaderProperties properties)
        {
            _gpuAccessor = gpuAccessor;
            _properties = properties;
            _stagePrefix = GetShaderStagePrefix(stage);

            _cbSlotToBindingMap = new int[18];
            _cbSlotToBindingMap.AsSpan().Fill(-1);

            _usedConstantBufferBindings = new HashSet<int>();

            properties.AddConstantBuffer(0, new BufferDefinition(BufferLayout.Std140, 0, 0, "support_buffer", SupportBuffer.GetStructureType()));
        }

        public int GetConstantBufferBinding(int slot)
        {
            int binding = _cbSlotToBindingMap[slot];
            if (binding < 0)
            {
                binding = _gpuAccessor.QueryBindingConstantBuffer(slot);
                _cbSlotToBindingMap[slot] = binding;
                string slotNumber = slot.ToString(CultureInfo.InvariantCulture);
                AddNewConstantBuffer(binding, $"{_stagePrefix}_c{slotNumber}");
            }

            return binding;
        }

        public bool TryGetConstantBufferSlot(int binding, out int slot)
        {
            for (slot = 0; slot < _cbSlotToBindingMap.Length; slot++)
            {
                if (_cbSlotToBindingMap[slot] == binding)
                {
                    return true;
                }
            }

            slot = 0;
            return false;
        }

        public void SetUsedConstantBufferBinding(int binding)
        {
            _usedConstantBufferBindings.Add(binding);
        }

        public BufferDescriptor[] GetConstantBufferDescriptors()
        {
            var descriptors = new BufferDescriptor[_usedConstantBufferBindings.Count];

            int descriptorIndex = 0;

            for (int slot = 0; slot < _cbSlotToBindingMap.Length; slot++)
            {
                int binding = _cbSlotToBindingMap[slot];

                if (binding >= 0 && _usedConstantBufferBindings.Contains(binding))
                {
                    descriptors[descriptorIndex++] = new BufferDescriptor(binding, slot);
                }
            }

            if (descriptors.Length != descriptorIndex)
            {
                Array.Resize(ref descriptors, descriptorIndex);
            }

            return descriptors;
        }

        private void AddNewConstantBuffer(int binding, string name)
        {
            StructureType type = new StructureType(new[]
            {
                new StructureField(AggregateType.Array | AggregateType.Vector4 | AggregateType.FP32, "data", Constants.ConstantBufferSize / 16)
            });

            _properties.AddConstantBuffer(binding, new BufferDefinition(BufferLayout.Std140, 0, binding, name, type));
        }

        public static string GetShaderStagePrefix(ShaderStage stage)
        {
            uint index = (uint)stage;

            if (index >= _stagePrefixes.Length)
            {
                return "invalid";
            }

            return _stagePrefixes[index];
        }
    }
}