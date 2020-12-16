using ARMeilleure.CodeGen;
using ARMeilleure.CodeGen.X86;
using ARMeilleure.Diagnostics;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation.Cache;
using ARMeilleure.Translation.PTC;
using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    static class Compiler
    {
        public static T Compile<T>(
            ControlFlowGraph cfg,
            OperandType[]    argTypes,
            OperandType      retType,
            CompilerOptions  options,
            PtcInfo          ptcInfo = null)
        {
            CompiledFunction func = Compile(cfg, argTypes, retType, options, ptcInfo);

            IntPtr codePtr = JitCache.Map(func);

            return Marshal.GetDelegateForFunctionPointer<T>(codePtr);
        }

        public static CompiledFunction Compile(
            ControlFlowGraph cfg,
            OperandType[]    argTypes,
            OperandType      retType,
            CompilerOptions  options,
            PtcInfo          ptcInfo = null)
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

            CompilerContext cctx = new CompilerContext(cfg, argTypes, retType, options);

            return CodeGenerator.Generate(cctx, ptcInfo);
        }
    }
}