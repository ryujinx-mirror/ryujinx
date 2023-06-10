using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Ryujinx.Graphics.Shader.Translation
{
    class ShaderConfig
    {
        // TODO: Non-hardcoded array size.
        public const int SamplerArraySize = 4;

        private const int ThreadsPerWarp = 32;

        public ShaderStage Stage { get; }

        public bool GpPassthrough { get; }
        public bool LastInVertexPipeline { get; private set; }

        public bool HasLayerInputAttribute { get; private set; }
        public int GpLayerInputAttribute { get; private set; }
        public int ThreadsPerInputPrimitive { get; }

        public OutputTopology OutputTopology { get; }

        public int MaxOutputVertices { get; }

        public int LocalMemorySize { get; }

        public ImapPixelType[] ImapTypes { get; }

        public int OmapTargets { get; }
        public bool OmapSampleMask { get; }
        public bool OmapDepth { get; }

        public IGpuAccessor GpuAccessor { get; }

        public TranslationOptions Options { get; }

        public ShaderProperties Properties => ResourceManager.Properties;

        public ResourceManager ResourceManager { get; set; }

        public bool TransformFeedbackEnabled { get; }

        private TransformFeedbackOutput[] _transformFeedbackOutputs;

        readonly struct TransformFeedbackVariable : IEquatable<TransformFeedbackVariable>
        {
            public IoVariable IoVariable { get; }
            public int Location { get; }
            public int Component { get; }

            public TransformFeedbackVariable(IoVariable ioVariable, int location = 0, int component = 0)
            {
                IoVariable = ioVariable;
                Location = location;
                Component = component;
            }

            public override bool Equals(object other)
            {
                return other is TransformFeedbackVariable tfbVar && Equals(tfbVar);
            }

            public bool Equals(TransformFeedbackVariable other)
            {
                return IoVariable == other.IoVariable &&
                    Location == other.Location &&
                    Component == other.Component;
            }

            public override int GetHashCode()
            {
                return (int)IoVariable | (Location << 8) | (Component << 16);
            }

            public override string ToString()
            {
                return $"{IoVariable}.{Location}.{Component}";
            }
        }

        private readonly Dictionary<TransformFeedbackVariable, TransformFeedbackOutput> _transformFeedbackDefinitions;

        public int Size { get; private set; }

        public byte ClipDistancesWritten { get; private set; }

        public FeatureFlags UsedFeatures { get; private set; }

        public int Cb1DataSize { get; private set; }

        public bool LayerOutputWritten { get; private set; }
        public int LayerOutputAttribute { get; private set; }

        public bool NextUsesFixedFuncAttributes { get; private set; }
        public int UsedInputAttributes { get; private set; }
        public int UsedOutputAttributes { get; private set; }
        public HashSet<int> UsedInputAttributesPerPatch { get; }
        public HashSet<int> UsedOutputAttributesPerPatch { get; }
        public HashSet<int> NextUsedInputAttributesPerPatch { get; private set; }
        public int PassthroughAttributes { get; private set; }
        private int _nextUsedInputAttributes;
        private int _thisUsedInputAttributes;
        private Dictionary<int, int> _perPatchAttributeLocations;

        public UInt128 NextInputAttributesComponents { get; private set; }
        public UInt128 ThisInputAttributesComponents { get; private set; }

        private readonly record struct TextureInfo(int CbufSlot, int Handle, bool Indexed, TextureFormat Format);

        private struct TextureMeta
        {
            public bool AccurateType;
            public SamplerType Type;
            public TextureUsageFlags UsageFlags;
        }

        private readonly Dictionary<TextureInfo, TextureMeta> _usedTextures;
        private readonly Dictionary<TextureInfo, TextureMeta> _usedImages;
        private TextureDescriptor[] _cachedTextureDescriptors;
        private TextureDescriptor[] _cachedImageDescriptors;

        public ShaderConfig(ShaderStage stage, IGpuAccessor gpuAccessor, TranslationOptions options)
        {
            Stage       = stage;
            GpuAccessor = gpuAccessor;
            Options     = options;

            _transformFeedbackDefinitions = new Dictionary<TransformFeedbackVariable, TransformFeedbackOutput>();

            TransformFeedbackEnabled =
                stage != ShaderStage.Compute &&
                gpuAccessor.QueryTransformFeedbackEnabled() &&
                gpuAccessor.QueryHostSupportsTransformFeedback();

            UsedInputAttributesPerPatch  = new HashSet<int>();
            UsedOutputAttributesPerPatch = new HashSet<int>();

            _usedTextures = new Dictionary<TextureInfo, TextureMeta>();
            _usedImages   = new Dictionary<TextureInfo, TextureMeta>();

            ResourceManager = new ResourceManager(stage, gpuAccessor, new ShaderProperties());

            if (!gpuAccessor.QueryHostSupportsTransformFeedback() && gpuAccessor.QueryTransformFeedbackEnabled())
            {
                StructureType tfeInfoStruct = new StructureType(new StructureField[]
                {
                    new StructureField(AggregateType.Array | AggregateType.U32, "base_offset", 4),
                    new StructureField(AggregateType.U32, "vertex_count")
                });

                BufferDefinition tfeInfoBuffer = new BufferDefinition(BufferLayout.Std430, 1, Constants.TfeInfoBinding, "tfe_info", tfeInfoStruct);

                Properties.AddStorageBuffer(Constants.TfeInfoBinding, tfeInfoBuffer);

                StructureType tfeDataStruct = new StructureType(new StructureField[]
                {
                    new StructureField(AggregateType.Array | AggregateType.U32, "data", 0)
                });

                for (int i = 0; i < Constants.TfeBuffersCount; i++)
                {
                    int binding = Constants.TfeBufferBaseBinding + i;
                    BufferDefinition tfeDataBuffer = new BufferDefinition(BufferLayout.Std430, 1, binding, $"tfe_data{i}", tfeDataStruct);
                    Properties.AddStorageBuffer(binding, tfeDataBuffer);
                }
            }
        }

        public ShaderConfig(
            ShaderStage stage,
            OutputTopology outputTopology,
            int maxOutputVertices,
            IGpuAccessor gpuAccessor,
            TranslationOptions options) : this(stage, gpuAccessor, options)
        {
            ThreadsPerInputPrimitive = 1;
            OutputTopology           = outputTopology;
            MaxOutputVertices        = maxOutputVertices;
        }

        public ShaderConfig(ShaderHeader header, IGpuAccessor gpuAccessor, TranslationOptions options) : this(header.Stage, gpuAccessor, options)
        {
            GpPassthrough            = header.Stage == ShaderStage.Geometry && header.GpPassthrough;
            ThreadsPerInputPrimitive = header.ThreadsPerInputPrimitive;
            OutputTopology           = header.OutputTopology;
            MaxOutputVertices        = header.MaxOutputVertexCount;
            LocalMemorySize          = header.ShaderLocalMemoryLowSize + header.ShaderLocalMemoryHighSize + (header.ShaderLocalMemoryCrsSize / ThreadsPerWarp);
            ImapTypes                = header.ImapTypes;
            OmapTargets              = header.OmapTargets;
            OmapSampleMask           = header.OmapSampleMask;
            OmapDepth                = header.OmapDepth;
            LastInVertexPipeline     = header.Stage < ShaderStage.Fragment;
        }

        private void EnsureTransformFeedbackInitialized()
        {
            if (HasTransformFeedbackOutputs() && _transformFeedbackOutputs == null)
            {
                TransformFeedbackOutput[] transformFeedbackOutputs = new TransformFeedbackOutput[0xc0];
                ulong vecMap = 0UL;

                for (int tfbIndex = 0; tfbIndex < 4; tfbIndex++)
                {
                    var locations = GpuAccessor.QueryTransformFeedbackVaryingLocations(tfbIndex);
                    var stride = GpuAccessor.QueryTransformFeedbackStride(tfbIndex);

                    for (int i = 0; i < locations.Length; i++)
                    {
                        byte wordOffset = locations[i];
                        if (wordOffset < 0xc0)
                        {
                            transformFeedbackOutputs[wordOffset] = new TransformFeedbackOutput(tfbIndex, i * 4, stride);
                            vecMap |= 1UL << (wordOffset / 4);
                        }
                    }
                }

                _transformFeedbackOutputs = transformFeedbackOutputs;

                while (vecMap != 0)
                {
                    int vecIndex = BitOperations.TrailingZeroCount(vecMap);

                    for (int subIndex = 0; subIndex < 4; subIndex++)
                    {
                        int wordOffset = vecIndex * 4 + subIndex;
                        int byteOffset = wordOffset * 4;

                        if (transformFeedbackOutputs[wordOffset].Valid)
                        {
                            IoVariable ioVariable = Instructions.AttributeMap.GetIoVariable(this, byteOffset, out int location);
                            int component = 0;

                            if (HasPerLocationInputOrOutputComponent(ioVariable, location, subIndex, isOutput: true))
                            {
                                component = subIndex;
                            }

                            var transformFeedbackVariable = new TransformFeedbackVariable(ioVariable, location, component);
                            _transformFeedbackDefinitions.TryAdd(transformFeedbackVariable, transformFeedbackOutputs[wordOffset]);
                        }
                    }

                    vecMap &= ~(1UL << vecIndex);
                }
            }
        }

        public TransformFeedbackOutput[] GetTransformFeedbackOutputs()
        {
            EnsureTransformFeedbackInitialized();
            return _transformFeedbackOutputs;
        }

        public bool TryGetTransformFeedbackOutput(IoVariable ioVariable, int location, int component, out TransformFeedbackOutput transformFeedbackOutput)
        {
            EnsureTransformFeedbackInitialized();
            var transformFeedbackVariable = new TransformFeedbackVariable(ioVariable, location, component);
            return _transformFeedbackDefinitions.TryGetValue(transformFeedbackVariable, out transformFeedbackOutput);
        }

        private bool HasTransformFeedbackOutputs()
        {
            return TransformFeedbackEnabled && (LastInVertexPipeline || Stage == ShaderStage.Fragment);
        }

        public bool HasTransformFeedbackOutputs(bool isOutput)
        {
            return TransformFeedbackEnabled && ((isOutput && LastInVertexPipeline) || (!isOutput && Stage == ShaderStage.Fragment));
        }

        public bool HasPerLocationInputOrOutput(IoVariable ioVariable, bool isOutput)
        {
            if (ioVariable == IoVariable.UserDefined)
            {
                return (!isOutput && !UsedFeatures.HasFlag(FeatureFlags.IaIndexing)) ||
                       (isOutput && !UsedFeatures.HasFlag(FeatureFlags.OaIndexing));
            }

            return ioVariable == IoVariable.FragmentOutputColor;
        }

        public bool HasPerLocationInputOrOutputComponent(IoVariable ioVariable, int location, int component, bool isOutput)
        {
            if (ioVariable != IoVariable.UserDefined || !HasTransformFeedbackOutputs(isOutput))
            {
                return false;
            }

            return GetTransformFeedbackOutputComponents(location, component) == 1;
        }

        public TransformFeedbackOutput GetTransformFeedbackOutput(int wordOffset)
        {
            EnsureTransformFeedbackInitialized();

            return _transformFeedbackOutputs[wordOffset];
        }

        public TransformFeedbackOutput GetTransformFeedbackOutput(int location, int component)
        {
            return GetTransformFeedbackOutput((AttributeConsts.UserAttributeBase / 4) + location * 4 + component);
        }

        public int GetTransformFeedbackOutputComponents(int location, int component)
        {
            EnsureTransformFeedbackInitialized();

            int baseIndex = (AttributeConsts.UserAttributeBase / 4) + location * 4;
            int index = baseIndex + component;
            int count = 1;

            for (; count < 4; count++)
            {
                ref var prev = ref _transformFeedbackOutputs[baseIndex + count - 1];
                ref var curr = ref _transformFeedbackOutputs[baseIndex + count];

                int prevOffset = prev.Offset;
                int currOffset = curr.Offset;

                if (!prev.Valid || !curr.Valid || prevOffset + 4 != currOffset)
                {
                    break;
                }
            }

            if (baseIndex + count <= index)
            {
                return 1;
            }

            return count;
        }

        public AggregateType GetFragmentOutputColorType(int location)
        {
            return AggregateType.Vector4 | GpuAccessor.QueryFragmentOutputType(location).ToAggregateType();
        }

        public AggregateType GetUserDefinedType(int location, bool isOutput)
        {
            if ((!isOutput && UsedFeatures.HasFlag(FeatureFlags.IaIndexing)) ||
                (isOutput && UsedFeatures.HasFlag(FeatureFlags.OaIndexing)))
            {
                return AggregateType.Array | AggregateType.Vector4 | AggregateType.FP32;
            }

            AggregateType type = AggregateType.Vector4;

            if (Stage == ShaderStage.Vertex && !isOutput)
            {
                type |= GpuAccessor.QueryAttributeType(location).ToAggregateType();
            }
            else
            {
                type |= AggregateType.FP32;
            }

            return type;
        }

        public int GetDepthRegister()
        {
            // The depth register is always two registers after the last color output.
            return BitOperations.PopCount((uint)OmapTargets) + 1;
        }

        public uint ConstantBuffer1Read(int offset)
        {
            if (Cb1DataSize < offset + 4)
            {
                Cb1DataSize = offset + 4;
            }

            return GpuAccessor.ConstantBuffer1Read(offset);
        }

        public TextureFormat GetTextureFormat(int handle, int cbufSlot = -1)
        {
            // When the formatted load extension is supported, we don't need to
            // specify a format, we can just declare it without a format and the GPU will handle it.
            if (GpuAccessor.QueryHostSupportsImageLoadFormatted())
            {
                return TextureFormat.Unknown;
            }

            var format = GpuAccessor.QueryTextureFormat(handle, cbufSlot);

            if (format == TextureFormat.Unknown)
            {
                GpuAccessor.Log($"Unknown format for texture {handle}.");

                format = TextureFormat.R8G8B8A8Unorm;
            }

            return format;
        }

        private static bool FormatSupportsAtomic(TextureFormat format)
        {
            return format == TextureFormat.R32Sint || format == TextureFormat.R32Uint;
        }

        public TextureFormat GetTextureFormatAtomic(int handle, int cbufSlot = -1)
        {
            // Atomic image instructions do not support GL_EXT_shader_image_load_formatted,
            // and must have a type specified. Default to R32Sint if not available.

            var format = GpuAccessor.QueryTextureFormat(handle, cbufSlot);

            if (!FormatSupportsAtomic(format))
            {
                GpuAccessor.Log($"Unsupported format for texture {handle}: {format}.");

                format = TextureFormat.R32Sint;
            }

            return format;
        }

        public void SizeAdd(int size)
        {
            Size += size;
        }

        public void InheritFrom(ShaderConfig other)
        {
            ClipDistancesWritten |= other.ClipDistancesWritten;
            UsedFeatures |= other.UsedFeatures;

            UsedInputAttributes |= other.UsedInputAttributes;
            UsedOutputAttributes |= other.UsedOutputAttributes;

            foreach (var kv in other._usedTextures)
            {
                if (!_usedTextures.TryAdd(kv.Key, kv.Value))
                {
                    _usedTextures[kv.Key] = MergeTextureMeta(kv.Value, _usedTextures[kv.Key]);
                }
            }

            foreach (var kv in other._usedImages)
            {
                if (!_usedImages.TryAdd(kv.Key, kv.Value))
                {
                    _usedImages[kv.Key] = MergeTextureMeta(kv.Value, _usedImages[kv.Key]);
                }
            }
        }

        public void SetLayerOutputAttribute(int attr)
        {
            LayerOutputWritten = true;
            LayerOutputAttribute = attr;
        }

        public void SetGeometryShaderLayerInputAttribute(int attr)
        {
            HasLayerInputAttribute = true;
            GpLayerInputAttribute = attr;
        }

        public void SetLastInVertexPipeline()
        {
            LastInVertexPipeline = true;
        }

        public void SetInputUserAttributeFixedFunc(int index)
        {
            UsedInputAttributes |= 1 << index;
        }

        public void SetOutputUserAttributeFixedFunc(int index)
        {
            UsedOutputAttributes |= 1 << index;
        }

        public void SetInputUserAttribute(int index, int component)
        {
            int mask = 1 << index;

            UsedInputAttributes |= mask;
            _thisUsedInputAttributes |= mask;
            ThisInputAttributesComponents |= UInt128.One << (index * 4 + component);
        }

        public void SetInputUserAttributePerPatch(int index)
        {
            UsedInputAttributesPerPatch.Add(index);
        }

        public void SetOutputUserAttribute(int index)
        {
            UsedOutputAttributes |= 1 << index;
        }

        public void SetOutputUserAttributePerPatch(int index)
        {
            UsedOutputAttributesPerPatch.Add(index);
        }

        public void MergeFromtNextStage(ShaderConfig config)
        {
            NextInputAttributesComponents = config.ThisInputAttributesComponents;
            NextUsedInputAttributesPerPatch = config.UsedInputAttributesPerPatch;
            NextUsesFixedFuncAttributes = config.UsedFeatures.HasFlag(FeatureFlags.FixedFuncAttr);
            MergeOutputUserAttributes(config.UsedInputAttributes, config.UsedInputAttributesPerPatch);

            if (UsedOutputAttributesPerPatch.Count != 0)
            {
                // Regular and per-patch input/output locations can't overlap,
                // so we must assign on our location using unused regular input/output locations.

                Dictionary<int, int> locationsMap = new Dictionary<int, int>();

                int freeMask = ~UsedOutputAttributes;

                foreach (int attr in UsedOutputAttributesPerPatch)
                {
                    int location = BitOperations.TrailingZeroCount(freeMask);
                    if (location == 32)
                    {
                        config.GpuAccessor.Log($"No enough free locations for patch input/output 0x{attr:X}.");
                        break;
                    }

                    locationsMap.Add(attr, location);
                    freeMask &= ~(1 << location);
                }

                // Both stages must agree on the locations, so use the same "map" for both.
                _perPatchAttributeLocations = locationsMap;
                config._perPatchAttributeLocations = locationsMap;
            }

            // We don't consider geometry shaders using the geometry shader passthrough feature
            // as being the last because when this feature is used, it can't actually modify any of the outputs,
            // so the stage that comes before it is the last one that can do modifications.
            if (config.Stage != ShaderStage.Fragment && (config.Stage != ShaderStage.Geometry || !config.GpPassthrough))
            {
                LastInVertexPipeline = false;
            }
        }

        public void MergeOutputUserAttributes(int mask, IEnumerable<int> perPatch)
        {
            _nextUsedInputAttributes = mask;

            if (GpPassthrough)
            {
                PassthroughAttributes = mask & ~UsedOutputAttributes;
            }
            else
            {
                UsedOutputAttributes |= mask;
                UsedOutputAttributesPerPatch.UnionWith(perPatch);
            }
        }

        public int GetPerPatchAttributeLocation(int index)
        {
            if (_perPatchAttributeLocations == null || !_perPatchAttributeLocations.TryGetValue(index, out int location))
            {
                return index;
            }

            return location;
        }

        public bool IsUsedOutputAttribute(int attr)
        {
            // The check for fixed function attributes on the next stage is conservative,
            // returning false if the output is just not used by the next stage is also valid.
            if (NextUsesFixedFuncAttributes &&
                attr >= AttributeConsts.UserAttributeBase &&
                attr < AttributeConsts.UserAttributeEnd)
            {
                int index = (attr - AttributeConsts.UserAttributeBase) >> 4;
                return (_nextUsedInputAttributes & (1 << index)) != 0;
            }

            return true;
        }

        public int GetFreeUserAttribute(bool isOutput, int index)
        {
            int useMask = isOutput ? _nextUsedInputAttributes : _thisUsedInputAttributes;
            int bit = -1;

            while (useMask != -1)
            {
                bit = BitOperations.TrailingZeroCount(~useMask);

                if (bit == 32)
                {
                    bit = -1;
                    break;
                }
                else if (index < 1)
                {
                    break;
                }

                useMask |= 1 << bit;
                index--;
            }

            return bit;
        }

        public void SetAllInputUserAttributes()
        {
            UsedInputAttributes |= Constants.AllAttributesMask;
            ThisInputAttributesComponents |= ~UInt128.Zero >> (128 - Constants.MaxAttributes * 4);
        }

        public void SetAllOutputUserAttributes()
        {
            UsedOutputAttributes |= Constants.AllAttributesMask;
        }

        public void SetClipDistanceWritten(int index)
        {
            ClipDistancesWritten |= (byte)(1 << index);
        }

        public void SetUsedFeature(FeatureFlags flags)
        {
            UsedFeatures |= flags;
        }

        public void SetUsedTexture(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int cbufSlot,
            int handle)
        {
            inst &= Instruction.Mask;
            bool isImage = inst == Instruction.ImageLoad || inst == Instruction.ImageStore || inst == Instruction.ImageAtomic;
            bool isWrite = inst == Instruction.ImageStore || inst == Instruction.ImageAtomic;
            bool accurateType = inst != Instruction.Lod && inst != Instruction.TextureSize;
            bool coherent = flags.HasFlag(TextureFlags.Coherent);

            if (isImage)
            {
                SetUsedTextureOrImage(_usedImages, cbufSlot, handle, type, format, true, isWrite, false, coherent);
            }
            else
            {
                bool intCoords = flags.HasFlag(TextureFlags.IntCoords) || inst == Instruction.TextureSize;
                SetUsedTextureOrImage(_usedTextures, cbufSlot, handle, type, TextureFormat.Unknown, intCoords, false, accurateType, coherent);
            }

            GpuAccessor.RegisterTexture(handle, cbufSlot);
        }

        private void SetUsedTextureOrImage(
            Dictionary<TextureInfo, TextureMeta> dict,
            int cbufSlot,
            int handle,
            SamplerType type,
            TextureFormat format,
            bool intCoords,
            bool write,
            bool accurateType,
            bool coherent)
        {
            var dimensions = type.GetDimensions();
            var isIndexed = type.HasFlag(SamplerType.Indexed);

            var usageFlags = TextureUsageFlags.None;

            if (intCoords)
            {
                usageFlags |= TextureUsageFlags.NeedsScaleValue;

                var canScale = Stage.SupportsRenderScale() && !isIndexed && !write && dimensions == 2;

                if (!canScale)
                {
                    // Resolution scaling cannot be applied to this texture right now.
                    // Flag so that we know to blacklist scaling on related textures when binding them.
                    usageFlags |= TextureUsageFlags.ResScaleUnsupported;
                }
            }

            if (write)
            {
                usageFlags |= TextureUsageFlags.ImageStore;
            }

            if (coherent)
            {
                usageFlags |= TextureUsageFlags.ImageCoherent;
            }

            int arraySize = isIndexed ? SamplerArraySize : 1;

            for (int layer = 0; layer < arraySize; layer++)
            {
                var info = new TextureInfo(cbufSlot, handle + layer * 2, isIndexed, format);
                var meta = new TextureMeta()
                {
                    AccurateType = accurateType,
                    Type = type,
                    UsageFlags = usageFlags
                };

                if (dict.TryGetValue(info, out var existingMeta))
                {
                    dict[info] = MergeTextureMeta(meta, existingMeta);
                }
                else
                {
                    dict.Add(info, meta);
                }
            }
        }

        private static TextureMeta MergeTextureMeta(TextureMeta meta, TextureMeta existingMeta)
        {
            meta.UsageFlags |= existingMeta.UsageFlags;

            // If the texture we have has inaccurate type information, then
            // we prefer the most accurate one.
            if (existingMeta.AccurateType)
            {
                meta.AccurateType = true;
                meta.Type = existingMeta.Type;
            }

            return meta;
        }

        public TextureDescriptor[] GetTextureDescriptors()
        {
            return _cachedTextureDescriptors ??= GetTextureOrImageDescriptors(_usedTextures, GpuAccessor.QueryBindingTexture);
        }

        public TextureDescriptor[] GetImageDescriptors()
        {
            return _cachedImageDescriptors ??= GetTextureOrImageDescriptors(_usedImages, GpuAccessor.QueryBindingImage);
        }

        private static TextureDescriptor[] GetTextureOrImageDescriptors(Dictionary<TextureInfo, TextureMeta> dict, Func<int, bool, int> getBindingCallback)
        {
            var descriptors = new TextureDescriptor[dict.Count];

            int i = 0;
            foreach (var kv in dict.OrderBy(x => x.Key.Indexed).ThenBy(x => x.Key.Handle))
            {
                var info = kv.Key;
                var meta = kv.Value;

                bool isBuffer = (meta.Type & SamplerType.Mask) == SamplerType.TextureBuffer;
                int binding = getBindingCallback(i, isBuffer);

                descriptors[i] = new TextureDescriptor(binding, meta.Type, info.Format, info.CbufSlot, info.Handle);
                descriptors[i].SetFlag(meta.UsageFlags);
                i++;
            }

            return descriptors;
        }

        public TextureDescriptor FindTextureDescriptor(AstTextureOperation texOp)
        {
            TextureDescriptor[] descriptors = GetTextureDescriptors();

            for (int i = 0; i < descriptors.Length; i++)
            {
                var descriptor = descriptors[i];

                if (descriptor.CbufSlot == texOp.CbufSlot &&
                    descriptor.HandleIndex == texOp.Handle &&
                    descriptor.Format == texOp.Format)
                {
                    return descriptor;
                }
            }

            return default;
        }

        private static int FindDescriptorIndex(TextureDescriptor[] array, AstTextureOperation texOp)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var descriptor = array[i];

                if (descriptor.Type == texOp.Type &&
                    descriptor.CbufSlot == texOp.CbufSlot &&
                    descriptor.HandleIndex == texOp.Handle &&
                    descriptor.Format == texOp.Format)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindDescriptorIndex(TextureDescriptor[] array, TextureOperation texOp, bool ignoreType = false)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var descriptor = array[i];

                if ((descriptor.Type == texOp.Type || ignoreType) &&
                    descriptor.CbufSlot == texOp.CbufSlot &&
                    descriptor.HandleIndex == texOp.Handle &&
                    descriptor.Format == texOp.Format)
                {
                    return i;
                }
            }

            return -1;
        }

        public int FindTextureDescriptorIndex(TextureOperation texOp, bool ignoreType = false)
        {
            return FindDescriptorIndex(GetTextureDescriptors(), texOp, ignoreType);
        }

        public int FindImageDescriptorIndex(TextureOperation texOp)
        {
            return FindDescriptorIndex(GetImageDescriptors(), texOp);
        }

        public ShaderProgramInfo CreateProgramInfo(ShaderIdentification identification = ShaderIdentification.None)
        {
            return new ShaderProgramInfo(
                ResourceManager.GetConstantBufferDescriptors(),
                ResourceManager.GetStorageBufferDescriptors(),
                GetTextureDescriptors(),
                GetImageDescriptors(),
                identification,
                GpLayerInputAttribute,
                Stage,
                UsedFeatures.HasFlag(FeatureFlags.InstanceId),
                UsedFeatures.HasFlag(FeatureFlags.DrawParameters),
                UsedFeatures.HasFlag(FeatureFlags.RtLayer),
                ClipDistancesWritten,
                OmapTargets);
        }
    }
}
