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

        public static string Generate(StructuredProgramInfo info, ShaderConfig config)
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

            return context.GetCode();
        }

        private static void PrintFunction(CodeGenContext context, StructuredProgramInfo info, StructuredFunction function, string funcName = null)
        {
            context.CurrentFunction = function;

            context.AppendLine(GetFunctionSignature(function, funcName));
            context.EnterScope();

            Declarations.DeclareLocals(context, function);

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
                    string expr = InstGen.GetExpression(context, operation);

                    if (expr != null)
                    {
                        context.AppendLine(expr + ";");
                    }
                }
                else if (node is AstAssignment assignment)
                {
                    AggregateType srcType = OperandManager.GetNodeDestType(context, assignment.Source);
                    AggregateType dstType = OperandManager.GetNodeDestType(context, assignment.Destination, isAsgDest: true);

                    string dest;

                    if (assignment.Destination is AstOperand operand && operand.Type.IsAttribute())
                    {
                        bool perPatch = operand.Type == OperandType.AttributePerPatch;
                        dest = OperandManager.GetOutAttributeName(context, operand.Value, perPatch);
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
            AggregateType srcType = OperandManager.GetNodeDestType(context, cond);

            return ReinterpretCast(context, cond, srcType, AggregateType.Bool);
        }
    }
}