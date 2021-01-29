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
            context.AppendLine("#version 440 core");
            context.AppendLine("#extension GL_ARB_gpu_shader_int64 : enable");
            context.AppendLine("#extension GL_ARB_shader_ballot : enable");
            context.AppendLine("#extension GL_ARB_shader_group_vote : enable");
            context.AppendLine("#extension GL_EXT_shader_image_load_formatted : enable");

            if (context.Config.Stage == ShaderStage.Compute)
            {
                context.AppendLine("#extension GL_ARB_compute_shader : enable");
            }

            if (context.Config.GpPassthrough)
            {
                context.AppendLine("#extension GL_NV_geometry_shader_passthrough : enable");
            }

            context.AppendLine("#pragma optionNV(fastmath off)");

            context.AppendLine();

            context.AppendLine($"const int {DefaultNames.UndefinedName} = 0;");
            context.AppendLine();

            if (context.Config.Stage == ShaderStage.Compute)
            {
                int localMemorySize = BitUtils.DivRoundUp(context.Config.GpuAccessor.QueryComputeLocalMemorySize(), 4);

                if (localMemorySize != 0)
                {
                    string localMemorySizeStr = NumberFormatter.FormatInt(localMemorySize);

                    context.AppendLine($"uint {DefaultNames.LocalMemoryName}[{localMemorySizeStr}];");
                    context.AppendLine();
                }

                int sharedMemorySize = BitUtils.DivRoundUp(context.Config.GpuAccessor.QueryComputeSharedMemorySize(), 4);

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
                if (context.Config.Stage == ShaderStage.Geometry)
                {
                    string inPrimitive = context.Config.GpuAccessor.QueryPrimitiveTopology().ToGlslString();

                    context.AppendLine($"layout ({inPrimitive}) in;");

                    if (context.Config.GpPassthrough)
                    {
                        context.AppendLine($"layout (passthrough) in gl_PerVertex");
                        context.EnterScope();
                        context.AppendLine("vec4 gl_Position;");
                        context.AppendLine("float gl_PointSize;");
                        context.AppendLine("float gl_ClipDistance[];");
                        context.LeaveScope(";");
                    }
                    else
                    {
                        string outPrimitive = context.Config.OutputTopology.ToGlslString();

                        int maxOutputVertices = context.Config.MaxOutputVertices;

                        context.AppendLine($"layout ({outPrimitive}, max_vertices = {maxOutputVertices}) out;");
                    }

                    context.AppendLine();
                }

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
                string localSizeX = NumberFormatter.FormatInt(context.Config.GpuAccessor.QueryComputeLocalSizeX());
                string localSizeY = NumberFormatter.FormatInt(context.Config.GpuAccessor.QueryComputeLocalSizeY());
                string localSizeZ = NumberFormatter.FormatInt(context.Config.GpuAccessor.QueryComputeLocalSizeZ());

                context.AppendLine(
                    "layout (" +
                    $"local_size_x = {localSizeX}, " +
                    $"local_size_y = {localSizeY}, " +
                    $"local_size_z = {localSizeZ}) in;");
                context.AppendLine();
            }

            if (context.Config.Stage == ShaderStage.Fragment || context.Config.Stage == ShaderStage.Compute)
            {
                if (context.Config.Stage == ShaderStage.Fragment)
                {
                    if (context.Config.GpuAccessor.QueryEarlyZForce())
                    {
                        context.AppendLine("layout(early_fragment_tests) in;");
                        context.AppendLine();
                    }

                    context.AppendLine($"uniform bool {DefaultNames.IsBgraName}[8];");
                    context.AppendLine();
                }

                if (DeclareRenderScale(context))
                {
                    context.AppendLine();
                }
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.AtomicMinMaxS32Shared) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/AtomicMinMaxS32Shared.glsl");
            }

            if ((info.HelperFunctionsMask & HelperFunctionsMask.AtomicMinMaxS32Storage) != 0)
            {
                AppendHelperFunction(context, "Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/AtomicMinMaxS32Storage.glsl");
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

        public static void DeclareLocals(CodeGenContext context, StructuredFunction function)
        {
            foreach (AstOperand decl in function.Locals)
            {
                string name = context.OperandManager.DeclareLocal(decl);

                context.AppendLine(GetVarTypeName(decl.VarType) + " " + name + ";");
            }
        }

        public static string GetVarTypeName(VariableType type)
        {
            switch (type)
            {
                case VariableType.Bool: return "bool";
                case VariableType.F32:  return "precise float";
                case VariableType.F64:  return "double";
                case VariableType.None: return "void";
                case VariableType.S32:  return "int";
                case VariableType.U32:  return "uint";
            }

            throw new ArgumentException($"Invalid variable type \"{type}\".");
        }

        private static void DeclareUniforms(CodeGenContext context, StructuredProgramInfo info)
        {
            string ubSize = "[" + NumberFormatter.FormatInt(Constants.ConstantBufferSize / 16) + "]";

            if (info.UsesCbIndexing)
            {
                int count = info.CBuffers.Max() + 1;

                int[] bindings = new int[count];

                for (int i = 0; i < count; i++)
                {
                    bindings[i] = context.Config.Counts.IncrementUniformBuffersCount();
                }

                foreach (int cbufSlot in info.CBuffers.OrderBy(x => x))
                {
                    context.CBufferDescriptors.Add(new BufferDescriptor(bindings[cbufSlot], cbufSlot));
                }

                string ubName = OperandManager.GetShaderStagePrefix(context.Config.Stage);

                ubName += "_" + DefaultNames.UniformNamePrefix;

                string blockName = $"{ubName}_{DefaultNames.BlockSuffix}";

                context.AppendLine($"layout (binding = {bindings[0]}, std140) uniform {blockName}");
                context.EnterScope();
                context.AppendLine("vec4 " + DefaultNames.DataName + ubSize + ";");
                context.LeaveScope($" {ubName}[{NumberFormatter.FormatInt(count)}];");
            }
            else
            {
                foreach (int cbufSlot in info.CBuffers.OrderBy(x => x))
                {
                    int binding = context.Config.Counts.IncrementUniformBuffersCount();

                    context.CBufferDescriptors.Add(new BufferDescriptor(binding, cbufSlot));

                    string ubName = OperandManager.GetShaderStagePrefix(context.Config.Stage);

                    ubName += "_" + DefaultNames.UniformNamePrefix + cbufSlot;

                    context.AppendLine($"layout (binding = {binding}, std140) uniform {ubName}");
                    context.EnterScope();
                    context.AppendLine("vec4 " + OperandManager.GetUbName(context.Config.Stage, cbufSlot, false) + ubSize + ";");
                    context.LeaveScope(";");
                }
            }
        }

        private static void DeclareStorages(CodeGenContext context, StructuredProgramInfo info)
        {
            string sbName = OperandManager.GetShaderStagePrefix(context.Config.Stage);

            sbName += "_" + DefaultNames.StorageNamePrefix;

            string blockName = $"{sbName}_{DefaultNames.BlockSuffix}";

            int count = info.SBuffers.Max() + 1;

            int[] bindings = new int[count];

            for (int i = 0; i < count; i++)
            {
                bindings[i] = context.Config.Counts.IncrementStorageBuffersCount();
            }

            foreach (int sbufSlot in info.SBuffers)
            {
                context.SBufferDescriptors.Add(new BufferDescriptor(bindings[sbufSlot], sbufSlot));
            }

            context.AppendLine($"layout (binding = {bindings[0]}, std430) buffer {blockName}");
            context.EnterScope();
            context.AppendLine("uint " + DefaultNames.DataName + "[];");
            context.LeaveScope($" {sbName}[{NumberFormatter.FormatInt(count)}];");
        }

        private static void DeclareSamplers(CodeGenContext context, StructuredProgramInfo info)
        {
            HashSet<string> samplers = new HashSet<string>();

            // Texture instructions other than TextureSample (like TextureSize)
            // may have incomplete sampler type information. In those cases,
            // we prefer instead the more accurate information from the
            // TextureSample instruction, if both are available.
            foreach (AstTextureOperation texOp in info.Samplers.OrderBy(x => x.Handle * 2 + (x.Inst == Instruction.TextureSample ? 0 : 1)))
            {
                string indexExpr = NumberFormatter.FormatInt(texOp.ArraySize);

                string samplerName = OperandManager.GetSamplerName(context.Config.Stage, texOp, indexExpr);

                if ((texOp.Flags & TextureFlags.Bindless) != 0 || !samplers.Add(samplerName))
                {
                    continue;
                }

                int firstBinding = -1;

                if ((texOp.Type & SamplerType.Indexed) != 0)
                {
                    for (int index = 0; index < texOp.ArraySize; index++)
                    {
                        int binding = context.Config.Counts.IncrementTexturesCount();

                        if (firstBinding < 0)
                        {
                            firstBinding = binding;
                        }

                        var desc = new TextureDescriptor(binding, texOp.Type, texOp.Format, texOp.CbufSlot, texOp.Handle + index * 2);

                        context.TextureDescriptors.Add(desc);
                    }
                }
                else
                {
                    firstBinding = context.Config.Counts.IncrementTexturesCount();

                    var desc = new TextureDescriptor(firstBinding, texOp.Type, texOp.Format, texOp.CbufSlot, texOp.Handle);

                    context.TextureDescriptors.Add(desc);
                }

                string samplerTypeName = texOp.Type.ToGlslSamplerType();

                context.AppendLine($"layout (binding = {firstBinding}) uniform {samplerTypeName} {samplerName};");
            }
        }

        private static void DeclareImages(CodeGenContext context, StructuredProgramInfo info)
        {
            HashSet<string> images = new HashSet<string>();

            foreach (AstTextureOperation texOp in info.Images.OrderBy(x => x.Handle))
            {
                string indexExpr = NumberFormatter.FormatInt(texOp.ArraySize);

                string imageName = OperandManager.GetImageName(context.Config.Stage, texOp, indexExpr);

                if ((texOp.Flags & TextureFlags.Bindless) != 0 || !images.Add(imageName))
                {
                    continue;
                }

                int firstBinding = -1;

                if ((texOp.Type & SamplerType.Indexed) != 0)
                {
                    for (int index = 0; index < texOp.ArraySize; index++)
                    {
                        int binding = context.Config.Counts.IncrementImagesCount();

                        if (firstBinding < 0)
                        {
                            firstBinding = binding;
                        }

                        var desc = new TextureDescriptor(binding, texOp.Type, texOp.Format, texOp.CbufSlot, texOp.Handle + index * 2);

                        context.ImageDescriptors.Add(desc);
                    }
                }
                else
                {
                    firstBinding = context.Config.Counts.IncrementImagesCount();

                    var desc = new TextureDescriptor(firstBinding, texOp.Type, texOp.Format, texOp.CbufSlot, texOp.Handle);

                    context.ImageDescriptors.Add(desc);
                }

                string layout = texOp.Format.ToGlslFormat();

                if (!string.IsNullOrEmpty(layout))
                {
                    layout = ", " + layout;
                }

                string imageTypeName = texOp.Type.ToGlslImageType(texOp.Format.GetComponentType());

                context.AppendLine($"layout (binding = {firstBinding}{layout}) uniform {imageTypeName} {imageName};");
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

                string pass = context.Config.GpPassthrough ? "passthrough, " : string.Empty;

                string name = $"{DefaultNames.IAttributePrefix}{attr}";

                if ((context.Config.Flags & TranslationFlags.Feedback) != 0)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        char swzMask = "xyzw"[c];

                        context.AppendLine($"layout ({pass}location = {attr}, component = {c}) {iq}in float {name}_{swzMask}{suffix};");
                    }
                }
                else
                {
                    context.AppendLine($"layout ({pass}location = {attr}) {iq}in vec4 {name}{suffix};");
                }
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
                DeclareOutputAttribute(context, attr);
            }

            foreach (int attr in info.OAttributes.OrderBy(x => x).Where(x => x >= MaxAttributes))
            {
                DeclareOutputAttribute(context, attr);
            }
        }

        private static void DeclareOutputAttribute(CodeGenContext context, int attr)
        {
            string name = $"{DefaultNames.OAttributePrefix}{attr}";

            if ((context.Config.Flags & TranslationFlags.Feedback) != 0)
            {
                for (int c = 0; c < 4; c++)
                {
                    char swzMask = "xyzw"[c];

                    context.AppendLine($"layout (location = {attr}, component = {c}) out float {name}_{swzMask};");
                }
            }
            else
            {
                context.AppendLine($"layout (location = {attr}) out vec4 {name};");
            }
        }

        private static bool DeclareRenderScale(CodeGenContext context)
        {
            if ((context.Config.UsedFeatures & (FeatureFlags.FragCoordXY | FeatureFlags.IntegerSampling)) != 0)
            {
                string stage = OperandManager.GetShaderStagePrefix(context.Config.Stage);

                int scaleElements = context.TextureDescriptors.Count + context.ImageDescriptors.Count;

                if (context.Config.Stage == ShaderStage.Fragment)
                {
                    scaleElements++; // Also includes render target scale, for gl_FragCoord.
                }

                context.AppendLine($"uniform float {stage}_renderScale[{scaleElements}];");

                if (context.Config.UsedFeatures.HasFlag(FeatureFlags.IntegerSampling))
                {
                    context.AppendLine();
                    AppendHelperFunction(context, $"Ryujinx.Graphics.Shader/CodeGen/Glsl/HelperFunctions/TexelFetchScale_{stage}.glsl");
                }

                return true;
            }

            return false;
        }

        private static void AppendHelperFunction(CodeGenContext context, string filename)
        {
            string code = EmbeddedResources.ReadAllText(filename);

            code = code.Replace("\t", CodeGenContext.Tab);
            code = code.Replace("$SHARED_MEM$", DefaultNames.SharedMemoryName);
            code = code.Replace("$STORAGE_MEM$", OperandManager.GetShaderStagePrefix(context.Config.Stage) + "_" + DefaultNames.StorageNamePrefix);

            context.AppendLine(code);
            context.AppendLine();
        }
    }
}