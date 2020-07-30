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

        public static ShaderProgram Translate(ulong address, IGpuAccessor gpuAccessor, TranslationFlags flags)
        {
            return Translate(DecodeShader(address, gpuAccessor, flags, out ShaderConfig config), config);
        }

        public static ShaderProgram Translate(ulong addressA, ulong addressB, IGpuAccessor gpuAccessor, TranslationFlags flags)
        {
            Operation[] opsA = DecodeShader(addressA, gpuAccessor, flags | TranslationFlags.VertexA, out ShaderConfig configA);
            Operation[] opsB = DecodeShader(addressB, gpuAccessor, flags, out ShaderConfig config);

            config.SetUsedFeature(configA.UsedFeatures);

            return Translate(Combine(opsA, opsB), config, configA.Size);
        }

        private static ShaderProgram Translate(Operation[] ops, ShaderConfig config, int sizeA = 0)
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
                sInfo.UsesInstanceId);

            string glslCode = program.Code;

            return new ShaderProgram(spInfo, config.Stage, glslCode, config.Size, sizeA);
        }

        private static Operation[] DecodeShader(ulong address, IGpuAccessor gpuAccessor, TranslationFlags flags, out ShaderConfig config)
        {
            Block[] cfg;

            if ((flags & TranslationFlags.Compute) != 0)
            {
                config = new ShaderConfig(gpuAccessor, flags);

                cfg = Decoder.Decode(gpuAccessor, address);
            }
            else
            {
                config = new ShaderConfig(new ShaderHeader(gpuAccessor, address), gpuAccessor, flags);

                cfg = Decoder.Decode(gpuAccessor, address + HeaderSize);
            }

            if (cfg == null)
            {
                gpuAccessor.Log("Invalid branch detected, failed to build CFG.");

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

                            gpuAccessor.Log($"Invalid instruction at 0x{op.Address:X6} (0x{op.RawOpCode:X16}).");
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

            config.SizeAdd((int)maxEndAddress + (flags.HasFlag(TranslationFlags.Compute) ? 0 : HeaderSize));

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