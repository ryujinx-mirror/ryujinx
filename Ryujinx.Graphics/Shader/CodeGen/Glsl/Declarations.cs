using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class Declarations
    {
        public static void Declare(CodeGenContext context, StructuredProgramInfo info)
        {
            context.AppendLine("#version 420 core");

            context.AppendLine();

            context.AppendLine($"const int {DefaultNames.UndefinedName} = 0;");

            context.AppendLine();

            if (context.Config.Type == GalShaderType.Geometry)
            {
                context.AppendLine("layout (points) in;");
                context.AppendLine("layout (triangle_strip, max_vertices = 4) out;");

                context.AppendLine();
            }

            context.AppendLine("layout (std140) uniform Extra");

            context.EnterScope();

            context.AppendLine("vec2 flip;");
            context.AppendLine("int instance;");

            context.LeaveScope(";");

            context.AppendLine();

            if (info.CBuffers.Count != 0)
            {
                DeclareUniforms(context, info);

                context.AppendLine();
            }

            if (info.Samplers.Count != 0)
            {
                DeclareSamplers(context, info);

                context.AppendLine();
            }

            if (info.IAttributes.Count != 0)
            {
                DeclareInputAttributes(context, info);

                context.AppendLine();
            }

            if (info.OAttributes.Count != 0)
            {
                DeclareOutputAttributes(context, info);

                context.AppendLine();
            }
        }

        public static void DeclareLocals(CodeGenContext context, StructuredProgramInfo info)
        {
            foreach (AstOperand decl in info.Locals)
            {
                string name = context.OperandManager.DeclareLocal(decl);

                context.AppendLine(GetVarTypeName(decl.VarType) + " " + name + ";");
            }
        }

        private static string GetVarTypeName(VariableType type)
        {
            switch (type)
            {
                case VariableType.Bool: return "bool";
                case VariableType.F32:  return "float";
                case VariableType.S32:  return "int";
                case VariableType.U32:  return "uint";
            }

            throw new ArgumentException($"Invalid variable type \"{type}\".");
        }

        private static void DeclareUniforms(CodeGenContext context, StructuredProgramInfo info)
        {
            foreach (int cbufSlot in info.CBuffers.OrderBy(x => x))
            {
                string ubName = OperandManager.GetShaderStagePrefix(context.Config.Type);

                ubName += "_" + DefaultNames.UniformNamePrefix + cbufSlot;

                context.CBufferDescriptors.Add(new CBufferDescriptor(ubName, cbufSlot));

                context.AppendLine("layout (std140) uniform " + ubName);

                context.EnterScope();

                string ubSize = "[" + NumberFormatter.FormatInt(context.Config.MaxCBufferSize / 16) + "]";

                context.AppendLine("vec4 " + OperandManager.GetUbName(context.Config.Type, cbufSlot) + ubSize + ";");

                context.LeaveScope(";");
            }
        }

        private static void DeclareSamplers(CodeGenContext context, StructuredProgramInfo info)
        {
            Dictionary<string, AstTextureOperation> samplers = new Dictionary<string, AstTextureOperation>();

            foreach (AstTextureOperation texOp in info.Samplers.OrderBy(x => x.Handle))
            {
                string samplerName = OperandManager.GetSamplerName(context.Config.Type, texOp);

                if (!samplers.TryAdd(samplerName, texOp))
                {
                    continue;
                }

                string samplerTypeName = GetSamplerTypeName(texOp.Type);

                context.AppendLine("uniform " + samplerTypeName + " " + samplerName + ";");
            }

            foreach (KeyValuePair<string, AstTextureOperation> kv in samplers)
            {
                string samplerName = kv.Key;

                AstTextureOperation texOp = kv.Value;

                TextureDescriptor desc;

                if ((texOp.Flags & TextureFlags.Bindless) != 0)
                {
                    AstOperand operand = texOp.GetSource(0) as AstOperand;

                    desc = new TextureDescriptor(samplerName, operand.CbufSlot, operand.CbufOffset);
                }
                else
                {
                    desc = new TextureDescriptor(samplerName, texOp.Handle);
                }

                context.TextureDescriptors.Add(desc);
            }
        }

        private static void DeclareInputAttributes(CodeGenContext context, StructuredProgramInfo info)
        {
            string suffix = context.Config.Type == GalShaderType.Geometry ? "[]" : string.Empty;

            foreach (int attr in info.IAttributes.OrderBy(x => x))
            {
                context.AppendLine($"layout (location = {attr}) in vec4 {DefaultNames.IAttributePrefix}{attr}{suffix};");
            }
        }

        private static void DeclareOutputAttributes(CodeGenContext context, StructuredProgramInfo info)
        {
            foreach (int attr in info.OAttributes.OrderBy(x => x))
            {
                context.AppendLine($"layout (location = {attr}) out vec4 {DefaultNames.OAttributePrefix}{attr};");
            }
        }

        private static string GetSamplerTypeName(TextureType type)
        {
            string typeName;

            switch (type & TextureType.Mask)
            {
                case TextureType.Texture1D:   typeName = "sampler1D";   break;
                case TextureType.Texture2D:   typeName = "sampler2D";   break;
                case TextureType.Texture3D:   typeName = "sampler3D";   break;
                case TextureType.TextureCube: typeName = "samplerCube"; break;

                default: throw new ArgumentException($"Invalid sampler type \"{type}\".");
            }

            if ((type & TextureType.Multisample) != 0)
            {
                typeName += "MS";
            }

            if ((type & TextureType.Array) != 0)
            {
                typeName += "Array";
            }

            if ((type & TextureType.Shadow) != 0)
            {
                typeName += "Shadow";
            }

            return typeName;
        }
    }
}