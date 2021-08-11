using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using System;
using System.Collections.Generic;
using System.Numerics;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    public static class Translator
    {
        private const int HeaderSize = 0x50;

        internal struct FunctionCode
        {
            public Operation[] Code { get; }

            public FunctionCode(Operation[] code)
            {
                Code = code;
            }
        }

        public static TranslatorContext CreateContext(
            ulong address,
            IGpuAccessor gpuAccessor,
            TranslationOptions options,
            TranslationCounts counts = null)
        {
            counts ??= new TranslationCounts();

            Block[][] cfg = DecodeShader(address, gpuAccessor, options, counts, out ShaderConfig config);

            return new TranslatorContext(address, cfg, config);
        }

        internal static ShaderProgram Translate(FunctionCode[] functions, ShaderConfig config, out ShaderProgramInfo shaderProgramInfo)
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

            ShaderProgram program;

            switch (config.Options.TargetLanguage)
            {
                case TargetLanguage.Glsl:
                    program = new ShaderProgram(config.Stage, GlslGenerator.Generate(sInfo, config));
                    break;
                default:
                    throw new NotImplementedException(config.Options.TargetLanguage.ToString());
            }

            shaderProgramInfo = new ShaderProgramInfo(
                config.GetConstantBufferDescriptors(),
                config.GetStorageBufferDescriptors(),
                config.GetTextureDescriptors(),
                config.GetImageDescriptors(),
                config.UsedFeatures.HasFlag(FeatureFlags.InstanceId),
                config.ClipDistancesWritten);

            return program;
        }

        private static Block[][] DecodeShader(
            ulong address,
            IGpuAccessor gpuAccessor,
            TranslationOptions options,
            TranslationCounts counts,
            out ShaderConfig config)
        {
            Block[][] cfg;
            ulong maxEndAddress = 0;

            if ((options.Flags & TranslationFlags.Compute) != 0)
            {
                config = new ShaderConfig(gpuAccessor, options, counts);

                cfg = Decoder.Decode(config, address);
            }
            else
            {
                config = new ShaderConfig(new ShaderHeader(gpuAccessor, address), gpuAccessor, options, counts);

                cfg = Decoder.Decode(config, address + HeaderSize);
            }

            for (int funcIndex = 0; funcIndex < cfg.Length; funcIndex++)
            {
                for (int blkIndex = 0; blkIndex < cfg[funcIndex].Length; blkIndex++)
                {
                    Block block = cfg[funcIndex][blkIndex];

                    if (maxEndAddress < block.EndAddress)
                    {
                        maxEndAddress = block.EndAddress;
                    }

                    if (!config.UsedFeatures.HasFlag(FeatureFlags.Bindless))
                    {
                        for (int index = 0; index < block.OpCodes.Count; index++)
                        {
                            if (block.OpCodes[index] is OpCodeTextureBase texture)
                            {
                                config.TextureHandlesForCache.Add(texture.HandleOffset);
                            }
                        }
                    }
                }
            }

            config.SizeAdd((int)maxEndAddress + (options.Flags.HasFlag(TranslationFlags.Compute) ? 0 : HeaderSize));

            return cfg;
        }

        internal static FunctionCode[] EmitShader(Block[][] cfg, ShaderConfig config, bool initializeOutputs, out int initializationOperations)
        {
            initializationOperations = 0;

            Dictionary<ulong, int> funcIds = new Dictionary<ulong, int>();

            for (int funcIndex = 0; funcIndex < cfg.Length; funcIndex++)
            {
                funcIds.Add(cfg[funcIndex][0].Address, funcIndex);
            }

            List<FunctionCode> funcs = new List<FunctionCode>();

            for (int funcIndex = 0; funcIndex < cfg.Length; funcIndex++)
            {
                EmitterContext context = new EmitterContext(config, funcIndex != 0, funcIds);

                if (initializeOutputs && funcIndex == 0)
                {
                    EmitOutputsInitialization(context, config);
                    initializationOperations = context.OperationsCount;
                }

                for (int blkIndex = 0; blkIndex < cfg[funcIndex].Length; blkIndex++)
                {
                    Block block = cfg[funcIndex][blkIndex];

                    context.CurrBlock = block;

                    context.MarkLabel(context.GetLabel(block.Address));

                    EmitOps(context, block);
                }

                funcs.Add(new FunctionCode(context.GetOperations()));
            }

            return funcs.ToArray();
        }

        private static void EmitOutputsInitialization(EmitterContext context, ShaderConfig config)
        {
            // Compute has no output attributes, and fragment is the last stage, so we
            // don't need to initialize outputs on those stages.
            if (config.Stage == ShaderStage.Compute || config.Stage == ShaderStage.Fragment)
            {
                return;
            }

            void InitializeOutput(int baseAttr)
            {
                for (int c = 0; c < 4; c++)
                {
                    context.Copy(Attribute(baseAttr + c * 4), ConstF(c == 3 ? 1f : 0f));
                }
            }

            if (config.Stage == ShaderStage.Vertex)
            {
                InitializeOutput(AttributeConsts.PositionX);
            }

            int usedAttribtes = context.Config.UsedOutputAttributes;
            while (usedAttribtes != 0)
            {
                int index = BitOperations.TrailingZeroCount(usedAttribtes);

                InitializeOutput(AttributeConsts.UserAttributeBase + index * 16);

                usedAttribtes &= ~(1 << index);
            }
        }

        private static void EmitOps(EmitterContext context, Block block)
        {
            for (int opIndex = 0; opIndex < block.OpCodes.Count; opIndex++)
            {
                OpCode op = block.OpCodes[opIndex];

                if ((context.Config.Options.Flags & TranslationFlags.DebugMode) != 0)
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
    }
}