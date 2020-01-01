using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using System;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    public static class Translator
    {
        private const int HeaderSize = 0x50;

        public static Span<byte> ExtractCode(Span<byte> code, bool compute, out int headerSize)
        {
            headerSize = compute ? 0 : HeaderSize;

            Block[] cfg = Decoder.Decode(code, (ulong)headerSize);

            if (cfg == null)
            {
                // TODO: Error.

                return code;
            }

            ulong endAddress = 0;

            foreach (Block block in cfg)
            {
                if (endAddress < block.EndAddress)
                {
                    endAddress = block.EndAddress;
                }
            }

            return code.Slice(0, headerSize + (int)endAddress);
        }

        public static ShaderProgram Translate(Span<byte> code, TranslatorCallbacks callbacks, TranslationFlags flags)
        {
            Operation[] ops = DecodeShader(code, callbacks, flags, out ShaderConfig config, out int size);

            return Translate(ops, config, size);
        }

        public static ShaderProgram Translate(Span<byte> vpACode, Span<byte> vpBCode, TranslatorCallbacks callbacks, TranslationFlags flags)
        {
            Operation[] vpAOps = DecodeShader(vpACode, callbacks, flags, out _, out _);
            Operation[] vpBOps = DecodeShader(vpBCode, callbacks, flags, out ShaderConfig config, out int sizeB);

            return Translate(Combine(vpAOps, vpBOps), config, sizeB);
        }

        private static ShaderProgram Translate(Operation[] ops, ShaderConfig config, int size)
        {
            BasicBlock[] blocks = ControlFlowGraph.MakeCfg(ops);

            if (blocks.Length > 0)
            {
                Dominance.FindDominators(blocks[0], blocks.Length);

                Dominance.FindDominanceFrontiers(blocks);

                Ssa.Rename(blocks);

                Optimizer.RunPass(blocks, config);

                Lowering.RunPass(blocks, config);
            }

            StructuredProgramInfo sInfo = StructuredProgram.MakeStructuredProgram(blocks, config);

            GlslProgram program = GlslGenerator.Generate(sInfo, config);

            ShaderProgramInfo spInfo = new ShaderProgramInfo(
                program.CBufferDescriptors,
                program.SBufferDescriptors,
                program.TextureDescriptors,
                program.ImageDescriptors,
                sInfo.InterpolationQualifiers,
                sInfo.UsesInstanceId);

            string glslCode = program.Code;

            return new ShaderProgram(spInfo, config.Stage, glslCode, size);
        }

        private static Operation[] DecodeShader(
            Span<byte>          code,
            TranslatorCallbacks callbacks,
            TranslationFlags    flags,
            out ShaderConfig    config,
            out int             size)
        {
            Block[] cfg;

            if ((flags & TranslationFlags.Compute) != 0)
            {
                config = new ShaderConfig(flags, callbacks);

                cfg = Decoder.Decode(code, 0);
            }
            else
            {
                config = new ShaderConfig(new ShaderHeader(code), flags, callbacks);

                cfg = Decoder.Decode(code, HeaderSize);
            }

            if (cfg == null)
            {
                config.PrintLog("Invalid branch detected, failed to build CFG.");

                size = 0;

                return Array.Empty<Operation>();
            }

            EmitterContext context = new EmitterContext(config);

            ulong maxEndAddress = 0;

            for (int blkIndex = 0; blkIndex < cfg.Length; blkIndex++)
            {
                Block block = cfg[blkIndex];

                if (maxEndAddress < block.EndAddress)
                {
                    maxEndAddress = block.EndAddress;
                }

                context.CurrBlock = block;

                context.MarkLabel(context.GetLabel(block.Address));

                for (int opIndex = 0; opIndex < block.OpCodes.Count; opIndex++)
                {
                    OpCode op = block.OpCodes[opIndex];

                    if ((flags & TranslationFlags.DebugMode) != 0)
                    {
                        string instName;

                        if (op.Emitter != null)
                        {
                            instName = op.Emitter.Method.Name;
                        }
                        else
                        {
                            instName = "???";

                            config.PrintLog($"Invalid instruction at 0x{op.Address:X6} (0x{op.RawOpCode:X16}).");
                        }

                        string dbgComment = $"0x{op.Address:X6}: 0x{op.RawOpCode:X16} {instName}";

                        context.Add(new CommentNode(dbgComment));
                    }

                    if (op.NeverExecute)
                    {
                        continue;
                    }

                    Operand predSkipLbl = null;

                    bool skipPredicateCheck = op is OpCodeBranch opBranch && !opBranch.PushTarget;

                    if (op is OpCodeBranchPop opBranchPop)
                    {
                        // If the instruction is a SYNC or BRK instruction with only one
                        // possible target address, then the instruction is basically
                        // just a simple branch, we can generate code similar to branch
                        // instructions, with the condition check on the branch itself.
                        skipPredicateCheck = opBranchPop.Targets.Count < 2;
                    }

                    if (!(op.Predicate.IsPT || skipPredicateCheck))
                    {
                        Operand label;

                        if (opIndex == block.OpCodes.Count - 1 && block.Next != null)
                        {
                            label = context.GetLabel(block.Next.Address);
                        }
                        else
                        {
                            label = Label();

                            predSkipLbl = label;
                        }

                        Operand pred = Register(op.Predicate);

                        if (op.InvertPredicate)
                        {
                            context.BranchIfTrue(label, pred);
                        }
                        else
                        {
                            context.BranchIfFalse(label, pred);
                        }
                    }

                    context.CurrOp = op;

                    op.Emitter?.Invoke(context);

                    if (predSkipLbl != null)
                    {
                        context.MarkLabel(predSkipLbl);
                    }
                }
            }

            size = (int)maxEndAddress + (((flags & TranslationFlags.Compute) != 0) ? 0 : HeaderSize);

            return context.GetOperations();
        }

        private static Operation[] Combine(Operation[] a, Operation[] b)
        {
            // Here we combine two shaders.
            // For shader A:
            // - All user attribute stores on shader A are turned into copies to a
            // temporary variable. It's assumed that shader B will consume them.
            // - All return instructions are turned into branch instructions, the
            // branch target being the start of the shader B code.
            // For shader B:
            // - All user attribute loads on shader B are turned into copies from a
            // temporary variable, as long that attribute is written by shader A.
            List<Operation> output = new List<Operation>(a.Length + b.Length);

            Operand[] temps = new Operand[AttributeConsts.UserAttributesCount * 4];

            Operand lblB = Label();

            for (int index = 0; index < a.Length; index++)
            {
                Operation operation = a[index];

                if (IsUserAttribute(operation.Dest))
                {
                    int tIndex = (operation.Dest.Value - AttributeConsts.UserAttributeBase) / 4;

                    Operand temp = temps[tIndex];

                    if (temp == null)
                    {
                        temp = Local();

                        temps[tIndex] = temp;
                    }

                    operation.Dest = temp;
                }

                if (operation.Inst == Instruction.Return)
                {
                    output.Add(new Operation(Instruction.Branch, lblB));
                }
                else
                {
                    output.Add(operation);
                }
            }

            output.Add(new Operation(Instruction.MarkLabel, lblB));

            for (int index = 0; index < b.Length; index++)
            {
                Operation operation = b[index];

                for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
                {
                    Operand src = operation.GetSource(srcIndex);

                    if (IsUserAttribute(src))
                    {
                        Operand temp = temps[(src.Value - AttributeConsts.UserAttributeBase) / 4];

                        if (temp != null)
                        {
                            operation.SetSource(srcIndex, temp);
                        }
                    }
                }

                output.Add(operation);
            }

            return output.ToArray();
        }

        private static bool IsUserAttribute(Operand operand)
        {
            return operand != null &&
                   operand.Type == OperandType.Attribute &&
                   operand.Value >= AttributeConsts.UserAttributeBase &&
                   operand.Value <  AttributeConsts.UserAttributeEnd;
        }
    }
}