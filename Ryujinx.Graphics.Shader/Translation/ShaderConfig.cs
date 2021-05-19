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

        public OutputTopology OutputTopology { get; }

        public int MaxOutputVertices { get; }

        public int LocalMemorySize { get; }

        public ImapPixelType[] ImapTypes { get; }

        public OmapTarget[] OmapTargets    { get; }
        public bool         OmapSampleMask { get; }
        public bool         OmapDepth      { get; }

        public IGpuAccessor GpuAccessor { get; }

        public TranslationFlags Flags { get; }

        public int Size { get; private set; }

        public byte ClipDistancesWritten { get; private set; }

        public FeatureFlags UsedFeatures { get; private set; }

        public HashSet<int> TextureHandlesForCache { get; }

        private readonly TranslationCounts _counts;

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

        public ShaderConfig(IGpuAccessor gpuAccessor, TranslationFlags flags, TranslationCounts counts)
        {
            Stage                  = ShaderStage.Compute;
            GpuAccessor            = gpuAccessor;
            Flags                  = flags;
            _counts                = counts;
            TextureHandlesForCache = new HashSet<int>();
            _usedTextures          = new Dictionary<TextureInfo, TextureMeta>();
            _usedImages            = new Dictionary<TextureInfo, TextureMeta>();
        }

        public ShaderConfig(ShaderHeader header, IGpuAccessor gpuAccessor, TranslationFlags flags, TranslationCounts counts) : this(gpuAccessor, flags, counts)
        {
            Stage             = header.Stage;
            GpPassthrough     = header.Stage == ShaderStage.Geometry && header.GpPassthrough;
            OutputTopology    = header.OutputTopology;
            MaxOutputVertices = header.MaxOutputVertexCount;
            LocalMemorySize   = header.ShaderLocalMemoryLowSize + header.ShaderLocalMemoryHighSize;
            ImapTypes         = header.ImapTypes;
            OmapTargets       = header.OmapTargets;
            OmapSampleMask    = header.OmapSampleMask;
            OmapDepth         = header.OmapDepth;
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
            if (GpuAccessor.QuerySupportsImageLoadFormatted())
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

        public void SizeAdd(int size)
        {
            Size += size;
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
            bool isImage = inst == Instruction.ImageLoad || inst == Instruction.ImageStore;
            bool isWrite = inst == Instruction.ImageStore;
            bool accurateType = inst != Instruction.TextureSize && inst != Instruction.Lod;

            if (isImage)
            {
                SetUsedTextureOrImage(_usedImages, cbufSlot, handle, type, format, true, isWrite, false);
            }
            else
            {
                SetUsedTextureOrImage(_usedTextures, cbufSlot, handle, type, TextureFormat.Unknown, flags.HasFlag(TextureFlags.IntCoords), false, accurateType);
            }
        }

        private static void SetUsedTextureOrImage(
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
            var isArray = type.HasFlag(SamplerType.Array);
            var isIndexed = type.HasFlag(SamplerType.Indexed);

            var usageFlags = TextureUsageFlags.None;

            if (intCoords)
            {
                usageFlags |= TextureUsageFlags.NeedsScaleValue;

                var canScale = (dimensions == 2 && !isArray) || (dimensions == 3 && isArray);
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
                    meta.UsageFlags |= existingMeta.UsageFlags;

                    // If the texture we have has inaccurate type information, then
                    // we prefer the most accurate one.
                    if (existingMeta.AccurateType)
                    {
                        meta.AccurateType = true;
                        meta.Type = existingMeta.Type;
                    }

                    dict[info] = meta;
                }
                else
                {
                    dict.Add(info, meta);
                }
            }
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
                usedMask = FillMask(usedMask);
            }

            return _cachedConstantBufferDescriptors = GetBufferDescriptors(usedMask, 0, _counts.IncrementUniformBuffersCount);
        }

        public BufferDescriptor[] GetStorageBufferDescriptors()
        {
            return _cachedStorageBufferDescriptors ??= GetBufferDescriptors(FillMask(_usedStorageBuffers), _usedStorageBuffersWrite, _counts.IncrementStorageBuffersCount);
        }

        private static int FillMask(int mask)
        {
            // When the storage or uniform buffers are used as array, we must allocate a binding
            // even for the "gaps" that are not used on the shader.
            // For this reason, fill up the gaps so that all slots up to the highest one are
            // marked as "used".
            return mask != 0 ? (int)(uint.MaxValue >> BitOperations.LeadingZeroCount((uint)mask)) : 0;
        }

        private static BufferDescriptor[] GetBufferDescriptors(int usedMask, int writtenMask, Func<int> getBindingCallback)
        {
            var descriptors = new BufferDescriptor[BitOperations.PopCount((uint)usedMask)];

            for (int i = 0; i < descriptors.Length; i++)
            {
                int slot = BitOperations.TrailingZeroCount(usedMask);

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