using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.Optimizations;
using ARMeilleure.Diagnostics;
using ARMeilleure.IntermediateRepresentation;
using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    static class Compiler
    {
        public static CompiledFunction Compile(
            ControlFlowGraph cfg,
            OperandType[] argTypes,
            OperandType retType,
            CompilerOptions options,
            Architecture target)
        {
            CompilerContext cctx = new(cfg, argTypes, retType, options);

            if (options.HasFlag(CompilerOptions.Optimize))
            {
                Logger.StartPass(PassName.TailMerge);

                TailMerge.RunPass(cctx);

                Logger.EndPass(PassName.TailMerge, cfg);
            }

            if (options.HasFlag(CompilerOptions.SsaForm))
            {
                Logger.StartPass(PassName.Dominance);

                Dominance.FindDominators(cfg);
                Dominance.FindDominanceFrontiers(cfg);

                Logger.EndPass(PassName.Dominance);

                Logger.StartPass(PassName.SsaConstruction);

                Ssa.Construct(cfg);

                Logger.EndPass(PassName.SsaConstruction, cfg);
            }
            else
            {
                Logger.StartPass(PassName.RegisterToLocal);

                RegisterToLocal.Rename(cfg);

                Logger.EndPass(PassName.RegisterToLocal, cfg);
            }

            if (target == Architecture.X64)
            {
                return CodeGen.X86.CodeGenerator.Generate(cctx);
            }
            else if (target == Architecture.Arm64)
            {
                return CodeGen.Arm64.CodeGenerator.Generate(cctx);
            }
            else
            {
                throw new NotImplementedException(target.ToString());
            }
        }
    }
}
