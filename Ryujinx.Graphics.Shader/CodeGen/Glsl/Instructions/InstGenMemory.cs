using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions.InstGenHelper;
using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenMemory
    {
        public static string ImageLoadOrStore(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;

            // TODO: Bindless texture support. For now we just return 0/do nothing.
            if (isBindless)
            {
                switch (texOp.Inst)
                {
                    case Instruction.ImageStore:
                        return "// imageStore(bindless)";
                    case Instruction.ImageLoad:
                        NumberFormatter.TryFormat(0, texOp.Format.GetComponentType(), out string imageConst);
                        return imageConst;
                    default:
                        return NumberFormatter.FormatInt(0);
                }
            }

            bool isArray   = (texOp.Type & SamplerType.Array)   != 0;
            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;

            string texCall;

            if (texOp.Inst == Instruction.ImageAtomic)
            {
                texCall = (texOp.Flags & TextureFlags.AtomicMask) switch {
                    TextureFlags.Add        => "imageAtomicAdd",
                    TextureFlags.Minimum    => "imageAtomicMin",
                    TextureFlags.Maximum    => "imageAtomicMax",
                    TextureFlags.Increment  => "imageAtomicAdd", // TODO: Clamp value.
                    TextureFlags.Decrement  => "imageAtomicAdd", // TODO: Clamp value.
                    TextureFlags.BitwiseAnd => "imageAtomicAnd",
                    TextureFlags.BitwiseOr  => "imageAtomicOr",
                    TextureFlags.BitwiseXor => "imageAtomicXor",
                    TextureFlags.Swap       => "imageAtomicExchange",
                    TextureFlags.CAS        => "imageAtomicCompSwap",
                    _                       => "imageAtomicAdd",
                };
            }
            else
            {
                texCall = texOp.Inst == Instruction.ImageLoad ? "imageLoad" : "imageStore";
            }

            int srcIndex = isBindless ? 1 : 0;

            string Src(VariableType type)
            {
                return GetSoureExpr(context, texOp.GetSource(srcIndex++), type);
            }

            string indexExpr = null;

            if (isIndexed)
            {
                indexExpr = Src(VariableType.S32);
            }

            string imageName = OperandManager.GetImageName(context.Config.Stage, texOp, indexExpr);

            texCall += "(" + imageName;

            int coordsCount = texOp.Type.GetDimensions();

            int pCount = coordsCount + (isArray ? 1 : 0);

            void Append(string str)
            {
                texCall += ", " + str;
            }

            string ApplyScaling(string vector)
            {
                if ((context.Config.Stage.SupportsRenderScale()) &&
                    texOp.Inst == Instruction.ImageLoad &&
                    !isBindless &&
                    !isIndexed)
                {
                    // Image scales start after texture ones.
                    int scaleIndex = context.Config.GetTextureDescriptors().Length + context.FindImageDescriptorIndex(texOp);

                    if (pCount == 3 && isArray)
                    {
                        // The array index is not scaled, just x and y.
                        vector = "ivec3(Helper_TexelFetchScale((" + vector + ").xy, " + scaleIndex + "), (" + vector + ").z)";
                    }
                    else if (pCount == 2 && !isArray)
                    {
                        vector = "Helper_TexelFetchScale(" + vector + ", " + scaleIndex + ")";
                    }
                }

                return vector;
            }

            if (pCount > 1)
            {
                string[] elems = new string[pCount];

                for (int index = 0; index < pCount; index++)
                {
                    elems[index] = Src(VariableType.S32);
                }

                Append(ApplyScaling("ivec" + pCount + "(" + string.Join(", ", elems) + ")"));
            }
            else
            {
                Append(Src(VariableType.S32));
            }

            if (texOp.Inst == Instruction.ImageStore)
            {
                VariableType type = texOp.Format.GetComponentType();

                string[] cElems = new string[4];

                for (int index = 0; index < 4; index++)
                {
                    if (srcIndex < texOp.SourcesCount)
                    {
                        cElems[index] = Src(type);
                    }
                    else
                    {
                        cElems[index] = type switch
                        {
                            VariableType.S32 => NumberFormatter.FormatInt(0),
                            VariableType.U32 => NumberFormatter.FormatUint(0),
                            _                => NumberFormatter.FormatFloat(0)
                        };
                    }
                }

                string prefix = type switch
                {
                    VariableType.S32 => "i",
                    VariableType.U32 => "u",
                    _                => string.Empty
                };

                Append(prefix + "vec4(" + string.Join(", ", cElems) + ")");
            }

            if (texOp.Inst == Instruction.ImageAtomic)
            {
                VariableType type = texOp.Format.GetComponentType();

                if ((texOp.Flags & TextureFlags.AtomicMask) == TextureFlags.CAS)
                {
                    Append(Src(type)); // Compare value.
                }

                string value = (texOp.Flags & TextureFlags.AtomicMask) switch
                {
                    TextureFlags.Increment => NumberFormatter.FormatInt(1, type), // TODO: Clamp value
                    TextureFlags.Decrement => NumberFormatter.FormatInt(-1, type), // TODO: Clamp value
                    _ => Src(type)
                };

                Append(value);

                texCall += ")";

                if (type != VariableType.S32)
                {
                    texCall = "int(" + texCall + ")";
                }
            }
            else
            {
                texCall += ")" + (texOp.Inst == Instruction.ImageLoad ? GetMask(texOp.Index) : "");
            }

            return texCall;
        }

        public static string LoadAttribute(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);
            IAstNode src3 = operation.GetSource(2);

            if (!(src1 is AstOperand baseAttr) || baseAttr.Type != OperandType.Constant)
            {
                throw new InvalidOperationException($"First input of {nameof(Instruction.LoadAttribute)} must be a constant operand.");
            }

            string indexExpr = GetSoureExpr(context, src3, GetSrcVarType(operation.Inst, 2));

            if (src2 is AstOperand operand && operand.Type == OperandType.Constant)
            {
                int attrOffset = baseAttr.Value + (operand.Value << 2);
                return OperandManager.GetAttributeName(attrOffset, context.Config, perPatch: false, isOutAttr: false, indexExpr);
            }
            else
            {
                string attrExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));
                attrExpr = Enclose(attrExpr, src2, Instruction.ShiftRightS32, isLhs: true);
                return OperandManager.GetAttributeName(attrExpr, context.Config, isOutAttr: false, indexExpr);
            }
        }

        public static string LoadConstant(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));
            offsetExpr = Enclose(offsetExpr, src2, Instruction.ShiftRightS32, isLhs: true);

            var config = context.Config;
            bool indexElement = !config.GpuAccessor.QueryHostHasVectorIndexingBug();

            if (src1 is AstOperand operand && operand.Type == OperandType.Constant)
            {
                bool cbIndexable = config.UsedFeatures.HasFlag(Translation.FeatureFlags.CbIndexing);
                return OperandManager.GetConstantBufferName(operand.Value, offsetExpr, config.Stage, cbIndexable, indexElement);
            }
            else
            {
                string slotExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));
                return OperandManager.GetConstantBufferName(slotExpr, offsetExpr, config.Stage, indexElement);
            }
        }

        public static string LoadLocal(CodeGenContext context, AstOperation operation)
        {
            return LoadLocalOrShared(context, operation, DefaultNames.LocalMemoryName);
        }

        public static string LoadShared(CodeGenContext context, AstOperation operation)
        {
            return LoadLocalOrShared(context, operation, DefaultNames.SharedMemoryName);
        }

        private static string LoadLocalOrShared(CodeGenContext context, AstOperation operation, string arrayName)
        {
            IAstNode src1 = operation.GetSource(0);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));

            return $"{arrayName}[{offsetExpr}]";
        }

        public static string LoadStorage(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string indexExpr  = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));
            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            return GetStorageBufferAccessor(indexExpr, offsetExpr, context.Config.Stage);
        }

        public static string Lod(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            int coordsCount = texOp.Type.GetDimensions();

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;

            // TODO: Bindless texture support. For now we just return 0.
            if (isBindless)
            {
                return NumberFormatter.FormatFloat(0);
            }

            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;

            string indexExpr = null;

            if (isIndexed)
            {
                indexExpr = GetSoureExpr(context, texOp.GetSource(0), VariableType.S32);
            }

            string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

            int coordsIndex = isBindless || isIndexed ? 1 : 0;

            string coordsExpr;

            if (coordsCount > 1)
            {
                string[] elems = new string[coordsCount];

                for (int index = 0; index < coordsCount; index++)
                {
                    elems[index] = GetSoureExpr(context, texOp.GetSource(coordsIndex + index), VariableType.F32);
                }

                coordsExpr = "vec" + coordsCount + "(" + string.Join(", ", elems) + ")";
            }
            else
            {
                coordsExpr = GetSoureExpr(context, texOp.GetSource(coordsIndex), VariableType.F32);
            }

            return $"textureQueryLod({samplerName}, {coordsExpr}){GetMask(texOp.Index)}";
        }

        public static string StoreAttribute(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);
            IAstNode src3 = operation.GetSource(2);

            if (!(src1 is AstOperand baseAttr) || baseAttr.Type != OperandType.Constant)
            {
                throw new InvalidOperationException($"First input of {nameof(Instruction.StoreAttribute)} must be a constant operand.");
            }

            string attrName;

            if (src2 is AstOperand operand && operand.Type == OperandType.Constant)
            {
                int attrOffset = baseAttr.Value + (operand.Value << 2);
                attrName = OperandManager.GetAttributeName(attrOffset, context.Config, perPatch: false, isOutAttr: true);
            }
            else
            {
                string attrExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));
                attrExpr = Enclose(attrExpr, src2, Instruction.ShiftRightS32, isLhs: true);
                attrName = OperandManager.GetAttributeName(attrExpr, context.Config, isOutAttr: true);
            }

            string value = GetSoureExpr(context, src3, GetSrcVarType(operation.Inst, 2));
            return $"{attrName} = {value}";
        }

        public static string StoreLocal(CodeGenContext context, AstOperation operation)
        {
            return StoreLocalOrShared(context, operation, DefaultNames.LocalMemoryName);
        }

        public static string StoreShared(CodeGenContext context, AstOperation operation)
        {
            return StoreLocalOrShared(context, operation, DefaultNames.SharedMemoryName);
        }

        private static string StoreLocalOrShared(CodeGenContext context, AstOperation operation, string arrayName)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));

            VariableType srcType = OperandManager.GetNodeDestType(context, src2);

            string src = TypeConversion.ReinterpretCast(context, src2, srcType, VariableType.U32);

            return $"{arrayName}[{offsetExpr}] = {src}";
        }

        public static string StoreShared16(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));

            VariableType srcType = OperandManager.GetNodeDestType(context, src2);

            string src = TypeConversion.ReinterpretCast(context, src2, srcType, VariableType.U32);

            return $"{HelperFunctionNames.StoreShared16}({offsetExpr}, {src})";
        }

        public static string StoreShared8(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));

            VariableType srcType = OperandManager.GetNodeDestType(context, src2);

            string src = TypeConversion.ReinterpretCast(context, src2, srcType, VariableType.U32);

            return $"{HelperFunctionNames.StoreShared8}({offsetExpr}, {src})";
        }

        public static string StoreStorage(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);
            IAstNode src3 = operation.GetSource(2);

            string indexExpr  = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));
            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            VariableType srcType = OperandManager.GetNodeDestType(context, src3);

            string src = TypeConversion.ReinterpretCast(context, src3, srcType, VariableType.U32);

            string sb = GetStorageBufferAccessor(indexExpr, offsetExpr, context.Config.Stage);

            return $"{sb} = {src}";
        }

        public static string StoreStorage16(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);
            IAstNode src3 = operation.GetSource(2);

            string indexExpr  = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));
            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            VariableType srcType = OperandManager.GetNodeDestType(context, src3);

            string src = TypeConversion.ReinterpretCast(context, src3, srcType, VariableType.U32);

            string sb = GetStorageBufferAccessor(indexExpr, offsetExpr, context.Config.Stage);

            return $"{HelperFunctionNames.StoreStorage16}({indexExpr}, {offsetExpr}, {src})";
        }

        public static string StoreStorage8(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);
            IAstNode src3 = operation.GetSource(2);

            string indexExpr  = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));
            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            VariableType srcType = OperandManager.GetNodeDestType(context, src3);

            string src = TypeConversion.ReinterpretCast(context, src3, srcType, VariableType.U32);

            string sb = GetStorageBufferAccessor(indexExpr, offsetExpr, context.Config.Stage);

            return $"{HelperFunctionNames.StoreStorage8}({indexExpr}, {offsetExpr}, {src})";
        }

        public static string TextureSample(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            bool isBindless     = (texOp.Flags & TextureFlags.Bindless)    != 0;
            bool isGather       = (texOp.Flags & TextureFlags.Gather)      != 0;
            bool hasDerivatives = (texOp.Flags & TextureFlags.Derivatives) != 0;
            bool intCoords      = (texOp.Flags & TextureFlags.IntCoords)   != 0;
            bool hasLodBias     = (texOp.Flags & TextureFlags.LodBias)     != 0;
            bool hasLodLevel    = (texOp.Flags & TextureFlags.LodLevel)    != 0;
            bool hasOffset      = (texOp.Flags & TextureFlags.Offset)      != 0;
            bool hasOffsets     = (texOp.Flags & TextureFlags.Offsets)     != 0;

            bool isArray       = (texOp.Type & SamplerType.Array)       != 0;
            bool isIndexed     = (texOp.Type & SamplerType.Indexed)     != 0;
            bool isMultisample = (texOp.Type & SamplerType.Multisample) != 0;
            bool isShadow      = (texOp.Type & SamplerType.Shadow)      != 0;

            SamplerType type = texOp.Type & SamplerType.Mask;

            bool is2D   = type == SamplerType.Texture2D;
            bool isCube = type == SamplerType.TextureCube;

            // 2D Array and Cube shadow samplers with LOD level or bias requires an extension.
            // If the extension is not supported, just remove the LOD parameter.
            if (isArray && isShadow && (is2D || isCube) && !context.Config.GpuAccessor.QueryHostSupportsTextureShadowLod())
            {
                hasLodBias = false;
                hasLodLevel = false;
            }

            // Cube shadow samplers with LOD level requires an extension.
            // If the extension is not supported, just remove the LOD level parameter.
            if (isShadow && isCube && !context.Config.GpuAccessor.QueryHostSupportsTextureShadowLod())
            {
                hasLodLevel = false;
            }

            // TODO: Bindless texture support. For now we just return 0.
            if (isBindless)
            {
                return NumberFormatter.FormatFloat(0);
            }

            string texCall = intCoords ? "texelFetch" : "texture";

            if (isGather)
            {
                texCall += "Gather";
            }
            else if (hasDerivatives)
            {
                texCall += "Grad";
            }
            else if (hasLodLevel && !intCoords)
            {
                texCall += "Lod";
            }

            if (hasOffset)
            {
                texCall += "Offset";
            }
            else if (hasOffsets)
            {
                texCall += "Offsets";
            }

            int srcIndex = isBindless ? 1 : 0;

            string Src(VariableType type)
            {
                return GetSoureExpr(context, texOp.GetSource(srcIndex++), type);
            }

            string indexExpr = null;

            if (isIndexed)
            {
                indexExpr = Src(VariableType.S32);
            }

            string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

            texCall += "(" + samplerName;

            int coordsCount = texOp.Type.GetDimensions();

            int pCount = coordsCount;

            int arrayIndexElem = -1;

            if (isArray)
            {
                arrayIndexElem = pCount++;
            }

            // The sampler 1D shadow overload expects a
            // dummy value on the middle of the vector, who knows why...
            bool hasDummy1DShadowElem = texOp.Type == (SamplerType.Texture1D | SamplerType.Shadow);

            if (hasDummy1DShadowElem)
            {
                pCount++;
            }

            if (isShadow && !isGather)
            {
                pCount++;
            }

            // On textureGather*, the comparison value is
            // always specified as an extra argument.
            bool hasExtraCompareArg = isShadow && isGather;

            if (pCount == 5)
            {
                pCount = 4;

                hasExtraCompareArg = true;
            }

            void Append(string str)
            {
                texCall += ", " + str;
            }

            VariableType coordType = intCoords ? VariableType.S32 : VariableType.F32;

            string AssemblePVector(int count)
            {
                if (count > 1)
                {
                    string[] elems = new string[count];

                    for (int index = 0; index < count; index++)
                    {
                        if (arrayIndexElem == index)
                        {
                            elems[index] = Src(VariableType.S32);

                            if (!intCoords)
                            {
                                elems[index] = "float(" + elems[index] + ")";
                            }
                        }
                        else if (index == 1 && hasDummy1DShadowElem)
                        {
                            elems[index] = NumberFormatter.FormatFloat(0);
                        }
                        else
                        {
                            elems[index] = Src(coordType);
                        }
                    }

                    string prefix = intCoords ? "i" : string.Empty;

                    return prefix + "vec" + count + "(" + string.Join(", ", elems) + ")";
                }
                else
                {
                    return Src(coordType);
                }
            }

            string ApplyScaling(string vector)
            {
                if (intCoords)
                {
                    if ((context.Config.Stage.SupportsRenderScale()) &&
                        !isBindless &&
                        !isIndexed)
                    {
                        int index = context.FindTextureDescriptorIndex(texOp);

                        if (pCount == 3 && isArray)
                        {
                            // The array index is not scaled, just x and y.
                            vector = "ivec3(Helper_TexelFetchScale((" + vector + ").xy, " + index + "), (" + vector + ").z)";
                        }
                        else if (pCount == 2 && !isArray)
                        {
                            vector = "Helper_TexelFetchScale(" + vector + ", " + index + ")";
                        }
                    }
                }

                return vector;
            }

            Append(ApplyScaling(AssemblePVector(pCount)));

            string AssembleDerivativesVector(int count)
            {
                if (count > 1)
                {
                    string[] elems = new string[count];

                    for (int index = 0; index < count; index++)
                    {
                        elems[index] = Src(VariableType.F32);
                    }

                    return "vec" + count + "(" + string.Join(", ", elems) + ")";
                }
                else
                {
                    return Src(VariableType.F32);
                }
            }

            if (hasExtraCompareArg)
            {
                Append(Src(VariableType.F32));
            }

            if (hasDerivatives)
            {
                Append(AssembleDerivativesVector(coordsCount)); // dPdx
                Append(AssembleDerivativesVector(coordsCount)); // dPdy
            }

            if (isMultisample)
            {
                Append(Src(VariableType.S32));
            }
            else if (hasLodLevel)
            {
                Append(Src(coordType));
            }

            string AssembleOffsetVector(int count)
            {
                if (count > 1)
                {
                    string[] elems = new string[count];

                    for (int index = 0; index < count; index++)
                    {
                        elems[index] = Src(VariableType.S32);
                    }

                    return "ivec" + count + "(" + string.Join(", ", elems) + ")";
                }
                else
                {
                    return Src(VariableType.S32);
                }
            }

            if (hasOffset)
            {
                Append(AssembleOffsetVector(coordsCount));
            }
            else if (hasOffsets)
            {
                texCall += $", ivec{coordsCount}[4](";

                texCall += AssembleOffsetVector(coordsCount) + ", ";
                texCall += AssembleOffsetVector(coordsCount) + ", ";
                texCall += AssembleOffsetVector(coordsCount) + ", ";
                texCall += AssembleOffsetVector(coordsCount) + ")";
            }

            if (hasLodBias)
            {
               Append(Src(VariableType.F32));
            }

            // textureGather* optional extra component index,
            // not needed for shadow samplers.
            if (isGather && !isShadow)
            {
               Append(Src(VariableType.S32));
            }

            texCall += ")" + (isGather || !isShadow ? GetMask(texOp.Index) : "");

            return texCall;
        }

        public static string TextureSize(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;

            // TODO: Bindless texture support. For now we just return 0.
            if (isBindless)
            {
                return NumberFormatter.FormatInt(0);
            }

            bool isIndexed = (texOp.Type & SamplerType.Indexed) != 0;

            string indexExpr = null;

            if (isIndexed)
            {
                indexExpr = GetSoureExpr(context, texOp.GetSource(0), VariableType.S32);
            }

            string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

            if (texOp.Index == 3)
            {
                return $"textureQueryLevels({samplerName})";
            }
            else
            {
                (TextureDescriptor descriptor, int descriptorIndex) = context.FindTextureDescriptor(texOp);
                bool hasLod = !descriptor.Type.HasFlag(SamplerType.Multisample) && descriptor.Type != SamplerType.TextureBuffer;
                string texCall;

                if (hasLod)
                {
                    int lodSrcIndex = isBindless || isIndexed ? 1 : 0;
                    IAstNode lod = operation.GetSource(lodSrcIndex);
                    string lodExpr = GetSoureExpr(context, lod, GetSrcVarType(operation.Inst, lodSrcIndex));

                    texCall = $"textureSize({samplerName}, {lodExpr}){GetMask(texOp.Index)}";
                }
                else
                {
                    texCall = $"textureSize({samplerName}){GetMask(texOp.Index)}";
                }

                if (context.Config.Stage.SupportsRenderScale() &&
                    !isBindless &&
                    !isIndexed)
                {
                    texCall = $"Helper_TextureSizeUnscale({texCall}, {descriptorIndex})";
                }

                return texCall;
            }
        }

        private static string GetStorageBufferAccessor(string slotExpr, string offsetExpr, ShaderStage stage)
        {
            string sbName = OperandManager.GetShaderStagePrefix(stage);

            sbName += "_" + DefaultNames.StorageNamePrefix;

            return $"{sbName}[{slotExpr}].{DefaultNames.DataName}[{offsetExpr}]";
        }

        private static string GetMask(int index)
        {
            return '.' + "rgba".Substring(index, 1);
        }
    }
}