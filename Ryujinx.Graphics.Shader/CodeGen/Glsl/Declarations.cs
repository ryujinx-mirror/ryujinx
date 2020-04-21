using Ryujinx.Common;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class Declarations
    {
        // At least 16 attributes are guaranteed by the spec.
        public const int MaxAttributes = 16;

        public static void Declare(CodeGenContext context, StructuredProgramInfo info)
        {
            context.AppendLine("#version 430 core");
            context.AppendLine("#extension GL_ARB_gpu_shader_int64 : enable");
            context.AppendLine("#extension GL_ARB_shader_ballot : enable");
            context.AppendLine("#extension GL_ARB_shader_group_vote : enable");

            if (context.Config.Stage == ShaderStage.Compute)
            {
                context.AppendLine("#extension GL_ARB_compute_shader : enable");
            }

            context.AppendLine("#pragma optionNV(fastmath off)");

            context.AppendLine();

            context.AppendLine($"const int {DefaultNames.UndefinedName} = 0;");
            context.AppendLine();

            if (context.Config.Stage == ShaderStage.Geometry)
            {
                string inPrimitive = ((InputTopology)context.Config.QueryInfo(QueryInfoName.PrimitiveTopology)).ToGlslString();

                context.AppendLine($"layout ({inPrimitive}) in;");

                string outPrimitive = context.Config.OutputTopology.ToGlslString();

                int maxOutputVertices = context.Config.MaxOutputVertices;

                context.AppendLine($"layout ({outPrimitive}, max_vertices = {maxOutputVertices}) out;");
                context.AppendLine();
            }

            if (context.Config.Stage == ShaderStage.Compute)
            {
                int localMemorySize = BitUtils.DivRoundUp(context.Config.QueryInfo(QueryInfoName.ComputeLocalMemorySize), 4);

                if (localMemorySize != 0)
                {
                    string localMemorySizeStr = NumberFormatter.FormatInt(localMemorySize);

                    context.AppendLine($"uint {DefaultNames.LocalMemoryName}[{localMemorySizeStr}];");
                    context.AppendLine();
                }

                int sharedMemorySize = BitUtils.DivRoundUp(context.Config.QueryInfo(QueryInfoName.ComputeSharedMemorySize), 4);

                if (sharedMemorySize != 0)
                {
                    string sharedMemorySizeStr = NumberFormatter.FormatInt(sharedMemorySize);

                    context.AppendLine($"shared uint {DefaultNames.SharedMemoryName}[{sharedMemorySizeStr}];");
                    context.AppendLine();
                }
            }
            else if (context.Config.LocalMemorySize != 0)
            {
                int localMemorySize = BitUtils.DivRoundUp(context.Config.LocalMemorySize, 4);

                string localMemorySizeStr = NumberFormatter.FormatInt(localMemorySize);

                context.AppendLine($"uint {DefaultNames.LocalMemoryName}[{localMemorySizeStr}];");
                context.AppendLine();
            }

            if (info.CBuffers.Count != 0)
            {
                DeclareUniforms(context, info);

                context.AppendLine();
            }

            if (info.SBuffers.Count != 0)
            {
                DeclareStorages(context, info);

                context.AppendLine();
            }

            if (info.Samplers.Count != 0)
            {
                DeclareSamplers(context, info);

                context.AppendLine();
            }

            if (info.Images.Count != 0)
            {
                DeclareImages(context, info);

                context.AppendLine();
            }

            if (context.Config.Stage != ShaderStage.Compute)
            {
                if (info.IAttributes.Count != 0)
                {
                    DeclareInputAttributes(context, info);

                    context.AppendLine();
                }

                if (info.OAttributes.Count != 0 || context.Config.Stage != ShaderStage.Fragment)
                {
                    DeclareOutputAttributes(context, info);

                    context.AppendLine();
                }
            }
            else
            {
                string localSizeX = NumberFormatter.FormatInt(context.Config.QueryInfo(QueryInfoName.ComputeLocalSizeX));
                string localSizeY = NumberFormatter.FormatInt(context.Config.QueryInfo(QueryInfoName.ComputeLocalSizeY));
                string localSizeZ = NumberFormatter.FormatInt(context.Config.QueryInfo(QueryInfoName.ComputeLocalSizeZ));

                context.AppendLine(
                    "layout (" +
                    $"local_size_x = {localSizeX}, " +
                    $"local_size_y = {localSizeY}, " +
                    $"local_size_z = {localSizeZ}) in;");
                context.AppendLine();
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.MultiplyHighS32) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/MultiplyHighS32.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.MultiplyHighU32) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/MultiplyHighU32.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.Shuffle) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/Shuffle.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.ShuffleDown) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/ShuffleDown.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.ShuffleUp) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/ShuffleUp.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.ShuffleXor) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/ShuffleXor.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.SwizzleAdd) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/SwizzleAdd.glsl");
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
                case VariableType.F32:  return "precise float";
                case VariableType.F64:  return "double";
                case VariableType.S32:  return "int";
                case VariableType.U32:  return "uint";
            }

            throw new ArgumentException($"Invalid variable type \"{type}\".");
        }

        private static void DeclareUniforms(CodeGenContext context, StructuredProgramInfo info)
        {
            foreach (int cbufSlot in info.CBuffers.OrderBy(x => x))
            {
                string ubName = OperandManager.GetShaderStagePrefix(context.Config.Stage);

                ubName += "_" + DefaultNames.UniformNamePrefix + cbufSlot;

                context.CBufferDescriptors.Add(new BufferDescriptor(ubName, cbufSlot));

                context.AppendLine("layout (std140) uniform " + ubName);

                context.EnterScope();

                string ubSize = "[" + NumberFormatter.FormatInt(Constants.ConstantBufferSize / 16) + "]";

                context.AppendLine("vec4 " + OperandManager.GetUbName(context.Config.Stage, cbufSlot) + ubSize + ";");

                context.LeaveScope(";");
            }
        }

        private static void DeclareStorages(CodeGenContext context, StructuredProgramInfo info)
        {
            string sbName = OperandManager.GetShaderStagePrefix(context.Config.Stage);

            sbName += "_" + DefaultNames.StorageNamePrefix;

            string blockName = $"{sbName}_{DefaultNames.BlockSuffix}";

            int maxSlot = 0;

            foreach (int sbufSlot in info.SBuffers)
            {
                context.SBufferDescriptors.Add(new BufferDescriptor($"{blockName}[{sbufSlot}]", sbufSlot));

                if (maxSlot < sbufSlot)
                {
                    maxSlot = sbufSlot;
                }
            }

            context.AppendLine("layout (std430) buffer " + blockName);

            context.EnterScope();

            context.AppendLine("uint " + DefaultNames.DataName + "[];");

            string arraySize = NumberFormatter.FormatInt(maxSlot + 1);

            context.LeaveScope($" {sbName}[{arraySize}];");
        }

        private static void DeclareSamplers(CodeGenContext context, StructuredProgramInfo info)
        {
            Dictionary<string, AstTextureOperation> samplers = new Dictionary<string, AstTextureOperation>();

            // Texture instructions other than TextureSample (like TextureSize)
            // may have incomplete sampler type information. In those cases,
            // we prefer instead the more accurate information from the
            // TextureSample instruction, if both are available.
            foreach (AstTextureOperation texOp in info.Samplers.OrderBy(x => x.Handle * 2 + (x.Inst == Instruction.TextureSample ? 0 : 1)))
            {
                string indexExpr = NumberFormatter.FormatInt(texOp.ArraySize);

                string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

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

                    desc = new TextureDescriptor(samplerName, texOp.Type, operand.CbufSlot, operand.CbufOffset);

                    context.TextureDescriptors.Add(desc);
                }
                else if ((texOp.Type & SamplerType.Indexed) != 0)
                {
                    for (int index = 0; index < texOp.ArraySize; index++)
                    {
                        string indexExpr = NumberFormatter.FormatInt(index);

                        string indexedSamplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

                        desc = new TextureDescriptor(indexedSamplerName, texOp.Type, texOp.Handle + index * 2);

                        context.TextureDescriptors.Add(desc);
                    }
                }
                else
                {
                    desc = new TextureDescriptor(samplerName, texOp.Type, texOp.Handle);

                    context.TextureDescriptors.Add(desc);
                }
            }
        }

        private static void DeclareImages(CodeGenContext context, StructuredProgramInfo info)
        {
            Dictionary<string, AstTextureOperation> images = new Dictionary<string, AstTextureOperation>();

            foreach (AstTextureOperation texOp in info.Images.OrderBy(x => x.Handle))
            {
                string indexExpr = NumberFormatter.FormatInt(texOp.ArraySize);

                string imageName = OperandManager.GetImageName(context.Config.Stage, texOp, indexExpr);

                if (!images.TryAdd(imageName, texOp))
                {
                    continue;
                }

                string layout = texOp.Format.ToGlslFormat();

                if (!string.IsNullOrEmpty(layout))
                {
                    layout = "layout(" + layout + ") ";
                }

                string imageTypeName = GetImageTypeName(texOp.Type, texOp.Format.GetComponentType());

                context.AppendLine("uniform " + layout + imageTypeName + " " + imageName + ";");
            }

            foreach (KeyValuePair<string, AstTextureOperation> kv in images)
            {
                string imageName = kv.Key;

                AstTextureOperation texOp = kv.Value;

                if ((texOp.Type & SamplerType.Indexed) != 0)
                {
                    for (int index = 0; index < texOp.ArraySize; index++)
                    {
                        string indexExpr = NumberFormatter.FormatInt(index);

                        string indexedSamplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

                        var desc = new TextureDescriptor(indexedSamplerName, texOp.Type, texOp.Handle + index * 2);

                        context.TextureDescriptors.Add(desc);
                    }
                }
                else
                {
                    var desc = new TextureDescriptor(imageName, texOp.Type, texOp.Handle);

                    context.ImageDescriptors.Add(desc);
                }
            }
        }

        private static void DeclareInputAttributes(CodeGenContext context, StructuredProgramInfo info)
        {
            string suffix = context.Config.Stage == ShaderStage.Geometry ? "[]" : string.Empty;

            foreach (int attr in info.IAttributes.OrderBy(x => x))
            {
                string iq = string.Empty;

                if (context.Config.Stage == ShaderStage.Fragment)
                {
                    iq = context.Config.ImapTypes[attr].GetFirstUsedType() switch
                    {
                        PixelImap.Constant => "flat ",
                        PixelImap.ScreenLinear => "noperspective ",
                        _ => string.Empty
                    };
                }

                context.AppendLine($"layout (location = {attr}) {iq}in vec4 {DefaultNames.IAttributePrefix}{attr}{suffix};");
            }
        }

        private static void DeclareOutputAttributes(CodeGenContext context, StructuredProgramInfo info)
        {
            if (context.Config.Stage == ShaderStage.Fragment)
            {
                DeclareUsedOutputAttributes(context, info);
            }
            else
            {
                DeclareAllOutputAttributes(context, info);
            }
        }

        private static void DeclareUsedOutputAttributes(CodeGenContext context, StructuredProgramInfo info)
        {
            foreach (int attr in info.OAttributes.OrderBy(x => x))
            {
                context.AppendLine($"layout (location = {attr}) out vec4 {DefaultNames.OAttributePrefix}{attr};");
            }
        }

        private static void DeclareAllOutputAttributes(CodeGenContext context, StructuredProgramInfo info)
        {
            for (int attr = 0; attr < MaxAttributes; attr++)
            {
                context.AppendLine($"layout (location = {attr}) out vec4 {DefaultNames.OAttributePrefix}{attr};");
            }

            foreach (int attr in info.OAttributes.OrderBy(x => x).Where(x => x >= MaxAttributes))
            {
                context.AppendLine($"layout (location = {attr}) out vec4 {DefaultNames.OAttributePrefix}{attr};");
            }
        }

        private static void AppendHelperFunction(CodeGenContext context, string filename)
        {
            string code = EmbeddedResources.ReadAllText(filename);

            context.AppendLine(code.Replace("\t", CodeGenContext.Tab));
            context.AppendLine();
        }

        private static string GetSamplerTypeName(SamplerType type)
        {
            string typeName;

            switch (type & SamplerType.Mask)
            {
                case SamplerType.Texture1D:     typeName = "sampler1D";     break;
                case SamplerType.TextureBuffer: typeName = "samplerBuffer"; break;
                case SamplerType.Texture2D:     typeName = "sampler2D";     break;
                case SamplerType.Texture3D:     typeName = "sampler3D";     break;
                case SamplerType.TextureCube:   typeName = "samplerCube";   break;

                default: throw new ArgumentException($"Invalid sampler type \"{type}\".");
            }

            if ((type & SamplerType.Multisample) != 0)
            {
                typeName += "MS";
            }

            if ((type & SamplerType.Array) != 0)
            {
                typeName += "Array";
            }

            if ((type & SamplerType.Shadow) != 0)
            {
                typeName += "Shadow";
            }

            return typeName;
        }

        private static string GetImageTypeName(SamplerType type, VariableType componentType)
        {
            string typeName;

            switch (type & SamplerType.Mask)
            {
                case SamplerType.Texture1D:     typeName = "image1D";     break;
                case SamplerType.TextureBuffer: typeName = "imageBuffer"; break;
                case SamplerType.Texture2D:     typeName = "image2D";     break;
                case SamplerType.Texture3D:     typeName = "image3D";     break;
                case SamplerType.TextureCube:   typeName = "imageCube";   break;

                default: throw new ArgumentException($"Invalid sampler type \"{type}\".");
            }

            if ((type & SamplerType.Multisample) != 0)
            {
                typeName += "MS";
            }

            if ((type & SamplerType.Array) != 0)
            {
                typeName += "Array";
            }

            switch (componentType)
            {
                case VariableType.U32: typeName = 'u' + typeName; break;
                case VariableType.S32: typeName = 'i' + typeName; break;
            }

            return typeName;
        }
    }
}