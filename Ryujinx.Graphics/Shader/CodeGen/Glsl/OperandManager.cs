using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    class OperandManager
    {
        private static string[] _stagePrefixes = new string[] { "vp", "tcp", "tep", "gp", "fp" };

        private struct BuiltInAttribute
        {
            public string Name { get; }

            public VariableType Type { get; }

            public BuiltInAttribute(string name, VariableType type)
            {
                Name = name;
                Type = type;
            }
        }

        private static Dictionary<int, BuiltInAttribute> _builtInAttributes =
                   new Dictionary<int, BuiltInAttribute>()
        {
            { AttributeConsts.Layer,               new BuiltInAttribute("gl_Layer",           VariableType.S32)  },
            { AttributeConsts.PointSize,           new BuiltInAttribute("gl_PointSize",       VariableType.F32)  },
            { AttributeConsts.PositionX,           new BuiltInAttribute("gl_Position.x",      VariableType.F32)  },
            { AttributeConsts.PositionY,           new BuiltInAttribute("gl_Position.y",      VariableType.F32)  },
            { AttributeConsts.PositionZ,           new BuiltInAttribute("gl_Position.z",      VariableType.F32)  },
            { AttributeConsts.PositionW,           new BuiltInAttribute("gl_Position.w",      VariableType.F32)  },
            { AttributeConsts.ClipDistance0,       new BuiltInAttribute("gl_ClipDistance[0]", VariableType.F32)  },
            { AttributeConsts.ClipDistance1,       new BuiltInAttribute("gl_ClipDistance[1]", VariableType.F32)  },
            { AttributeConsts.ClipDistance2,       new BuiltInAttribute("gl_ClipDistance[2]", VariableType.F32)  },
            { AttributeConsts.ClipDistance3,       new BuiltInAttribute("gl_ClipDistance[3]", VariableType.F32)  },
            { AttributeConsts.ClipDistance4,       new BuiltInAttribute("gl_ClipDistance[4]", VariableType.F32)  },
            { AttributeConsts.ClipDistance5,       new BuiltInAttribute("gl_ClipDistance[5]", VariableType.F32)  },
            { AttributeConsts.ClipDistance6,       new BuiltInAttribute("gl_ClipDistance[6]", VariableType.F32)  },
            { AttributeConsts.ClipDistance7,       new BuiltInAttribute("gl_ClipDistance[7]", VariableType.F32)  },
            { AttributeConsts.PointCoordX,         new BuiltInAttribute("gl_PointCoord.x",    VariableType.F32)  },
            { AttributeConsts.PointCoordY,         new BuiltInAttribute("gl_PointCoord.y",    VariableType.F32)  },
            { AttributeConsts.TessCoordX,          new BuiltInAttribute("gl_TessCoord.x",     VariableType.F32)  },
            { AttributeConsts.TessCoordY,          new BuiltInAttribute("gl_TessCoord.y",     VariableType.F32)  },
            { AttributeConsts.InstanceId,          new BuiltInAttribute("instance",           VariableType.S32)  },
            { AttributeConsts.VertexId,            new BuiltInAttribute("gl_VertexID",        VariableType.S32)  },
            { AttributeConsts.FrontFacing,         new BuiltInAttribute("gl_FrontFacing",     VariableType.Bool) },
            { AttributeConsts.FragmentOutputDepth, new BuiltInAttribute("gl_FragDepth",       VariableType.F32)  }
        };

        private Dictionary<AstOperand, string> _locals;

        public OperandManager()
        {
            _locals = new Dictionary<AstOperand, string>();
        }

        public string DeclareLocal(AstOperand operand)
        {
            string name = $"{DefaultNames.LocalNamePrefix}_{_locals.Count}";

            _locals.Add(operand, name);

            return name;
        }

        public string GetExpression(AstOperand operand, GalShaderType shaderType)
        {
            switch (operand.Type)
            {
                case OperandType.Attribute:
                    return GetAttributeName(operand, shaderType);

                case OperandType.Constant:
                    return NumberFormatter.FormatInt(operand.Value);

                case OperandType.ConstantBuffer:
                    return GetConstantBufferName(operand, shaderType);

                case OperandType.LocalVariable:
                    return _locals[operand];

                case OperandType.Undefined:
                    return DefaultNames.UndefinedName;
            }

            throw new ArgumentException($"Invalid operand type \"{operand.Type}\".");
        }

        public static string GetConstantBufferName(AstOperand cbuf, GalShaderType shaderType)
        {
            string ubName = GetUbName(shaderType, cbuf.CbufSlot);

            ubName += "[" + (cbuf.CbufOffset >> 2) + "]";

            return ubName + "." + GetSwizzleMask(cbuf.CbufOffset & 3);
        }

        public static string GetConstantBufferName(IAstNode slot, string offsetExpr, GalShaderType shaderType)
        {
            // Non-constant slots are not supported.
            // It is expected that upstream stages are never going to generate non-constant
            // slot access.
            AstOperand operand = (AstOperand)slot;

            string ubName = GetUbName(shaderType, operand.Value);

            string index0 = "[" + offsetExpr + " >> 4]";
            string index1 = "[" + offsetExpr + " >> 2 & 3]";

            return ubName + index0 + index1;
        }

        public static string GetOutAttributeName(AstOperand attr, GalShaderType shaderType)
        {
            return GetAttributeName(attr, shaderType, isOutAttr: true);
        }

        private static string GetAttributeName(AstOperand attr, GalShaderType shaderType, bool isOutAttr = false)
        {
            int value = attr.Value;

            string swzMask = GetSwizzleMask((value >> 2) & 3);

            if (value >= AttributeConsts.UserAttributeBase &&
                value <  AttributeConsts.UserAttributeEnd)
            {
                value -= AttributeConsts.UserAttributeBase;

                string prefix = isOutAttr
                    ? DefaultNames.OAttributePrefix
                    : DefaultNames.IAttributePrefix;

                string name = $"{prefix}{(value >> 4)}";

                if (shaderType == GalShaderType.Geometry && !isOutAttr)
                {
                    name += "[0]";
                }

                name += "." + swzMask;

                return name;
            }
            else
            {
                if (value >= AttributeConsts.FragmentOutputColorBase &&
                    value <  AttributeConsts.FragmentOutputColorEnd)
                {
                    value -= AttributeConsts.FragmentOutputColorBase;

                    return $"{DefaultNames.OAttributePrefix}{(value >> 4)}.{swzMask}";
                }
                else if (_builtInAttributes.TryGetValue(value & ~3, out BuiltInAttribute builtInAttr))
                {
                    // TODO: There must be a better way to handle this...
                    if (shaderType == GalShaderType.Fragment)
                    {
                        switch (value & ~3)
                        {
                            case AttributeConsts.PositionX: return "gl_FragCoord.x";
                            case AttributeConsts.PositionY: return "gl_FragCoord.y";
                            case AttributeConsts.PositionZ: return "gl_FragCoord.z";
                            case AttributeConsts.PositionW: return "1.0";
                        }
                    }

                    string name = builtInAttr.Name;

                    if (shaderType == GalShaderType.Geometry && !isOutAttr)
                    {
                        name = "gl_in[0]." + name;
                    }

                    return name;
                }
            }

            // TODO: Warn about unknown built-in attribute.

            return isOutAttr ? "// bad_attr0x" + value.ToString("X") : "0.0";
        }

        public static string GetUbName(GalShaderType shaderType, int slot)
        {
            string ubName = GetShaderStagePrefix(shaderType);

            ubName += "_" + DefaultNames.UniformNamePrefix + slot;

            return ubName + "_" + DefaultNames.UniformNameSuffix;
        }

        public static string GetSamplerName(GalShaderType shaderType, AstTextureOperation texOp)
        {
            string suffix;

            if ((texOp.Flags & TextureFlags.Bindless) != 0)
            {
                AstOperand operand = texOp.GetSource(0) as AstOperand;

                suffix = "_cb" + operand.CbufSlot + "_" + operand.CbufOffset;
            }
            else
            {
                suffix = (texOp.Handle - 8).ToString();
            }

            return GetShaderStagePrefix(shaderType) + "_" + DefaultNames.SamplerNamePrefix + suffix;
        }

        public static string GetShaderStagePrefix(GalShaderType shaderType)
        {
            return _stagePrefixes[(int)shaderType];
        }

        private static string GetSwizzleMask(int value)
        {
            return "xyzw".Substring(value, 1);
        }

        public static VariableType GetNodeDestType(IAstNode node)
        {
            if (node is AstOperation operation)
            {
                return GetDestVarType(operation.Inst);
            }
            else if (node is AstOperand operand)
            {
                if (operand.Type == OperandType.Attribute)
                {
                    if (_builtInAttributes.TryGetValue(operand.Value & ~3, out BuiltInAttribute builtInAttr))
                    {
                        return builtInAttr.Type;
                    }
                }

                return OperandInfo.GetVarType(operand);
            }
            else
            {
                throw new ArgumentException($"Invalid node type \"{node?.GetType().Name ?? "null"}\".");
            }
        }
    }
}