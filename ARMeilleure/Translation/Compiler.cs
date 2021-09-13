using ARMeilleure.CodeGen;
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
            Logger.StartPass(PassName.Dominance);

            if ((options & CompilerOptions.SsaForm) != 0)
            {
                Dominance.FindDominators(cfg);
                Dominance.FindDominanceFrontiers(cfg);
            }

            Logger.EndPass(PassName.Dominance);

            Logger.StartPass(PassName.SsaConstruction);

            if ((options & CompilerOptions.SsaForm) != 0)
            {
                Ssa.Construct(cfg);
            }
            else
            {
                RegisterToLocal.Rename(cfg);
            }

            Logger.EndPass(PassName.SsaConstruction, cfg);

            CompilerContext cctx = new(cfg, argTypes, retType, options);

            return CodeGenerator.Generate(cctx);
        }
    }
}