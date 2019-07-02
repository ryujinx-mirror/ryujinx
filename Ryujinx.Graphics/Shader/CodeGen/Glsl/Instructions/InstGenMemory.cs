using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions.InstGenHelper;
using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenMemory
    {
        public static string LoadConstant(CodeGenContext context, AstOperation operation)
        {
            IAstNode src1 = operation.GetSource(1);

            string offsetExpr = GetSoureExpr(context, src1, GetSrcVarType(operation.Inst, 1));

            offsetExpr = Enclose(offsetExpr, src1, Instruction.ShiftRightS32, isLhs: true);

            return OperandManager.GetConstantBufferName(operation.GetSource(0), offsetExpr, context.Config.Type);
        }

        public static string TextureSample(CodeGenContext context, AstOperation operation)
        {
            AstTextureOperation texOp = (AstTextureOperation)operation;

            bool isBindless    = (texOp.Flags & TextureFlags.Bindless)   != 0;
            bool isGather      = (texOp.Flags & TextureFlags.Gather)     != 0;
            bool intCoords     = (texOp.Flags & TextureFlags.IntCoords)  != 0;
            bool hasLodBias    = (texOp.Flags & TextureFlags.LodBias)    != 0;
            bool hasLodLevel   = (texOp.Flags & TextureFlags.LodLevel)   != 0;
            bool hasOffset     = (texOp.Flags & TextureFlags.Offset)     != 0;
            bool hasOffsets    = (texOp.Flags & TextureFlags.Offsets)    != 0;
            bool isArray       = (texOp.Type  & TextureType.Array)       != 0;
            bool isMultisample = (texOp.Type  & TextureType.Multisample) != 0;
            bool isShadow      = (texOp.Type  & TextureType.Shadow)      != 0;

            string texCall = intCoords ? "texelFetch" : "texture";

            if (isGather)
            {
                texCall += "Gather";
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

            string samplerName = OperandManager.GetSamplerName(context.Config.Type, texOp);

            texCall += "(" + samplerName;

            int coordsCount = texOp.Type.GetCoordsCount();

            int pCount = coordsCount;

            int arrayIndexElem = -1;

            if (isArray)
            {
                arrayIndexElem = pCount++;
            }

            // The sampler 1D shadow overload expects a
            // dummy value on the middle of the vector, who knows why...
            bool hasDummy1DShadowElem = texOp.Type == (TextureType.Texture1D | TextureType.Shadow);

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

            string samplerName = OperandManager.GetSamplerName(context.Config.Type, texOp);

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