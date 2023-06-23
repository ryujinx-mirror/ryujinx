using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.CodeGen.Spirv;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using System;
using System.Collections.Generic;
using System.Linq;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    public static class Translator
    {
        private const int HeaderSize = 0x50;

        internal readonly struct FunctionCode
        {
            public Operation[] Code { get; }

            public FunctionCode(Operation[] code)
            {
                Code = code;
            }
        }

        public static TranslatorContext CreateContext(ulong address, IGpuAccessor gpuAccessor, TranslationOptions options)
        {
            return DecodeShader(address, gpuAccessor, options);
        }

        internal static ShaderProgram Translate(FunctionCode[] functions, ShaderConfig config)
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

            List<Function> funcs = new List<Function>(functions.Length);

            for (int i = 0; i < functions.Length; i++)
            {
                funcs.Add(null);
            }

            HelperFunctionManager hfm = new HelperFunctionManager(funcs, config.Stage);

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

                    Optimizer.RunPass(hfm, cfg.Blocks, config);
                    Rewriter.RunPass(hfm, cfg.Blocks, config);
                }

                funcs[i] = new Function(cfg.Blocks, $"fun{i}", false, inArgumentsCount, outArgumentsCount);
            }

            var identification = ShaderIdentifier.Identify(funcs, config);

            var sInfo = StructuredProgram.MakeStructuredProgram(funcs, config);

            var info = config.CreateProgramInfo(identification);

            return config.Options.TargetLanguage switch
            {
                TargetLanguage.Glsl => new ShaderProgram(info, TargetLanguage.Glsl, GlslGenerator.Generate(sInfo, config)),
                TargetLanguage.Spirv => new ShaderProgram(info, TargetLanguage.Spirv, SpirvGenerator.Generate(sInfo, config)),
                _ => throw new NotImplementedException(config.Options.TargetLanguage.ToString())
            };
        }

        private static TranslatorContext DecodeShader(ulong address, IGpuAccessor gpuAccessor, TranslationOptions options)
        {
            ShaderConfig config;
            DecodedProgram program;
            ulong maxEndAddress = 0;

            if (options.Flags.HasFlag(TranslationFlags.Compute))
            {
                config = new ShaderConfig(ShaderStage.Compute, gpuAccessor, options, gpuAccessor.QueryComputeLocalMemorySize());

                program = Decoder.Decode(config, address);
            }
            else
            {
                config = new ShaderConfig(new ShaderHeader(gpuAccessor, address), gpuAccessor, options);

                program = Decoder.Decode(config, address + HeaderSize);
            }

            foreach (DecodedFunction function in program)
            {
                foreach (Block block in function.Blocks)
                {
                    if (maxEndAddress < block.EndAddress)
                    {
                        maxEndAddress = block.EndAddress;
                    }
                }
            }

            config.SizeAdd((int)maxEndAddress + (options.Flags.HasFlag(TranslationFlags.Compute) ? 0 : HeaderSize));

            return new TranslatorContext(address, program, config);
        }

        internal static FunctionCode[] EmitShader(DecodedProgram program, ShaderConfig config, bool initializeOutputs, out int initializationOperations)
        {
            initializationOperations = 0;

            FunctionMatch.RunPass(program);

            foreach (DecodedFunction function in program.Where(x => !x.IsCompilerGenerated).OrderBy(x => x.Address))
            {
                program.AddFunctionAndSetId(function);
            }

            FunctionCode[] functions = new FunctionCode[program.FunctionsWithIdCount];

            for (int index = 0; index < functions.Length; index++)
            {
                EmitterContext context = new EmitterContext(program, config, index != 0);

                if (initializeOutputs && index == 0)
                {
                    EmitOutputsInitialization(context, config);
                    initializationOperations = context.OperationsCount;
                }

                DecodedFunction function = program.GetFunctionById(index);

                foreach (Block block in function.Blocks)
                {
                    context.CurrBlock = block;

                    context.EnterBlock(block.Address);

                    EmitOps(context, block);
                }

                functions[index] = new FunctionCode(context.GetOperations());
            }

            return functions;
        }

        private static void EmitOutputsInitialization(EmitterContext context, ShaderConfig config)
        {
            // Compute has no output attributes, and fragment is the last stage, so we
            // don't need to initialize outputs on those stages.
            if (config.Stage == ShaderStage.Compute || config.Stage == ShaderStage.Fragment)
            {
                return;
            }

            if (config.Stage == ShaderStage.Vertex)
            {
                InitializePositionOutput(context);
            }

            UInt128 usedAttributes = context.Config.NextInputAttributesComponents;
            while (usedAttributes != UInt128.Zero)
            {
                int index = (int)UInt128.TrailingZeroCount(usedAttributes);
                int vecIndex = index / 4;

                usedAttributes &= ~(UInt128.One << index);

                // We don't need to initialize passthrough attributes.
                if ((context.Config.PassthroughAttributes & (1 << vecIndex)) != 0)
                {
                    continue;
                }

                InitializeOutputComponent(context, vecIndex, index & 3, perPatch: false);
            }

            if (context.Config.NextUsedInputAttributesPerPatch != null)
            {
                foreach (int vecIndex in context.Config.NextUsedInputAttributesPerPatch.Order())
                {
                    InitializeOutput(context, vecIndex, perPatch: true);
                }
            }

            if (config.NextUsesFixedFuncAttributes)
            {
                bool supportsLayerFromVertexOrTess = config.GpuAccessor.QueryHostSupportsLayerVertexTessellation();
                int fixedStartAttr = supportsLayerFromVertexOrTess ? 0 : 1;

                for (int i = fixedStartAttr; i < fixedStartAttr + 5 + AttributeConsts.TexCoordCount; i++)
                {
                    int index = config.GetFreeUserAttribute(isOutput: true, i);
                    if (index < 0)
                    {
                        break;
                    }

                    InitializeOutput(context, index, perPatch: false);

                    config.SetOutputUserAttributeFixedFunc(index);
                }
            }
        }

        private static void InitializePositionOutput(EmitterContext context)
        {
            for (int c = 0; c < 4; c++)
            {
                context.Store(StorageKind.Output, IoVariable.Position, null, Const(c), ConstF(c == 3 ? 1f : 0f));
            }
        }

        private static void InitializeOutput(EmitterContext context, int location, bool perPatch)
        {
            for (int c = 0; c < 4; c++)
            {
                InitializeOutputComponent(context, location, c, perPatch);
            }
        }

        private static void InitializeOutputComponent(EmitterContext context, int location, int c, bool perPatch)
        {
            StorageKind storageKind = perPatch ? StorageKind.OutputPerPatch : StorageKind.Output;

            if (context.Config.UsedFeatures.HasFlag(FeatureFlags.OaIndexing))
            {
                Operand invocationId = null;

                if (context.Config.Stage == ShaderStage.TessellationControl && !perPatch)
                {
                    invocationId = context.Load(StorageKind.Input, IoVariable.InvocationId);
                }

                int index = location * 4 + c;

                context.Store(storageKind, IoVariable.UserDefined, invocationId, Const(index), ConstF(c == 3 ? 1f : 0f));
            }
            else
            {
                if (context.Config.Stage == ShaderStage.TessellationControl && !perPatch)
                {
                    Operand invocationId = context.Load(StorageKind.Input, IoVariable.InvocationId);
                    context.Store(storageKind, IoVariable.UserDefined, Const(location), invocationId, Const(c), ConstF(c == 3 ? 1f : 0f));
                }
                else
                {
                    context.Store(storageKind, IoVariable.UserDefined, null, Const(location), Const(c), ConstF(c == 3 ? 1f : 0f));
                }
            }
        }

        private static void EmitOps(EmitterContext context, Block block)
        {
            for (int opIndex = 0; opIndex < block.OpCodes.Count; opIndex++)
            {
                InstOp op = block.OpCodes[opIndex];

                if (context.Config.Options.Flags.HasFlag(TranslationFlags.DebugMode))
                {
                    string instName;

                    if (op.Emitter != null)
                    {
                        instName = op.Name.ToString();
                    }
                    else
                    {
                        instName = "???";

                        context.Config.GpuAccessor.Log($"Invalid instruction at 0x{op.Address:X6} (0x{op.RawOpCode:X16}).");
                    }

                    string dbgComment = $"0x{op.Address:X6}: 0x{op.RawOpCode:X16} {instName}";

                    context.Add(new CommentNode(dbgComment));
                }

                InstConditional opConditional = new InstConditional(op.RawOpCode);

                bool noPred = op.Props.HasFlag(InstProps.NoPred);
                if (!noPred && opConditional.Pred == RegisterConsts.PredicateTrueIndex && opConditional.PredInv)
                {
                    continue;
                }

                Operand predSkipLbl = null;

                if (Decoder.IsPopBranch(op.Name))
                {
                    // If the instruction is a SYNC or BRK instruction with only one
                    // possible target address, then the instruction is basically
                    // just a simple branch, we can generate code similar to branch
                    // instructions, with the condition check on the branch itself.
                    noPred = block.SyncTargets.Count <= 1;
                }
                else if (op.Name == InstName.Bra)
                {
                    noPred = true;
                }

                if (!(opConditional.Pred == RegisterConsts.PredicateTrueIndex || noPred))
                {
                    Operand label;

                    if (opIndex == block.OpCodes.Count - 1 && block.HasNext())
                    {
                        label = context.GetLabel(block.Successors[0].Address);
                    }
                    else
                    {
                        label = Label();

                        predSkipLbl = label;
                    }

                    Operand pred = Register(opConditional.Pred, RegisterType.Predicate);

                    if (opConditional.PredInv)
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