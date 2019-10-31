using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions.InstGenHelper;
using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenMemory
    {
        public static string ImageStore(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            bool isBindless = (texOp.Flags & TextureFlags.Bindless) != 0;

            bool isArray = (texOp.Type & SamplerType.Array) != 0;

            string texCall = "imageStore";

            string imageName = OperandManager.GetImageName(context.Config.Stage, texOp);

            texCall += "(" + imageName;

            int coordsCount = texOp.Type.GetDimensions();

            int pCount = coordsCount;

            int arrayIndexElem = -1;

            if (isArray)
            {
                arrayIndexElem = pCount++;
            }

            int srcIndex = isBindless ? 1 : 0;

            string Src(VariableType type)
            {
                return GetSoureExpr(context, texOp.GetSource(srcIndex++), type);
            }

            void Append(string str)
            {
                texCall += ", " + str;
            }

            if (pCount > 1)
            {
                string[] elems = new string[pCount];

                for (int index = 0; index < pCount; index++)
                {
                    elems[index] = Src(VariableType.S32);
                }

                Append("ivec" + pCount + "(" + string.Join(", ", elems) + ")");
            }
            else
            {
                Append(Src(VariableType.S32));
            }

            string[] cElems = new string[4];

            for (int index = 0; index < 4; index++)
            {
                if (srcIndex < texOp.SourcesCount)
                {
                    cElems[index] = Src(VariableType.F32);
                }
                else
                {
                    cElems[index] = NumberFormatter.FormatFloat(0);
                }
            }

            Append("vec4(" + string.Join(", ", cElems) + ")");

            texCall += ")";

            return texCall;
        }

        public static string LoadAttribute(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            if (!(src1 is AstOperand attr) || attr.Type != OperandType.Attribute)
            {
                throw new InvalidOperationException("First source of LoadAttribute must be a attribute.");
            }

            string indexExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            return OperandManager.GetAttributeName(attr, context.Config.Stage, isOutAttr: false, indexExpr);
        }

        public static string LoadConstant(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            offsetExpr = Enclose(offsetExpr, src2, Instruction.ShiftRightS32, isLhs: true);

            return OperandManager.GetConstantBufferName(src1, offsetExpr, context.Config.Stage);
        }

        public static string LoadLocal(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));

            return $"{DefaultNames.LocalMemoryName}[{offsetExpr}]";
        }

        public static string LoadStorage(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            return OperandManager.GetStorageBufferName(src1, offsetExpr, context.Config.Stage);
        }

        public static string StoreLocal(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 0));

            VariableType srcType = OperandManager.GetNodeDestType(src2);

            string src = TypeConversion.ReinterpretCast(context, src2, srcType, VariableType.F32);

            return $"{DefaultNames.LocalMemoryName}[{offsetExpr}] = {src}";
        }

        public static string StoreStorage(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(0);
            IAstNode src2 = operation.GetSource(1);
            IAstNode src3 = operation.GetSource(2);

            string offsetExpr = GetSoureExpr(context, src2, GetSrcVarType(operation.Inst, 1));

            VariableType srcType = OperandManager.GetNodeDestType(src3);

            string src = TypeConversion.ReinterpretCast(context, src3, srcType, VariableType.F32);

            string sbName = OperandManager.GetStorageBufferName(src1, offsetExpr, context.Config.Stage);

            return $"{sbName} = {src}";
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
            bool isMultisample = (texOp.Type & SamplerType.Multisample) != 0;
            bool isShadow      = (texOp.Type & SamplerType.Shadow)      != 0;

            // This combination is valid, but not available on GLSL.
            // For now, ignore the LOD level and do a normal sample.
            // TODO: How to implement it properly?
            if (hasLodLevel && isArray && isShadow)
            {
                hasLodLevel = false;
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

            string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp);

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

            int srcIndex = isBindless ? 1 : 0;

            string Src(VariableType type)
            {
                return GetSoureExpr(context, texOp.GetSource(srcIndex++), type);
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

            Append(AssemblePVector(pCount));

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

            if (hasDerivatives)
            {
                Append(AssembleDerivativesVector(coordsCount)); // dPdx
                Append(AssembleDerivativesVector(coordsCount)); // dPdy
            }

            if (hasExtraCompareArg)
            {
                Append(Src(VariableType.F32));
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

            texCall += ")" + (isGather || !isShadow ? GetMask(texOp.ComponentMask) : "");

            return texCall;
        }

        public static string TextureSize(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            bool isBindless  = (texOp.Flags & TextureFlags.Bindless) != 0;

            string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp);

            IAstNode src0 = operation.GetSource(isBindless ? 1 : 0);

            string src0Expr = GetSoureExpr(context, src0, GetSrcVarType(operation.Inst, 0));

            return $"textureSize({samplerName}, {src0Expr}){GetMask(texOp.ComponentMask)}";
        }

        private static string GetMask(int compMask)
        {
            string mask = ".";

            for (int index = 0; index < 4; index++)
            {
                if ((compMask & (1 << index)) != 0)
                {
                    mask += "rgba".Substring(index, 1);
                }
            }

            return mask;
        }
    }
}