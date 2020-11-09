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

        private struct FunctionCode
        {
            public Operation[] Code { get; }

            public FunctionCode(Operation[] code)
            {
                Code = code;
            }
        }

        public static ShaderProgram Translate(
            ulong address,
            IGpuAccessor gpuAccessor,
            TranslationFlags flags,
            TranslationCounts counts = null)
        {
            counts ??= new TranslationCounts();

            return Translate(DecodeShader(address, gpuAccessor, flags, counts, out ShaderConfig config), config);
        }

        public static ShaderProgram Translate(
            ulong addressA,
            ulong addressB,
            IGpuAccessor gpuAccessor,
            TranslationFlags flags,
            TranslationCounts counts = null)
        {
            counts ??= new TranslationCounts();

            FunctionCode[] funcA = DecodeShader(addressA, gpuAccessor, flags | TranslationFlags.VertexA, counts, out ShaderConfig configA);
            FunctionCode[] funcB = DecodeShader(addressB, gpuAccessor, flags, counts, out ShaderConfig config);

            config.SetUsedFeature(configA.UsedFeatures);

            return Translate(Combine(funcA, funcB), config, configA.Size);
        }

        private static ShaderProgram Translate(FunctionCode[] functions, ShaderConfig config, int sizeA = 0)
        {
            var cfgs = new ControlFlowGraph[functions.Length];
            var frus = new RegisterUsage.FunctionRegisterUsage[functions.Length];

            for (int i = 0; i < functions.Length; i++)
            {
                cfgs[i] = ControlFlowGraph.Create(functions[i].Code);

                if (i != 0)
                {
                    frus[i] = RegisterUsage.RunPass(cfgs[i]);
                }
            }

            Function[] funcs = new Function[functions.Length];

            for (int i = 0; i < functions.Length; i++)
            {
                var cfg = cfgs[i];

                int inArgumentsCount = 0;
                int outArgumentsCount = 0;

                if (i != 0)
                {
                    var fru = frus[i];

                    inArgumentsCount  = fru.InArguments.Length;
                    outArgumentsCount = fru.OutArguments.Length;
                }

                if (cfg.Blocks.Length != 0)
                {
                    RegisterUsage.FixupCalls(cfg.Blocks, frus);

                    Dominance.FindDominators(cfg);

                    Dominance.FindDominanceFrontiers(cfg.Blocks);

                    Ssa.Rename(cfg.Blocks);

                    Optimizer.RunPass(cfg.Blocks, config);

                    Rewriter.RunPass(cfg.Blocks, config);
                }

                funcs[i] = new Function(cfg.Blocks, $"fun{i}", false, inArgumentsCount, outArgumentsCount);
            }

            StructuredProgramInfo sInfo = StructuredProgram.MakeStructuredProgram(funcs, config);

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

        private static FunctionCode[] DecodeShader(
            ulong address,
            IGpuAccessor gpuAccessor,
            TranslationFlags flags,
            TranslationCounts counts,
            out ShaderConfig config)
        {
            Block[][] cfg;

            if ((flags & TranslationFlags.Compute) != 0)
            {
                config = new ShaderConfig(gpuAccessor, flags, counts);

                cfg = Decoder.Decode(gpuAccessor, address);
            }
            else
            {
                config = new ShaderConfig(new ShaderHeader(gpuAccessor, address), gpuAccessor, flags, counts);

                cfg = Decoder.Decode(gpuAccessor, address + HeaderSize);
            }

            if (cfg == null)
            {
                gpuAccessor.Log("Invalid branch detected, failed to build CFG.");

                return Array.Empty<FunctionCode>();
            }

            Dictionary<ulong, int> funcIds = new Dictionary<ulong, int>();

            for (int funcIndex = 0; funcIndex < cfg.Length; funcIndex++)
            {
                funcIds.Add(cfg[funcIndex][0].Address, funcIndex);
            }

            List<FunctionCode> funcs = new List<FunctionCode>();

            ulong maxEndAddress = 0;

            for (int funcIndex = 0; funcIndex < cfg.Length; funcIndex++)
            {
                EmitterContext context = new EmitterContext(config, funcIndex != 0, funcIds);

                for (int blkIndex = 0; blkIndex < cfg[funcIndex].Length; blkIndex++)
                {
                    Block block = cfg[funcIndex][blkIndex];

                    if (maxEndAddress < block.EndAddress)
                    {
                        maxEndAddress = block.EndAddress;
                    }

                    context.CurrBlock = block;

                    context.MarkLabel(context.GetLabel(block.Address));

                    EmitOps(context, block);
                }

                funcs.Add(new FunctionCode(context.GetOperations()));
            }

            config.SizeAdd((int)maxEndAddress + (flags.HasFlag(TranslationFlags.Compute) ? 0 : HeaderSize));

            return funcs.ToArray();
        }

        internal static void EmitOps(EmitterContext context, Block block)
        {
            for (int opIndex = 0; opIndex < block.OpCodes.Count; opIndex++)
            {
                OpCode op = block.OpCodes[opIndex];

                if ((context.Config.Flags & TranslationFlags.DebugMode) != 0)
                {
                    string instName;

                    if (op.Emitter != null)
                    {
                        instName = op.Emitter.Method.Name;
                    }
                    else
                    {
                        instName = "???";

                        context.Config.GpuAccessor.Log($"Invalid instruction at 0x{op.Address:X6} (0x{op.RawOpCode:X16}).");
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

        private static FunctionCode[] Combine(FunctionCode[] a, FunctionCode[] b)
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
            FunctionCode[] output = new FunctionCode[a.Length + b.Length - 1];

            List<Operation> ops = new List<Operation>(a.Length + b.Length);

            Operand[] temps = new Operand[AttributeConsts.UserAttributesCount * 4];

            Operand lblB = Label();

            for (int index = 0; index < a[0].Code.Length; index++)
            {
                Operation operation = a[0].Code[index];

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
                    ops.Add(new Operation(Instruction.Branch, lblB));
                }
                else
                {
                    ops.Add(operation);
                }
            }

            ops.Add(new Operation(Instruction.MarkLabel, lblB));

            for (int index = 0; index < b[0].Code.Length; index++)
            {
                Operation operation = b[0].Code[index];

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

                ops.Add(operation);
            }

            output[0] = new FunctionCode(ops.ToArray());

            for (int i = 1; i < a.Length; i++)
            {
                output[i] = a[i];
            }

            for (int i = 1; i < b.Length; i++)
            {
                output[a.Length + i - 1] = b[i];
            }

            return output;
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