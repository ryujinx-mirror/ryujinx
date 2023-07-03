using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class Rewriter
    {
        public static void RunPass(HelperFunctionManager hfm, BasicBlock[] blocks, ShaderConfig config)
        {
            bool isVertexShader = config.Stage == ShaderStage.Vertex;
            bool isImpreciseFragmentShader = config.Stage == ShaderStage.Fragment && config.GpuAccessor.QueryHostReducedPrecision();
            bool hasConstantBufferDrawParameters = config.GpuAccessor.QueryHasConstantBufferDrawParameters();
            bool hasVectorIndexingBug = config.GpuAccessor.QueryHostHasVectorIndexingBug();
            bool supportsSnormBufferTextureFormat = config.GpuAccessor.QueryHostSupportsSnormBufferTextureFormat();

            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                BasicBlock block = blocks[blkIndex];

                for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
                {
                    if (node.Value is not Operation operation)
                    {
                        continue;
                    }

                    if (isVertexShader)
                    {
                        if (hasConstantBufferDrawParameters)
                        {
                            if (ReplaceConstantBufferWithDrawParameters(node, operation))
                            {
                                config.SetUsedFeature(FeatureFlags.DrawParameters);
                            }
                        }
                        else if (HasConstantBufferDrawParameters(operation))
                        {
                            config.SetUsedFeature(FeatureFlags.DrawParameters);
                        }
                    }

                    if (isImpreciseFragmentShader)
                    {
                        EnableForcePreciseIfNeeded(operation);
                    }

                    if (hasVectorIndexingBug)
                    {
                        InsertVectorComponentSelect(node, config);
                    }

                    if (operation is TextureOperation texOp)
                    {
                        node = InsertTexelFetchScale(hfm, node, config);
                        node = InsertTextureSizeUnscale(hfm, node, config);

                        if (texOp.Inst == Instruction.TextureSample)
                        {
                            node = InsertCoordNormalization(hfm, node, config);
                            node = InsertCoordGatherBias(node, config);
                            node = InsertConstOffsets(node, config);

                            if (texOp.Type == SamplerType.TextureBuffer && !supportsSnormBufferTextureFormat)
                            {
                                node = InsertSnormNormalization(node, config);
                            }
                        }
                    }
                    else
                    {
                        node = InsertSharedStoreSmallInt(hfm, node);

                        if (config.Options.TargetLanguage != TargetLanguage.Spirv)
                        {
                            node = InsertSharedAtomicSigned(hfm, node);
                        }
                    }
                }
            }
        }

        private static void EnableForcePreciseIfNeeded(Operation operation)
        {
            // There are some cases where a small bias is added to values to prevent division by zero.
            // When operating with reduced precision, it is possible for this bias to get rounded to 0
            // and cause a division by zero.
            // To prevent that, we force those operations to be precise even if the host wants
            // imprecise operations for performance.

            if (operation.Inst == (Instruction.FP32 | Instruction.Divide) &&
                operation.GetSource(0).Type == OperandType.Constant &&
                operation.GetSource(0).AsFloat() == 1f &&
                operation.GetSource(1).AsgOp is Operation addOp &&
                addOp.Inst == (Instruction.FP32 | Instruction.Add) &&
                addOp.GetSource(1).Type == OperandType.Constant)
            {
                addOp.ForcePrecise = true;
            }
        }

        private static void InsertVectorComponentSelect(LinkedListNode<INode> node, ShaderConfig config)
        {
            Operation operation = (Operation)node.Value;

            if (operation.Inst != Instruction.Load ||
                operation.StorageKind != StorageKind.ConstantBuffer ||
                operation.SourcesCount < 3)
            {
                return;
            }

            Operand bindingIndex = operation.GetSource(0);
            Operand fieldIndex = operation.GetSource(1);
            Operand elemIndex = operation.GetSource(operation.SourcesCount - 1);

            if (bindingIndex.Type != OperandType.Constant ||
                fieldIndex.Type != OperandType.Constant ||
                elemIndex.Type == OperandType.Constant)
            {
                return;
            }

            BufferDefinition buffer = config.Properties.ConstantBuffers[bindingIndex.Value];
            StructureField field = buffer.Type.Fields[fieldIndex.Value];

            int elemCount = (field.Type & AggregateType.ElementCountMask) switch
            {
                AggregateType.Vector2 => 2,
                AggregateType.Vector3 => 3,
                AggregateType.Vector4 => 4,
                _ => 1,
            };

            if (elemCount == 1)
            {
                return;
            }

            Operand result = null;

            for (int i = 0; i < elemCount; i++)
            {
                Operand value = Local();
                Operand[] inputs = new Operand[operation.SourcesCount];

                for (int srcIndex = 0; srcIndex < inputs.Length - 1; srcIndex++)
                {
                    inputs[srcIndex] = operation.GetSource(srcIndex);
                }

                inputs[^1] = Const(i);

                Operation loadOp = new(Instruction.Load, StorageKind.ConstantBuffer, value, inputs);

                node.List.AddBefore(node, loadOp);

                if (i == 0)
                {
                    result = value;
                }
                else
                {
                    Operand isCurrentIndex = Local();
                    Operand selection = Local();

                    Operation compareOp = new(Instruction.CompareEqual, isCurrentIndex, new Operand[] { elemIndex, Const(i) });
                    Operation selectOp = new(Instruction.ConditionalSelect, selection, new Operand[] { isCurrentIndex, value, result });

                    node.List.AddBefore(node, compareOp);
                    node.List.AddBefore(node, selectOp);

                    result = selection;
                }
            }

            operation.TurnIntoCopy(result);
        }

        private static LinkedListNode<INode> InsertSharedStoreSmallInt(HelperFunctionManager hfm, LinkedListNode<INode> node)
        {
            Operation operation = (Operation)node.Value;
            HelperFunctionName name;

            if (operation.StorageKind == StorageKind.SharedMemory8)
            {
                name = HelperFunctionName.SharedStore8;
            }
            else if (operation.StorageKind == StorageKind.SharedMemory16)
            {
                name = HelperFunctionName.SharedStore16;
            }
            else
            {
                return node;
            }

            if (operation.Inst != Instruction.Store)
            {
                return node;
            }

            Operand memoryId = operation.GetSource(0);
            Operand byteOffset = operation.GetSource(1);
            Operand value = operation.GetSource(2);

            Debug.Assert(memoryId.Type == OperandType.Constant);

            int functionId = hfm.GetOrCreateFunctionId(name, memoryId.Value);

            Operand[] callArgs = new Operand[] { Const(functionId), byteOffset, value };

            LinkedListNode<INode> newNode = node.List.AddBefore(node, new Operation(Instruction.Call, 0, (Operand)null, callArgs));

            Utils.DeleteNode(node, operation);

            return newNode;
        }

        private static LinkedListNode<INode> InsertSharedAtomicSigned(HelperFunctionManager hfm, LinkedListNode<INode> node)
        {
            Operation operation = (Operation)node.Value;
            HelperFunctionName name;

            if (operation.Inst == Instruction.AtomicMaxS32)
            {
                name = HelperFunctionName.SharedAtomicMaxS32;
            }
            else if (operation.Inst == Instruction.AtomicMinS32)
            {
                name = HelperFunctionName.SharedAtomicMinS32;
            }
            else
            {
                return node;
            }

            if (operation.StorageKind != StorageKind.SharedMemory)
            {
                return node;
            }

            Operand result = operation.Dest;
            Operand memoryId = operation.GetSource(0);
            Operand byteOffset = operation.GetSource(1);
            Operand value = operation.GetSource(2);

            Debug.Assert(memoryId.Type == OperandType.Constant);

            int functionId = hfm.GetOrCreateFunctionId(name, memoryId.Value);

            Operand[] callArgs = new Operand[] { Const(functionId), byteOffset, value };

            LinkedListNode<INode> newNode = node.List.AddBefore(node, new Operation(Instruction.Call, 0, result, callArgs));

            Utils.DeleteNode(node, operation);

            return newNode;
        }

        private static LinkedListNode<INode> InsertTexelFetchScale(HelperFunctionManager hfm, LinkedListNode<INode> node, ShaderConfig config)
        {
            TextureOperation texOp = (TextureOperation)node.Value;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;
            bool intCoords = (texOp.Flags & TextureFlags.IntCoords) != 0;
            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;

            int coordsCount = texOp.Type.GetDimensions();

            int coordsIndex = isBindless || isIndexed ? 1 : 0;

            bool isImage = IsImageInstructionWithScale(texOp.Inst);

            if ((texOp.Inst == Instruction.TextureSample || isImage) &&
                (intCoords || isImage) &&
                !isBindless &&
                !isIndexed &&
                config.Stage.SupportsRenderScale() &&
                TypeSupportsScale(texOp.Type))
            {
                int functionId = hfm.GetOrCreateFunctionId(HelperFunctionName.TexelFetchScale);
                int samplerIndex = isImage
                    ? config.ResourceManager.GetTextureDescriptors().Length + config.ResourceManager.FindImageDescriptorIndex(texOp.Binding)
                    : config.ResourceManager.FindTextureDescriptorIndex(texOp.Binding);

                for (int index = 0; index < coordsCount; index++)
                {
                    Operand scaledCoord = Local();
                    Operand[] callArgs;

                    if (config.Stage == ShaderStage.Fragment)
                    {
                        callArgs = new Operand[] { Const(functionId), texOp.GetSource(coordsIndex + index), Const(samplerIndex), Const(index) };
                    }
                    else
                    {
                        callArgs = new Operand[] { Const(functionId), texOp.GetSource(coordsIndex + index), Const(samplerIndex) };
                    }

                    node.List.AddBefore(node, new Operation(Instruction.Call, 0, scaledCoord, callArgs));

                    texOp.SetSource(coordsIndex + index, scaledCoord);
                }
            }

            return node;
        }

        private static LinkedListNode<INode> InsertTextureSizeUnscale(HelperFunctionManager hfm, LinkedListNode<INode> node, ShaderConfig config)
        {
            TextureOperation texOp = (TextureOperation)node.Value;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;
            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;

            if (texOp.Inst == Instruction.TextureSize &&
                texOp.Index < 2 &&
                !isBindless &&
                !isIndexed &&
                config.Stage.SupportsRenderScale() &&
                TypeSupportsScale(texOp.Type))
            {
                int functionId = hfm.GetOrCreateFunctionId(HelperFunctionName.TextureSizeUnscale);
                int samplerIndex = config.ResourceManager.FindTextureDescriptorIndex(texOp.Binding);

                for (int index = texOp.DestsCount - 1; index >= 0; index--)
                {
                    Operand dest = texOp.GetDest(index);

                    Operand unscaledSize = Local();

                    // Replace all uses with the unscaled size value.
                    // This must be done before the call is added, since it also is a use of the original size.
                    foreach (INode useOp in dest.UseOps)
                    {
                        for (int srcIndex = 0; srcIndex < useOp.SourcesCount; srcIndex++)
                        {
                            if (useOp.GetSource(srcIndex) == dest)
                            {
                                useOp.SetSource(srcIndex, unscaledSize);
                            }
                        }
                    }

                    Operand[] callArgs = new Operand[] { Const(functionId), dest, Const(samplerIndex) };

                    node.List.AddAfter(node, new Operation(Instruction.Call, 0, unscaledSize, callArgs));
                }
            }

            return node;
        }

        private static bool IsImageInstructionWithScale(Instruction inst)
        {
            // Currently, we don't support scaling images that are modified,
            // so we only need to care about the load instruction.
            return inst == Instruction.ImageLoad;
        }

        private static bool TypeSupportsScale(SamplerType type)
        {
            return (type & SamplerType.Mask) == SamplerType.Texture2D;
        }

        private static LinkedListNode<INode> InsertCoordNormalization(HelperFunctionManager hfm, LinkedListNode<INode> node, ShaderConfig config)
        {
            // Emulate non-normalized coordinates by normalizing the coordinates on the shader.
            // Without normalization, the coordinates are expected to the in the [0, W or H] range,
            // and otherwise, it is expected to be in the [0, 1] range.
            // We normalize by dividing the coords by the texture size.

            TextureOperation texOp = (TextureOperation)node.Value;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;

            if (isBindless)
            {
                return node;
            }

            bool intCoords = (texOp.Flags & TextureFlags.IntCoords) != 0;

            (int cbufSlot, int handle) = config.ResourceManager.GetCbufSlotAndHandleForTexture(texOp.Binding);

            bool isCoordNormalized = config.GpuAccessor.QueryTextureCoordNormalized(handle, cbufSlot);

            if (isCoordNormalized || intCoords)
            {
                return node;
            }

            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;

            int coordsCount = texOp.Type.GetDimensions();
            int coordsIndex = isBindless || isIndexed ? 1 : 0;

            config.SetUsedFeature(FeatureFlags.IntegerSampling);

            int normCoordsCount = (texOp.Type & SamplerType.Mask) == SamplerType.TextureCube ? 2 : coordsCount;

            for (int index = 0; index < normCoordsCount; index++)
            {
                Operand coordSize = Local();

                Operand[] texSizeSources;

                if (isBindless || isIndexed)
                {
                    texSizeSources = new Operand[] { texOp.GetSource(0), Const(0) };
                }
                else
                {
                    texSizeSources = new Operand[] { Const(0) };
                }

                LinkedListNode<INode> textureSizeNode = node.List.AddBefore(node, new TextureOperation(
                    Instruction.TextureSize,
                    texOp.Type,
                    texOp.Format,
                    texOp.Flags,
                    texOp.Binding,
                    index,
                    new[] { coordSize },
                    texSizeSources));

                config.ResourceManager.SetUsageFlagsForTextureQuery(texOp.Binding, texOp.Type);

                Operand source = texOp.GetSource(coordsIndex + index);

                Operand coordNormalized = Local();

                node.List.AddBefore(node, new Operation(Instruction.FP32 | Instruction.Divide, coordNormalized, source, GenerateI2f(node, coordSize)));

                texOp.SetSource(coordsIndex + index, coordNormalized);

                InsertTextureSizeUnscale(hfm, textureSizeNode, config);
            }

            return node;
        }

        private static LinkedListNode<INode> InsertCoordGatherBias(LinkedListNode<INode> node, ShaderConfig config)
        {
            // The gather behavior when the coordinate sits right in the middle of two texels is not well defined.
            // To ensure the correct texel is sampled, we add a small bias value to the coordinate.
            // This value is calculated as the minimum value required to change the texel it will sample from,
            // and is 0 if the host does not require the bias.

            TextureOperation texOp = (TextureOperation)node.Value;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;
            bool isGather = (texOp.Flags & TextureFlags.Gather) != 0;

            int gatherBiasPrecision = config.GpuAccessor.QueryHostGatherBiasPrecision();

            if (!isGather || gatherBiasPrecision == 0)
            {
                return node;
            }

#pragma warning disable IDE0059 // Remove unnecessary value assignment
            bool intCoords = (texOp.Flags & TextureFlags.IntCoords) != 0;

            bool isArray = (texOp.Type & SamplerType.Array) != 0;
            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;
#pragma warning restore IDE0059

            int coordsCount = texOp.Type.GetDimensions();
            int coordsIndex = isBindless || isIndexed ? 1 : 0;

            config.SetUsedFeature(FeatureFlags.IntegerSampling);

            int normCoordsCount = (texOp.Type & SamplerType.Mask) == SamplerType.TextureCube ? 2 : coordsCount;

            for (int index = 0; index < normCoordsCount; index++)
            {
                Operand coordSize = Local();
                Operand scaledSize = Local();
                Operand bias = Local();

                Operand[] texSizeSources;

                if (isBindless || isIndexed)
                {
                    texSizeSources = new Operand[] { texOp.GetSource(0), Const(0) };
                }
                else
                {
                    texSizeSources = new Operand[] { Const(0) };
                }

                node.List.AddBefore(node, new TextureOperation(
                    Instruction.TextureSize,
                    texOp.Type,
                    texOp.Format,
                    texOp.Flags,
                    texOp.Binding,
                    index,
                    new[] { coordSize },
                    texSizeSources));

                node.List.AddBefore(node, new Operation(
                    Instruction.FP32 | Instruction.Multiply,
                    scaledSize,
                    GenerateI2f(node, coordSize),
                    ConstF((float)(1 << (gatherBiasPrecision + 1)))));
                node.List.AddBefore(node, new Operation(Instruction.FP32 | Instruction.Divide, bias, ConstF(1f), scaledSize));

                Operand source = texOp.GetSource(coordsIndex + index);

                Operand coordBiased = Local();

                node.List.AddBefore(node, new Operation(Instruction.FP32 | Instruction.Add, coordBiased, source, bias));

                texOp.SetSource(coordsIndex + index, coordBiased);
            }

            return node;
        }

        private static LinkedListNode<INode> InsertConstOffsets(LinkedListNode<INode> node, ShaderConfig config)
        {
            // Non-constant texture offsets are not allowed (according to the spec),
            // however some GPUs does support that.
            // For GPUs where it is not supported, we can replace the instruction with the following:
            // For texture*Offset, we replace it by texture*, and add the offset to the P coords.
            // The offset can be calculated as offset / textureSize(lod), where lod = textureQueryLod(coords).
            // For texelFetchOffset, we replace it by texelFetch and add the offset to the P coords directly.
            // For textureGatherOffset, we split the operation into up to 4 operations, one for each component
            // that is accessed, where each textureGather operation has a different offset for each pixel.

            TextureOperation texOp = (TextureOperation)node.Value;

            bool hasOffset = (texOp.Flags & TextureFlags.Offset) != 0;
            bool hasOffsets = (texOp.Flags & TextureFlags.Offsets) != 0;

            bool hasInvalidOffset = (hasOffset || hasOffsets) && !config.GpuAccessor.QueryHostSupportsNonConstantTextureOffset();

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;

            if (!hasInvalidOffset)
            {
                return node;
            }

            bool isGather = (texOp.Flags & TextureFlags.Gather) != 0;
            bool hasDerivatives = (texOp.Flags & TextureFlags.Derivatives) != 0;
            bool intCoords = (texOp.Flags & TextureFlags.IntCoords) != 0;
            bool hasLodBias = (texOp.Flags & TextureFlags.LodBias) != 0;
            bool hasLodLevel = (texOp.Flags & TextureFlags.LodLevel) != 0;

            bool isArray = (texOp.Type & SamplerType.Array) != 0;
            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;
            bool isMultisample = (texOp.Type & SamplerType.Multisample) != 0;
            bool isShadow = (texOp.Type & SamplerType.Shadow) != 0;

            int coordsCount = texOp.Type.GetDimensions();

            int offsetsCount;

            if (hasOffsets)
            {
                offsetsCount = coordsCount * 4;
            }
            else if (hasOffset)
            {
                offsetsCount = coordsCount;
            }
            else
            {
                offsetsCount = 0;
            }

            Operand[] offsets = new Operand[offsetsCount];
            Operand[] sources = new Operand[texOp.SourcesCount - offsetsCount];

            int copyCount = 0;

            if (isBindless || isIndexed)
            {
                copyCount++;
            }

            Operand[] lodSources = new Operand[copyCount + coordsCount];

            for (int index = 0; index < lodSources.Length; index++)
            {
                lodSources[index] = texOp.GetSource(index);
            }

            copyCount += coordsCount;

            if (isArray)
            {
                copyCount++;
            }

            if (isShadow)
            {
                copyCount++;
            }

            if (hasDerivatives)
            {
                copyCount += coordsCount * 2;
            }

            if (isMultisample)
            {
                copyCount++;
            }
            else if (hasLodLevel)
            {
                copyCount++;
            }

            int srcIndex = 0;
            int dstIndex = 0;

            for (int index = 0; index < copyCount; index++)
            {
                sources[dstIndex++] = texOp.GetSource(srcIndex++);
            }

            bool areAllOffsetsConstant = true;

            for (int index = 0; index < offsetsCount; index++)
            {
                Operand offset = texOp.GetSource(srcIndex++);

                areAllOffsetsConstant &= offset.Type == OperandType.Constant;

                offsets[index] = offset;
            }

            hasInvalidOffset &= !areAllOffsetsConstant;

            if (!hasInvalidOffset)
            {
                return node;
            }

            if (hasLodBias)
            {
                sources[dstIndex++] = texOp.GetSource(srcIndex++);
            }

            if (isGather && !isShadow)
            {
                sources[dstIndex++] = texOp.GetSource(srcIndex++);
            }

            int coordsIndex = isBindless || isIndexed ? 1 : 0;

            int componentIndex = texOp.Index;

            Operand[] dests = new Operand[texOp.DestsCount];

            for (int i = 0; i < texOp.DestsCount; i++)
            {
                dests[i] = texOp.GetDest(i);
            }

            Operand bindlessHandle = isBindless || isIndexed ? sources[0] : null;

            LinkedListNode<INode> oldNode = node;

            if (isGather && !isShadow)
            {
                config.SetUsedFeature(FeatureFlags.IntegerSampling);

                Operand[] newSources = new Operand[sources.Length];

                sources.CopyTo(newSources, 0);

                Operand[] texSizes = InsertTextureLod(node, texOp, lodSources, bindlessHandle, coordsCount);

                int destIndex = 0;

                for (int compIndex = 0; compIndex < 4; compIndex++)
                {
                    if (((texOp.Index >> compIndex) & 1) == 0)
                    {
                        continue;
                    }

                    for (int index = 0; index < coordsCount; index++)
                    {
                        Operand offset = Local();

                        Operand intOffset = offsets[index + (hasOffsets ? compIndex * coordsCount : 0)];

                        node.List.AddBefore(node, new Operation(
                            Instruction.FP32 | Instruction.Divide,
                            offset,
                            GenerateI2f(node, intOffset),
                            GenerateI2f(node, texSizes[index])));

                        Operand source = sources[coordsIndex + index];

                        Operand coordPlusOffset = Local();

                        node.List.AddBefore(node, new Operation(Instruction.FP32 | Instruction.Add, coordPlusOffset, source, offset));

                        newSources[coordsIndex + index] = coordPlusOffset;
                    }

                    TextureOperation newTexOp = new(
                        Instruction.TextureSample,
                        texOp.Type,
                        texOp.Format,
                        texOp.Flags & ~(TextureFlags.Offset | TextureFlags.Offsets),
                        texOp.Binding,
                        1,
                        new[] { dests[destIndex++] },
                        newSources);

                    node = node.List.AddBefore(node, newTexOp);
                }
            }
            else
            {
                if (intCoords)
                {
                    for (int index = 0; index < coordsCount; index++)
                    {
                        Operand source = sources[coordsIndex + index];

                        Operand coordPlusOffset = Local();

                        node.List.AddBefore(node, new Operation(Instruction.Add, coordPlusOffset, source, offsets[index]));

                        sources[coordsIndex + index] = coordPlusOffset;
                    }
                }
                else
                {
                    config.SetUsedFeature(FeatureFlags.IntegerSampling);

                    Operand[] texSizes = InsertTextureLod(node, texOp, lodSources, bindlessHandle, coordsCount);

                    for (int index = 0; index < coordsCount; index++)
                    {
                        Operand offset = Local();

                        Operand intOffset = offsets[index];

                        node.List.AddBefore(node, new Operation(
                            Instruction.FP32 | Instruction.Divide,
                            offset,
                            GenerateI2f(node, intOffset),
                            GenerateI2f(node, texSizes[index])));

                        Operand source = sources[coordsIndex + index];

                        Operand coordPlusOffset = Local();

                        node.List.AddBefore(node, new Operation(Instruction.FP32 | Instruction.Add, coordPlusOffset, source, offset));

                        sources[coordsIndex + index] = coordPlusOffset;
                    }
                }

                TextureOperation newTexOp = new(
                    Instruction.TextureSample,
                    texOp.Type,
                    texOp.Format,
                    texOp.Flags & ~(TextureFlags.Offset | TextureFlags.Offsets),
                    texOp.Binding,
                    componentIndex,
                    dests,
                    sources);

                node = node.List.AddBefore(node, newTexOp);
            }

            node.List.Remove(oldNode);

            for (int index = 0; index < texOp.SourcesCount; index++)
            {
                texOp.SetSource(index, null);
            }

            return node;
        }

        private static Operand[] InsertTextureLod(
            LinkedListNode<INode> node,
            TextureOperation texOp,
            Operand[] lodSources,
            Operand bindlessHandle,
            int coordsCount)
        {
            Operand[] texSizes = new Operand[coordsCount];

            Operand lod = Local();

            node.List.AddBefore(node, new TextureOperation(
                Instruction.Lod,
                texOp.Type,
                texOp.Format,
                texOp.Flags,
                texOp.Binding,
                0,
                new[] { lod },
                lodSources));

            for (int index = 0; index < coordsCount; index++)
            {
                texSizes[index] = Local();

                Operand[] texSizeSources;

                if (bindlessHandle != null)
                {
                    texSizeSources = new Operand[] { bindlessHandle, GenerateF2i(node, lod) };
                }
                else
                {
                    texSizeSources = new Operand[] { GenerateF2i(node, lod) };
                }

                node.List.AddBefore(node, new TextureOperation(
                    Instruction.TextureSize,
                    texOp.Type,
                    texOp.Format,
                    texOp.Flags,
                    texOp.Binding,
                    index,
                    new[] { texSizes[index] },
                    texSizeSources));
            }

            return texSizes;
        }

        private static LinkedListNode<INode> InsertSnormNormalization(LinkedListNode<INode> node, ShaderConfig config)
        {
            TextureOperation texOp = (TextureOperation)node.Value;

            // We can't query the format of a bindless texture,
            // because the handle is unknown, it can have any format.
            if (texOp.Flags.HasFlag(TextureFlags.Bindless))
            {
                return node;
            }

            (int cbufSlot, int handle) = config.ResourceManager.GetCbufSlotAndHandleForTexture(texOp.Binding);

            TextureFormat format = config.GpuAccessor.QueryTextureFormat(handle, cbufSlot);

            int maxPositive = format switch
            {
                TextureFormat.R8Snorm => sbyte.MaxValue,
                TextureFormat.R8G8Snorm => sbyte.MaxValue,
                TextureFormat.R8G8B8A8Snorm => sbyte.MaxValue,
                TextureFormat.R16Snorm => short.MaxValue,
                TextureFormat.R16G16Snorm => short.MaxValue,
                TextureFormat.R16G16B16A16Snorm => short.MaxValue,
                _ => 0,
            };

            // The value being 0 means that the format is not a SNORM format,
            // so there's nothing to do here.
            if (maxPositive == 0)
            {
                return node;
            }

            // Do normalization. We assume SINT formats are being used
            // as replacement for SNORM (which is not supported).
            for (int i = 0; i < texOp.DestsCount; i++)
            {
                Operand dest = texOp.GetDest(i);

                INode[] uses = dest.UseOps.ToArray();

                Operation convOp = new(Instruction.ConvertS32ToFP32, Local(), dest);
                Operation normOp = new(Instruction.FP32 | Instruction.Multiply, Local(), convOp.Dest, ConstF(1f / maxPositive));

                node = node.List.AddAfter(node, convOp);
                node = node.List.AddAfter(node, normOp);

                foreach (INode useOp in uses)
                {
                    if (useOp is not Operation op)
                    {
                        continue;
                    }

                    // Replace all uses of the texture pixel value with the normalized value.
                    for (int index = 0; index < op.SourcesCount; index++)
                    {
                        if (op.GetSource(index) == dest)
                        {
                            op.SetSource(index, normOp.Dest);
                        }
                    }
                }
            }

            return node;
        }

        private static Operand GenerateI2f(LinkedListNode<INode> node, Operand value)
        {
            Operand res = Local();

            node.List.AddBefore(node, new Operation(Instruction.ConvertS32ToFP32, res, value));

            return res;
        }

        private static Operand GenerateF2i(LinkedListNode<INode> node, Operand value)
        {
            Operand res = Local();

            node.List.AddBefore(node, new Operation(Instruction.ConvertFP32ToS32, res, value));

            return res;
        }

        private static bool ReplaceConstantBufferWithDrawParameters(LinkedListNode<INode> node, Operation operation)
        {
            Operand GenerateLoad(IoVariable ioVariable)
            {
                Operand value = Local();
                node.List.AddBefore(node, new Operation(Instruction.Load, StorageKind.Input, value, Const((int)ioVariable)));
                return value;
            }

            bool modified = false;

            for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
            {
                Operand src = operation.GetSource(srcIndex);

                if (src.Type == OperandType.ConstantBuffer && src.GetCbufSlot() == 0)
                {
                    switch (src.GetCbufOffset())
                    {
                        case Constants.NvnBaseVertexByteOffset / 4:
                            operation.SetSource(srcIndex, GenerateLoad(IoVariable.BaseVertex));
                            modified = true;
                            break;
                        case Constants.NvnBaseInstanceByteOffset / 4:
                            operation.SetSource(srcIndex, GenerateLoad(IoVariable.BaseInstance));
                            modified = true;
                            break;
                        case Constants.NvnDrawIndexByteOffset / 4:
                            operation.SetSource(srcIndex, GenerateLoad(IoVariable.DrawIndex));
                            modified = true;
                            break;
                    }
                }
            }

            return modified;
        }

        private static bool HasConstantBufferDrawParameters(Operation operation)
        {
            for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
            {
                Operand src = operation.GetSource(srcIndex);

                if (src.Type == OperandType.ConstantBuffer && src.GetCbufSlot() == 0)
                {
                    switch (src.GetCbufOffset())
                    {
                        case Constants.NvnBaseVertexByteOffset / 4:
                        case Constants.NvnBaseInstanceByteOffset / 4:
                        case Constants.NvnDrawIndexByteOffset / 4:
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
