using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using Spv.Generator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using static Spv.Specification;
using SpvInstruction = Spv.Generator.Instruction;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    static class Declarations
    {
        private static readonly string[] _stagePrefixes = { "cp", "vp", "tcp", "tep", "gp", "fp" };

        public static void DeclareParameters(CodeGenContext context, StructuredFunction function)
        {
            DeclareParameters(context, function.InArguments, 0);
            DeclareParameters(context, function.OutArguments, function.InArguments.Length);
        }

        private static void DeclareParameters(CodeGenContext context, IEnumerable<AggregateType> argTypes, int argIndex)
        {
            foreach (var argType in argTypes)
            {
                var argPointerType = context.TypePointer(StorageClass.Function, context.GetType(argType));
                var spvArg = context.FunctionParameter(argPointerType);

                context.DeclareArgument(argIndex++, spvArg);
            }
        }

        public static void DeclareLocals(CodeGenContext context, StructuredFunction function)
        {
            foreach (AstOperand local in function.Locals)
            {
                var localPointerType = context.TypePointer(StorageClass.Function, context.GetType(local.VarType));
                var spvLocal = context.Variable(localPointerType, StorageClass.Function);

                context.AddLocalVariable(spvLocal);
                context.DeclareLocal(local, spvLocal);
            }
        }

        public static void DeclareLocalForArgs(CodeGenContext context, List<StructuredFunction> functions)
        {
            for (int funcIndex = 0; funcIndex < functions.Count; funcIndex++)
            {
                StructuredFunction function = functions[funcIndex];
                SpvInstruction[] locals = new SpvInstruction[function.InArguments.Length];

                for (int i = 0; i < function.InArguments.Length; i++)
                {
                    var type = function.GetArgumentType(i);
                    var localPointerType = context.TypePointer(StorageClass.Function, context.GetType(type));
                    var spvLocal = context.Variable(localPointerType, StorageClass.Function);

                    context.AddLocalVariable(spvLocal);

                    locals[i] = spvLocal;
                }

                context.DeclareLocalForArgs(funcIndex, locals);
            }
        }

        public static void DeclareAll(CodeGenContext context, StructuredProgramInfo info)
        {
            DeclareConstantBuffers(context, context.Config.Properties.ConstantBuffers.Values);
            DeclareStorageBuffers(context, context.Config.Properties.StorageBuffers.Values);
            DeclareMemories(context, context.Config.Properties.LocalMemories, context.LocalMemories, StorageClass.Private);
            DeclareMemories(context, context.Config.Properties.SharedMemories, context.SharedMemories, StorageClass.Workgroup);
            DeclareSamplers(context, context.Config.GetTextureDescriptors());
            DeclareImages(context, context.Config.GetImageDescriptors());
            DeclareInputsAndOutputs(context, info);
        }

        private static void DeclareMemories(
            CodeGenContext context,
            IReadOnlyDictionary<int, MemoryDefinition> memories,
            Dictionary<int, SpvInstruction> dict,
            StorageClass storage)
        {
            foreach ((int id, MemoryDefinition memory) in memories)
            {
                var pointerType = context.TypePointer(storage, context.GetType(memory.Type, memory.ArrayLength));
                var variable = context.Variable(pointerType, storage);

                context.AddGlobalVariable(variable);

                dict.Add(id, variable);
            }
        }

        private static void DeclareConstantBuffers(CodeGenContext context, IEnumerable<BufferDefinition> buffers)
        {
            DeclareBuffers(context, buffers, isBuffer: false);
        }

        private static void DeclareStorageBuffers(CodeGenContext context, IEnumerable<BufferDefinition> buffers)
        {
            DeclareBuffers(context, buffers, isBuffer: true);
        }

        private static void DeclareBuffers(CodeGenContext context, IEnumerable<BufferDefinition> buffers, bool isBuffer)
        {
            HashSet<SpvInstruction> decoratedTypes = new();

            foreach (BufferDefinition buffer in buffers)
            {
                int alignment = buffer.Layout == BufferLayout.Std140 ? 16 : 4;
                int alignmentMask = alignment - 1;
                int offset = 0;

                SpvInstruction[] structFieldTypes = new SpvInstruction[buffer.Type.Fields.Length];
                int[] structFieldOffsets = new int[buffer.Type.Fields.Length];

                for (int fieldIndex = 0; fieldIndex < buffer.Type.Fields.Length; fieldIndex++)
                {
                    StructureField field = buffer.Type.Fields[fieldIndex];
                    int fieldSize = (field.Type.GetSizeInBytes() + alignmentMask) & ~alignmentMask;

                    structFieldTypes[fieldIndex] = context.GetType(field.Type, field.ArrayLength);
                    structFieldOffsets[fieldIndex] = offset;

                    if (field.Type.HasFlag(AggregateType.Array))
                    {
                        // We can't decorate the type more than once.
                        if (decoratedTypes.Add(structFieldTypes[fieldIndex]))
                        {
                            context.Decorate(structFieldTypes[fieldIndex], Decoration.ArrayStride, (LiteralInteger)fieldSize);
                        }

                        // Zero lengths are assumed to be a "runtime array" (which does not have a explicit length
                        // specified on the shader, and instead assumes the bound buffer length).
                        // It is only valid as the last struct element.

                        Debug.Assert(field.ArrayLength > 0 || fieldIndex == buffer.Type.Fields.Length - 1);

                        offset += fieldSize * field.ArrayLength;
                    }
                    else
                    {
                        offset += fieldSize;
                    }
                }

                var structType = context.TypeStruct(false, structFieldTypes);

                if (decoratedTypes.Add(structType))
                {
                    context.Decorate(structType, isBuffer ? Decoration.BufferBlock : Decoration.Block);

                    for (int fieldIndex = 0; fieldIndex < structFieldOffsets.Length; fieldIndex++)
                    {
                        context.MemberDecorate(structType, fieldIndex, Decoration.Offset, (LiteralInteger)structFieldOffsets[fieldIndex]);
                    }
                }

                var pointerType = context.TypePointer(StorageClass.Uniform, structType);
                var variable = context.Variable(pointerType, StorageClass.Uniform);

                context.Name(variable, buffer.Name);
                context.Decorate(variable, Decoration.DescriptorSet, (LiteralInteger)buffer.Set);
                context.Decorate(variable, Decoration.Binding, (LiteralInteger)buffer.Binding);
                context.AddGlobalVariable(variable);

                if (isBuffer)
                {
                    context.StorageBuffers.Add(buffer.Binding, variable);
                }
                else
                {
                    context.ConstantBuffers.Add(buffer.Binding, variable);
                }
            }
        }

        private static void DeclareSamplers(CodeGenContext context, TextureDescriptor[] descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                var meta = new TextureMeta(descriptor.CbufSlot, descriptor.HandleIndex, descriptor.Format);

                if (context.Samplers.ContainsKey(meta))
                {
                    continue;
                }

                int setIndex = context.Config.Options.TargetApi == TargetApi.Vulkan ? 2 : 0;

                var dim = (descriptor.Type & SamplerType.Mask) switch
                {
                    SamplerType.Texture1D => Dim.Dim1D,
                    SamplerType.Texture2D => Dim.Dim2D,
                    SamplerType.Texture3D => Dim.Dim3D,
                    SamplerType.TextureCube => Dim.Cube,
                    SamplerType.TextureBuffer => Dim.Buffer,
                    _ => throw new InvalidOperationException($"Invalid sampler type \"{descriptor.Type & SamplerType.Mask}\"."),
                };

                var imageType = context.TypeImage(
                    context.TypeFP32(),
                    dim,
                    descriptor.Type.HasFlag(SamplerType.Shadow),
                    descriptor.Type.HasFlag(SamplerType.Array),
                    descriptor.Type.HasFlag(SamplerType.Multisample),
                    1,
                    ImageFormat.Unknown);

                var nameSuffix = meta.CbufSlot < 0 ? $"_tcb_{meta.Handle:X}" : $"_cb{meta.CbufSlot}_{meta.Handle:X}";

                var sampledImageType = context.TypeSampledImage(imageType);
                var sampledImagePointerType = context.TypePointer(StorageClass.UniformConstant, sampledImageType);
                var sampledImageVariable = context.Variable(sampledImagePointerType, StorageClass.UniformConstant);

                context.Samplers.Add(meta, (imageType, sampledImageType, sampledImageVariable));
                context.SamplersTypes.Add(meta, descriptor.Type);

                context.Name(sampledImageVariable, $"{GetStagePrefix(context.Config.Stage)}_tex{nameSuffix}");
                context.Decorate(sampledImageVariable, Decoration.DescriptorSet, (LiteralInteger)setIndex);
                context.Decorate(sampledImageVariable, Decoration.Binding, (LiteralInteger)descriptor.Binding);
                context.AddGlobalVariable(sampledImageVariable);
            }
        }

        private static void DeclareImages(CodeGenContext context, TextureDescriptor[] descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                var meta = new TextureMeta(descriptor.CbufSlot, descriptor.HandleIndex, descriptor.Format);

                if (context.Images.ContainsKey(meta))
                {
                    continue;
                }

                int setIndex = context.Config.Options.TargetApi == TargetApi.Vulkan ? 3 : 0;

                var dim = GetDim(descriptor.Type);

                var imageType = context.TypeImage(
                    context.GetType(meta.Format.GetComponentType()),
                    dim,
                    descriptor.Type.HasFlag(SamplerType.Shadow),
                    descriptor.Type.HasFlag(SamplerType.Array),
                    descriptor.Type.HasFlag(SamplerType.Multisample),
                    AccessQualifier.ReadWrite,
                    GetImageFormat(meta.Format));

                var nameSuffix = meta.CbufSlot < 0 ?
                    $"_tcb_{meta.Handle:X}_{meta.Format.ToGlslFormat()}" :
                    $"_cb{meta.CbufSlot}_{meta.Handle:X}_{meta.Format.ToGlslFormat()}";

                var imagePointerType = context.TypePointer(StorageClass.UniformConstant, imageType);
                var imageVariable = context.Variable(imagePointerType, StorageClass.UniformConstant);

                context.Images.Add(meta, (imageType, imageVariable));

                context.Name(imageVariable, $"{GetStagePrefix(context.Config.Stage)}_img{nameSuffix}");
                context.Decorate(imageVariable, Decoration.DescriptorSet, (LiteralInteger)setIndex);
                context.Decorate(imageVariable, Decoration.Binding, (LiteralInteger)descriptor.Binding);

                if (descriptor.Flags.HasFlag(TextureUsageFlags.ImageCoherent))
                {
                    context.Decorate(imageVariable, Decoration.Coherent);
                }

                context.AddGlobalVariable(imageVariable);
            }
        }

        private static Dim GetDim(SamplerType type)
        {
            return (type & SamplerType.Mask) switch
            {
                SamplerType.Texture1D => Dim.Dim1D,
                SamplerType.Texture2D => Dim.Dim2D,
                SamplerType.Texture3D => Dim.Dim3D,
                SamplerType.TextureCube => Dim.Cube,
                SamplerType.TextureBuffer => Dim.Buffer,
                _ => throw new ArgumentException($"Invalid sampler type \"{type & SamplerType.Mask}\"."),
            };
        }

        private static ImageFormat GetImageFormat(TextureFormat format)
        {
            return format switch
            {
                TextureFormat.Unknown => ImageFormat.Unknown,
                TextureFormat.R8Unorm => ImageFormat.R8,
                TextureFormat.R8Snorm => ImageFormat.R8Snorm,
                TextureFormat.R8Uint => ImageFormat.R8ui,
                TextureFormat.R8Sint => ImageFormat.R8i,
                TextureFormat.R16Float => ImageFormat.R16f,
                TextureFormat.R16Unorm => ImageFormat.R16,
                TextureFormat.R16Snorm => ImageFormat.R16Snorm,
                TextureFormat.R16Uint => ImageFormat.R16ui,
                TextureFormat.R16Sint => ImageFormat.R16i,
                TextureFormat.R32Float => ImageFormat.R32f,
                TextureFormat.R32Uint => ImageFormat.R32ui,
                TextureFormat.R32Sint => ImageFormat.R32i,
                TextureFormat.R8G8Unorm => ImageFormat.Rg8,
                TextureFormat.R8G8Snorm => ImageFormat.Rg8Snorm,
                TextureFormat.R8G8Uint => ImageFormat.Rg8ui,
                TextureFormat.R8G8Sint => ImageFormat.Rg8i,
                TextureFormat.R16G16Float => ImageFormat.Rg16f,
                TextureFormat.R16G16Unorm => ImageFormat.Rg16,
                TextureFormat.R16G16Snorm => ImageFormat.Rg16Snorm,
                TextureFormat.R16G16Uint => ImageFormat.Rg16ui,
                TextureFormat.R16G16Sint => ImageFormat.Rg16i,
                TextureFormat.R32G32Float => ImageFormat.Rg32f,
                TextureFormat.R32G32Uint => ImageFormat.Rg32ui,
                TextureFormat.R32G32Sint => ImageFormat.Rg32i,
                TextureFormat.R8G8B8A8Unorm => ImageFormat.Rgba8,
                TextureFormat.R8G8B8A8Snorm => ImageFormat.Rgba8Snorm,
                TextureFormat.R8G8B8A8Uint => ImageFormat.Rgba8ui,
                TextureFormat.R8G8B8A8Sint => ImageFormat.Rgba8i,
                TextureFormat.R16G16B16A16Float => ImageFormat.Rgba16f,
                TextureFormat.R16G16B16A16Unorm => ImageFormat.Rgba16,
                TextureFormat.R16G16B16A16Snorm => ImageFormat.Rgba16Snorm,
                TextureFormat.R16G16B16A16Uint => ImageFormat.Rgba16ui,
                TextureFormat.R16G16B16A16Sint => ImageFormat.Rgba16i,
                TextureFormat.R32G32B32A32Float => ImageFormat.Rgba32f,
                TextureFormat.R32G32B32A32Uint => ImageFormat.Rgba32ui,
                TextureFormat.R32G32B32A32Sint => ImageFormat.Rgba32i,
                TextureFormat.R10G10B10A2Unorm => ImageFormat.Rgb10A2,
                TextureFormat.R10G10B10A2Uint => ImageFormat.Rgb10a2ui,
                TextureFormat.R11G11B10Float => ImageFormat.R11fG11fB10f,
                _ => throw new ArgumentException($"Invalid texture format \"{format}\"."),
            };
        }

        private static void DeclareInputsAndOutputs(CodeGenContext context, StructuredProgramInfo info)
        {
            foreach (var ioDefinition in info.IoDefinitions)
            {
                PixelImap iq = PixelImap.Unused;

                if (context.Config.Stage == ShaderStage.Fragment)
                {
                    var ioVariable = ioDefinition.IoVariable;
                    if (ioVariable == IoVariable.UserDefined)
                    {
                        iq = context.Config.ImapTypes[ioDefinition.Location].GetFirstUsedType();
                    }
                    else
                    {
                        (_, AggregateType varType) = IoMap.GetSpirvBuiltIn(ioVariable);
                        AggregateType elemType = varType & AggregateType.ElementTypeMask;

                        if (elemType is AggregateType.S32 or AggregateType.U32)
                        {
                            iq = PixelImap.Constant;
                        }
                    }
                }

                bool isOutput = ioDefinition.StorageKind.IsOutput();
                bool isPerPatch = ioDefinition.StorageKind.IsPerPatch();

                DeclareInputOrOutput(context, ioDefinition, isOutput, isPerPatch, iq);
            }
        }

        private static void DeclareInputOrOutput(CodeGenContext context, IoDefinition ioDefinition, bool isOutput, bool isPerPatch, PixelImap iq = PixelImap.Unused)
        {
            IoVariable ioVariable = ioDefinition.IoVariable;
            var storageClass = isOutput ? StorageClass.Output : StorageClass.Input;

            bool isBuiltIn;
            BuiltIn builtIn = default;
            AggregateType varType;

            if (ioVariable == IoVariable.UserDefined)
            {
                varType = context.Config.GetUserDefinedType(ioDefinition.Location, isOutput);
                isBuiltIn = false;
            }
            else if (ioVariable == IoVariable.FragmentOutputColor)
            {
                varType = context.Config.GetFragmentOutputColorType(ioDefinition.Location);
                isBuiltIn = false;
            }
            else
            {
                (builtIn, varType) = IoMap.GetSpirvBuiltIn(ioVariable);
                isBuiltIn = true;

                if (varType == AggregateType.Invalid)
                {
                    throw new InvalidOperationException($"Unknown variable {ioVariable}.");
                }
            }

            bool hasComponent = context.Config.HasPerLocationInputOrOutputComponent(ioVariable, ioDefinition.Location, ioDefinition.Component, isOutput);

            if (hasComponent)
            {
                varType &= AggregateType.ElementTypeMask;
            }
            else if (ioVariable == IoVariable.UserDefined && context.Config.HasTransformFeedbackOutputs(isOutput))
            {
                varType &= AggregateType.ElementTypeMask;
                varType |= context.Config.GetTransformFeedbackOutputComponents(ioDefinition.Location, ioDefinition.Component) switch
                {
                    2 => AggregateType.Vector2,
                    3 => AggregateType.Vector3,
                    4 => AggregateType.Vector4,
                    _ => AggregateType.Invalid,
                };
            }

            var spvType = context.GetType(varType, IoMap.GetSpirvBuiltInArrayLength(ioVariable));
            bool builtInPassthrough = false;

            if (!isPerPatch && IoMap.IsPerVertex(ioVariable, context.Config.Stage, isOutput))
            {
                int arraySize = context.Config.Stage == ShaderStage.Geometry ? context.InputVertices : 32;
                spvType = context.TypeArray(spvType, context.Constant(context.TypeU32(), arraySize));

                if (context.Config.GpPassthrough && context.Config.GpuAccessor.QueryHostSupportsGeometryShaderPassthrough())
                {
                    builtInPassthrough = true;
                }
            }

            if (context.Config.Stage == ShaderStage.TessellationControl && isOutput && !isPerPatch)
            {
                spvType = context.TypeArray(spvType, context.Constant(context.TypeU32(), context.Config.ThreadsPerInputPrimitive));
            }

            var spvPointerType = context.TypePointer(storageClass, spvType);
            var spvVar = context.Variable(spvPointerType, storageClass);

            if (builtInPassthrough)
            {
                context.Decorate(spvVar, Decoration.PassthroughNV);
            }

            if (isBuiltIn)
            {
                if (isPerPatch)
                {
                    context.Decorate(spvVar, Decoration.Patch);
                }

                if (context.Config.GpuAccessor.QueryHostReducedPrecision() && ioVariable == IoVariable.Position)
                {
                    context.Decorate(spvVar, Decoration.Invariant);
                }

                context.Decorate(spvVar, Decoration.BuiltIn, (LiteralInteger)builtIn);
            }
            else if (isPerPatch)
            {
                context.Decorate(spvVar, Decoration.Patch);

                if (ioVariable == IoVariable.UserDefined)
                {
                    int location = context.Config.GetPerPatchAttributeLocation(ioDefinition.Location);

                    context.Decorate(spvVar, Decoration.Location, (LiteralInteger)location);
                }
            }
            else if (ioVariable == IoVariable.UserDefined)
            {
                context.Decorate(spvVar, Decoration.Location, (LiteralInteger)ioDefinition.Location);

                if (hasComponent)
                {
                    context.Decorate(spvVar, Decoration.Component, (LiteralInteger)ioDefinition.Component);
                }

                if (!isOutput &&
                    !isPerPatch &&
                    (context.Config.PassthroughAttributes & (1 << ioDefinition.Location)) != 0 &&
                    context.Config.GpuAccessor.QueryHostSupportsGeometryShaderPassthrough())
                {
                    context.Decorate(spvVar, Decoration.PassthroughNV);
                }
            }
            else if (ioVariable == IoVariable.FragmentOutputColor)
            {
                int location = ioDefinition.Location;

                if (context.Config.Stage == ShaderStage.Fragment && context.Config.GpuAccessor.QueryDualSourceBlendEnable())
                {
                    int firstLocation = BitOperations.TrailingZeroCount(context.Config.UsedOutputAttributes);
                    int index = location - firstLocation;
                    int mask = 3 << firstLocation;

                    if ((uint)index < 2 && (context.Config.UsedOutputAttributes & mask) == mask)
                    {
                        context.Decorate(spvVar, Decoration.Location, (LiteralInteger)firstLocation);
                        context.Decorate(spvVar, Decoration.Index, (LiteralInteger)index);
                    }
                    else
                    {
                        context.Decorate(spvVar, Decoration.Location, (LiteralInteger)location);
                    }
                }
                else
                {
                    context.Decorate(spvVar, Decoration.Location, (LiteralInteger)location);
                }
            }

            if (!isOutput)
            {
                switch (iq)
                {
                    case PixelImap.Constant:
                        context.Decorate(spvVar, Decoration.Flat);
                        break;
                    case PixelImap.ScreenLinear:
                        context.Decorate(spvVar, Decoration.NoPerspective);
                        break;
                }
            }
            else if (context.Config.TryGetTransformFeedbackOutput(
                ioVariable,
                ioDefinition.Location,
                ioDefinition.Component,
                out var transformFeedbackOutput))
            {
                context.Decorate(spvVar, Decoration.XfbBuffer, (LiteralInteger)transformFeedbackOutput.Buffer);
                context.Decorate(spvVar, Decoration.XfbStride, (LiteralInteger)transformFeedbackOutput.Stride);
                context.Decorate(spvVar, Decoration.Offset, (LiteralInteger)transformFeedbackOutput.Offset);
            }

            context.AddGlobalVariable(spvVar);

            var dict = isPerPatch
                ? (isOutput ? context.OutputsPerPatch : context.InputsPerPatch)
                : (isOutput ? context.Outputs : context.Inputs);
            dict.Add(ioDefinition, spvVar);
        }

        private static string GetStagePrefix(ShaderStage stage)
        {
            return _stagePrefixes[(int)stage];
        }
    }
}
