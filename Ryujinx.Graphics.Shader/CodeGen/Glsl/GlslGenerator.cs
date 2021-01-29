using Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using System;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.TypeConversion;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class GlslGenerator
    {
        private const string MainFunctionName = "main";

        public static GlslProgram Generate(StructuredProgramInfo info, ShaderConfig config)
        {
            CodeGenContext context = new CodeGenContext(info, config);

            Declarations.Declare(context, info);

            if (info.Functions.Count != 0)
            {
                for (int i = 1; i < info.Functions.Count; i++)
                {
                    context.AppendLine($"{GetFunctionSignature(info.Functions[i])};");
                }

                context.AppendLine();

                for (int i = 1; i < info.Functions.Count; i++)
                {
                    PrintFunction(context, info, info.Functions[i]);

                    context.AppendLine();
                }
            }

            PrintFunction(context, info, info.Functions[0], MainFunctionName);

            return new GlslProgram(
                context.CBufferDescriptors.ToArray(),
                context.SBufferDescriptors.ToArray(),
                context.TextureDescriptors.ToArray(),
                context.ImageDescriptors.ToArray(),
                context.GetCode());
        }

        private static void PrintFunction(CodeGenContext context, StructuredProgramInfo info, StructuredFunction function, string funcName = null)
        {
            context.CurrentFunction = function;

            context.AppendLine(GetFunctionSignature(function, funcName));
            context.EnterScope();

            Declarations.DeclareLocals(context, function);

            if (funcName == MainFunctionName)
            {
                // Some games will leave some elements of gl_Position uninitialized,
                // in those cases, the elements will contain undefined values according
                // to the spec, but on NVIDIA they seems to be always initialized to (0, 0, 0, 1),
                // so we do explicit initialization to avoid UB on non-NVIDIA gpus.
                if (context.Config.Stage == ShaderStage.Vertex)
                {
                    context.AppendLine("gl_Position = vec4(0.0, 0.0, 0.0, 1.0);");
                }

                // Ensure that unused attributes are set, otherwise the downstream
                // compiler may eliminate them.
                // (Not needed for fragment shader as it is the last stage).
                if (context.Config.Stage != ShaderStage.Compute &&
                    context.Config.Stage != ShaderStage.Fragment &&
                    !context.Config.GpPassthrough)
                {
                    for (int attr = 0; attr < Declarations.MaxAttributes; attr++)
                    {
                        if (info.OAttributes.Contains(attr))
                        {
                            continue;
                        }

                        if ((context.Config.Flags & TranslationFlags.Feedback) != 0)
                        {
                            context.AppendLine($"{DefaultNames.OAttributePrefix}{attr}_x = 0;");
                            context.AppendLine($"{DefaultNames.OAttributePrefix}{attr}_y = 0;");
                            context.AppendLine($"{DefaultNames.OAttributePrefix}{attr}_z = 0;");
                            context.AppendLine($"{DefaultNames.OAttributePrefix}{attr}_w = 0;");
                        }
                        else
                        {
                            context.AppendLine($"{DefaultNames.OAttributePrefix}{attr} = vec4(0);");
                        }
                    }
                }
            }

            PrintBlock(context, function.MainBlock);

            context.LeaveScope();
        }

        private static string GetFunctionSignature(StructuredFunction function, string funcName = null)
        {
            string[] args = new string[function.InArguments.Length + function.OutArguments.Length];

            for (int i = 0; i < function.InArguments.Length; i++)
            {
                args[i] = $"{Declarations.GetVarTypeName(function.InArguments[i])} {OperandManager.GetArgumentName(i)}";
            }

            for (int i = 0; i < function.OutArguments.Length; i++)
            {
                int j = i + function.InArguments.Length;

                args[j] = $"out {Declarations.GetVarTypeName(function.OutArguments[i])} {OperandManager.GetArgumentName(j)}";
            }

            return $"{Declarations.GetVarTypeName(function.ReturnType)} {funcName ?? function.Name}({string.Join(", ", args)})";
        }

        private static void PrintBlock(CodeGenContext context, AstBlock block)
        {
            AstBlockVisitor visitor = new AstBlockVisitor(block);

            visitor.BlockEntered += (sender, e) =>
            {
                switch (e.Block.Type)
                {
                    case AstBlockType.DoWhile:
                        context.AppendLine("do");
                        break;

                    case AstBlockType.Else:
                        context.AppendLine("else");
                        break;

                    case AstBlockType.ElseIf:
                        context.AppendLine($"else if ({GetCondExpr(context, e.Block.Condition)})");
                        break;

                    case AstBlockType.If:
                        context.AppendLine($"if ({GetCondExpr(context, e.Block.Condition)})");
                        break;

                    default: throw new InvalidOperationException($"Found unexpected block type \"{e.Block.Type}\".");
                }

                context.EnterScope();
            };

            visitor.BlockLeft += (sender, e) =>
            {
                context.LeaveScope();

                if (e.Block.Type == AstBlockType.DoWhile)
                {
                    context.AppendLine($"while ({GetCondExpr(context, e.Block.Condition)});");
                }
            };

            foreach (IAstNode node in visitor.Visit())
            {
                if (node is AstOperation operation)
                {
                    context.AppendLine(InstGen.GetExpression(context, operation) + ";");
                }
                else if (node is AstAssignment assignment)
                {
                    VariableType srcType = OperandManager.GetNodeDestType(context, assignment.Source);
                    VariableType dstType = OperandManager.GetNodeDestType(context, assignment.Destination);

                    string dest;

                    if (assignment.Destination is AstOperand operand && operand.Type == OperandType.Attribute)
                    {
                        dest = OperandManager.GetOutAttributeName(operand, context.Config);
                    }
                    else
                    {
                        dest = InstGen.GetExpression(context, assignment.Destination);
                    }

                    string src = ReinterpretCast(context, assignment.Source, srcType, dstType);

                    context.AppendLine(dest + " = " + src + ";");
                }
                else if (node is AstComment comment)
                {
                    context.AppendLine("// " + comment.Comment);
                }
                else
                {
                    throw new InvalidOperationException($"Found unexpected node type \"{node?.GetType().Name ?? "null"}\".");
                }
            }
        }

        private static string GetCondExpr(CodeGenContext context, IAstNode cond)
        {
            VariableType srcType = OperandManager.GetNodeDestType(context, cond);

            return ReinterpretCast(context, cond, srcType, VariableType.Bool);
        }
    }
}