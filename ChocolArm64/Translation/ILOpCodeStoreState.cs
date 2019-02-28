using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeStoreState : IILEmit
    {
        private ILBlock _block;

        private TranslatedSub _callSub;

        public ILOpCodeStoreState(ILBlock block, TranslatedSub callSub = null)
        {
            _block   = block;
            _callSub = callSub;
        }

        public void Emit(ILMethodBuilder context)
        {
            long intOutputs = context.RegUsage.GetIntOutputs(_block);
            long vecOutputs = context.RegUsage.GetVecOutputs(_block);

            if (Optimizations.AssumeStrictAbiCompliance && context.IsSubComplete)
            {
                intOutputs = RegisterUsage.ClearCallerSavedIntRegs(intOutputs, context.IsAarch64);
                vecOutputs = RegisterUsage.ClearCallerSavedVecRegs(vecOutputs, context.IsAarch64);
            }

            if (_callSub != null)
            {
                //Those register are assigned on the callee function, without
                //reading it's value first. We don't need to write them because
                //they are not going to be read on the callee.
                intOutputs &= ~_callSub.IntNiRegsMask;
                vecOutputs &= ~_callSub.VecNiRegsMask;
            }

            StoreLocals(context, intOutputs, RegisterType.Int);
            StoreLocals(context, vecOutputs, RegisterType.Vector);
        }

        private void StoreLocals(ILMethodBuilder context, long outputs, RegisterType baseType)
        {
            for (int bit = 0; bit < 64; bit++)
            {
                long mask = 1L << bit;

                if ((outputs & mask) != 0)
                {
                    Register reg = ILMethodBuilder.GetRegFromBit(bit, baseType);

                    context.Generator.EmitLdarg(TranslatedSub.StateArgIdx);
                    context.Generator.EmitLdloc(context.GetLocalIndex(reg));

                    context.Generator.Emit(OpCodes.Stfld, reg.GetField());
                }
            }
        }
    }
}