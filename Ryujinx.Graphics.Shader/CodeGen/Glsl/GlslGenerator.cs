using Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.TypeConversion;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class GlslGenerator
    {
        public static GlslProgram Generate(StructuredProgramInfo info, ShaderConfig config)
        {
            CodeGenContext context = new CodeGenContext(config);

            Declarations.Declare(context, info);

            PrintMainBlock(context, info);

            return new GlslProgram(
                context.CBufferDescriptors.ToArray(),
                context.SBufferDescriptors.ToArray(),
                context.TextureDescriptors.ToArray(),
                context.ImageDescriptors.ToArray(),
                context.GetCode());
        }

        private static void PrintMainBlock(CodeGenContext context, StructuredProgramInfo info)
        {
            context.AppendLine("void main()");

            context.EnterScope();

            Declarations.DeclareLocals(context, info);

            // Ensure that unused attributes are set, otherwise the downstream
            // compiler may eliminate them.
            // (Not needed for fragment shader as it is the last stage).
            if (context.Config.Stage != ShaderStage.Compute &&
                context.Config.Stage != ShaderStage.Fragment)
            {
                for (int attr = 0; attr < Declarations.MaxAttributes; attr++)
                {
                    if (info.OAttributes.Contains(attr))
                    {
                        continue;
                    }

                    context.AppendLine($"{DefaultNames.OAttributePrefix}{attr} = vec4(0);");
                }
            }

            PrintBlock(context, info.MainBlock);

            context.LeaveScope();
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
                    VariableType srcType = OperandManager.GetNodeDestType(assignment.Source);
                    VariableType dstType = OperandManager.GetNodeDestType(assignment.Destination);

                    string dest;

                    if (assignment.Destination is AstOperand operand && operand.Type == OperandType.Attribute)
                    {
                        dest = OperandManager.GetOutAttributeName(operand, context.Config.Stage);
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
            VariableType srcType = OperandManager.GetNodeDestType(cond);

            return ReinterpretCast(context, cond, srcType, VariableType.Bool);
        }
    }
}