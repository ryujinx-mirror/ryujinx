using Ryujinx.Graphics.Shader.IntermediateRepresentation;
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

        public ShaderStage Stage { get; }

        public bool GpPassthrough { get; }

        public int ThreadsPerInputPrimitive { get; }

        public OutputTopology OutputTopology { get; }

        public int MaxOutputVertices { get; }

        public int LocalMemorySize { get; }

        public ImapPixelType[] ImapTypes { get; }

        public OmapTarget[] OmapTargets    { get; }
        public bool         OmapSampleMask { get; }
        public bool         OmapDepth      { get; }

        public IGpuAccessor GpuAccessor { get; }

        public TranslationOptions Options { get; }

        public int Size { get; private set; }

        public byte ClipDistancesWritten { get; private set; }

        public FeatureFlags UsedFeatures { get; private set; }

        public HashSet<int> TextureHandlesForCache { get; }

        private readonly TranslationCounts _counts;

        public int UsedInputAttributes { get; private set; }
        public int UsedInputAttributesPerPatch { get; private set; }
        public int UsedOutputAttributes { get; private set; }
        public int UsedOutputAttributesPerPatch { get; private set; }
        public int PassthroughAttributes { get; private set; }

        private int _usedConstantBuffers;
        private int _usedStorageBuffers;
        private int _usedStorageBuffersWrite;

        private struct TextureInfo : IEquatable<TextureInfo>
        {
            public int CbufSlot { get; }
            public int Handle { get; }
            public bool Indexed { get; }
            public TextureFormat Format { get; }

            public TextureInfo(int cbufSlot, int handle, bool indexed, TextureFormat format)
            {
                CbufSlot = cbufSlot;
                Handle = handle;
                Indexed = indexed;
                Format = format;
            }

            public override bool Equals(object obj)
            {
                return obj is TextureInfo other && Equals(other);
            }

            public bool Equals(TextureInfo other)
            {
                return CbufSlot == other.CbufSlot && Handle == other.Handle && Indexed == other.Indexed && Format == other.Format;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(CbufSlot, Handle, Indexed, Format);
            }
        }

        private struct TextureMeta
        {
            public bool AccurateType;
            public SamplerType Type;
            public TextureUsageFlags UsageFlags;
        }

        private readonly Dictionary<TextureInfo, TextureMeta> _usedTextures;
        private readonly Dictionary<TextureInfo, TextureMeta> _usedImages;

        private BufferDescriptor[] _cachedConstantBufferDescriptors;
        private BufferDescriptor[] _cachedStorageBufferDescriptors;
        private TextureDescriptor[] _cachedTextureDescriptors;
        private TextureDescriptor[] _cachedImageDescriptors;

        public int FirstConstantBufferBinding { get; private set; }
        public int FirstStorageBufferBinding { get; private set; }

        public ShaderConfig(IGpuAccessor gpuAccessor, TranslationOptions options, TranslationCounts counts)
        {
            Stage                  = ShaderStage.Compute;
            GpuAccessor            = gpuAccessor;
            Options                = options;
            _counts                = counts;
            TextureHandlesForCache = new HashSet<int>();
            _usedTextures          = new Dictionary<TextureInfo, TextureMeta>();
            _usedImages            = new Dictionary<TextureInfo, TextureMeta>();
        }

        public ShaderConfig(ShaderHeader header, IGpuAccessor gpuAccessor, TranslationOptions options, TranslationCounts counts) : this(gpuAccessor, options, counts)
        {
            Stage                    = header.Stage;
            GpPassthrough            = header.Stage == ShaderStage.Geometry && header.GpPassthrough;
            ThreadsPerInputPrimitive = header.ThreadsPerInputPrimitive;
            OutputTopology           = header.OutputTopology;
            MaxOutputVertices        = header.MaxOutputVertexCount;
            LocalMemorySize          = header.ShaderLocalMemoryLowSize + header.ShaderLocalMemoryHighSize;
            ImapTypes                = header.ImapTypes;
            OmapTargets              = header.OmapTargets;
            OmapSampleMask           = header.OmapSampleMask;
            OmapDepth                = header.OmapDepth;
        }

        public int GetDepthRegister()
        {
            int count = 0;

            for (int index = 0; index < OmapTargets.Length; index++)
            {
                for (int component = 0; component < 4; component++)
                {
                    if (OmapTargets[index].ComponentEnabled(component))
                    {
                        count++;
                    }
                }
            }

            // The depth register is always two registers after the last color output.
            return count + 1;
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

        private bool FormatSupportsAtomic(TextureFormat format)
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

            TextureHandlesForCache.UnionWith(other.TextureHandlesForCache);

            UsedInputAttributes |= other.UsedInputAttributes;
            UsedOutputAttributes |= other.UsedOutputAttributes;
            _usedConstantBuffers |= other._usedConstantBuffers;
            _usedStorageBuffers |= other._usedStorageBuffers;
            _usedStorageBuffersWrite |= other._usedStorageBuffersWrite;

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

        public void SetInputUserAttribute(int index, bool perPatch)
        {
            if (perPatch)
            {
                UsedInputAttributesPerPatch |= 1 << index;
            }
            else
            {
                UsedInputAttributes |= 1 << index;
            }
        }

        public void SetOutputUserAttribute(int index, bool perPatch)
        {
            if (perPatch)
            {
                UsedOutputAttributesPerPatch |= 1 << index;
            }
            else
            {
                UsedOutputAttributes |= 1 << index;
            }
        }

        public void MergeOutputUserAttributes(int mask, int maskPerPatch)
        {
            if (GpPassthrough)
            {
                PassthroughAttributes = mask & ~UsedOutputAttributes;
            }
            else
            {
                UsedOutputAttributes |= mask;
                UsedOutputAttributesPerPatch |= maskPerPatch;
            }
        }

        public void SetAllInputUserAttributes()
        {
            UsedInputAttributes |= Constants.AllAttributesMask;
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

        public Operand CreateCbuf(int slot, int offset)
        {
            SetUsedConstantBuffer(slot);
            return OperandHelper.Cbuf(slot, offset);
        }

        public void SetUsedConstantBuffer(int slot)
        {
            _usedConstantBuffers |= 1 << slot;
        }

        public void SetUsedStorageBuffer(int slot, bool write)
        {
            int mask = 1 << slot;
            _usedStorageBuffers |= mask;

            if (write)
            {
                _usedStorageBuffersWrite |= mask;
            }
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
            bool accurateType = inst != Instruction.Lod;

            if (isImage)
            {
                SetUsedTextureOrImage(_usedImages, cbufSlot, handle, type, format, true, isWrite, false);
            }
            else
            {
                bool intCoords = flags.HasFlag(TextureFlags.IntCoords) || inst == Instruction.TextureSize;
                SetUsedTextureOrImage(_usedTextures, cbufSlot, handle, type, TextureFormat.Unknown, intCoords, false, accurateType);
            }
        }

        private void SetUsedTextureOrImage(
            Dictionary<TextureInfo, TextureMeta> dict,
            int cbufSlot,
            int handle,
            SamplerType type,
            TextureFormat format,
            bool intCoords,
            bool write,
            bool accurateType)
        {
            var dimensions = type.GetDimensions();
            var isIndexed = type.HasFlag(SamplerType.Indexed);

            var usageFlags = TextureUsageFlags.None;

            if (intCoords)
            {
                usageFlags |= TextureUsageFlags.NeedsScaleValue;

                var canScale = (Stage == ShaderStage.Fragment || Stage == ShaderStage.Compute) && !isIndexed && !write && dimensions == 2;

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

        public BufferDescriptor[] GetConstantBufferDescriptors()
        {
            if (_cachedConstantBufferDescriptors != null)
            {
                return _cachedConstantBufferDescriptors;
            }

            int usedMask = _usedConstantBuffers;

            if (UsedFeatures.HasFlag(FeatureFlags.CbIndexing))
            {
                usedMask |= (int)GpuAccessor.QueryConstantBufferUse();
            }

            FirstConstantBufferBinding = _counts.UniformBuffersCount;

            return _cachedConstantBufferDescriptors = GetBufferDescriptors(
                usedMask,
                0,
                UsedFeatures.HasFlag(FeatureFlags.CbIndexing),
                _counts.IncrementUniformBuffersCount);
        }

        public BufferDescriptor[] GetStorageBufferDescriptors()
        {
            if (_cachedStorageBufferDescriptors != null)
            {
                return _cachedStorageBufferDescriptors;
            }

            FirstStorageBufferBinding = _counts.StorageBuffersCount;

            return _cachedStorageBufferDescriptors = GetBufferDescriptors(
                _usedStorageBuffers,
                _usedStorageBuffersWrite,
                true,
                _counts.IncrementStorageBuffersCount);
        }

        private static BufferDescriptor[] GetBufferDescriptors(
            int usedMask,
            int writtenMask,
            bool isArray,
            Func<int> getBindingCallback)
        {
            var descriptors = new BufferDescriptor[BitOperations.PopCount((uint)usedMask)];

            int lastSlot = -1;

            for (int i = 0; i < descriptors.Length; i++)
            {
                int slot = BitOperations.TrailingZeroCount(usedMask);

                if (isArray)
                {
                    // The next array entries also consumes bindings, even if they are unused.
                    for (int j = lastSlot + 1; j < slot; j++)
                    {
                        getBindingCallback();
                    }
                }

                lastSlot = slot;

                descriptors[i] = new BufferDescriptor(getBindingCallback(), slot);

                if ((writtenMask & (1 << slot)) != 0)
                {
                    descriptors[i].SetFlag(BufferUsageFlags.Write);
                }

                usedMask &= ~(1 << slot);
            }

            return descriptors;
        }

        public TextureDescriptor[] GetTextureDescriptors()
        {
            return _cachedTextureDescriptors ??= GetTextureOrImageDescriptors(_usedTextures, _counts.IncrementTexturesCount);
        }

        public TextureDescriptor[] GetImageDescriptors()
        {
            return _cachedImageDescriptors ??= GetTextureOrImageDescriptors(_usedImages, _counts.IncrementImagesCount);
        }

        private static TextureDescriptor[] GetTextureOrImageDescriptors(Dictionary<TextureInfo, TextureMeta> dict, Func<int> getBindingCallback)
        {
            var descriptors = new TextureDescriptor[dict.Count];

            int i = 0;
            foreach (var kv in dict.OrderBy(x => x.Key.Indexed).OrderBy(x => x.Key.Handle))
            {
                var info = kv.Key;
                var meta = kv.Value;

                int binding = getBindingCallback();

                descriptors[i] = new TextureDescriptor(binding, meta.Type, info.Format, info.CbufSlot, info.Handle);
                descriptors[i].SetFlag(meta.UsageFlags);
                i++;
            }

            return descriptors;
        }
    }
}