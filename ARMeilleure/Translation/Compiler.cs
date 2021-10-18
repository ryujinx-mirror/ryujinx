using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.Optimizations;
using ARMeilleure.CodeGen.X86;
using ARMeilleure.Diagnostics;
using ARMeilleure.IntermediateRepresentation;

namespace ARMeilleure.Translation
{
    static class Compiler
    {
        public static CompiledFunction Compile(
            ControlFlowGraph cfg,
            OperandType[]    argTypes,
            OperandType      retType,
            CompilerOptions  options)
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

            return CodeGenerator.Generate(cctx);
        }
    }
}