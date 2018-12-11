using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeLoadState : IILEmit
    {
        private ILBlock _block;

        public ILOpCodeLoadState(ILBlock block)
        {
            _block = block;
        }

        public void Emit(ILMethodBuilder context)
        {
            long intInputs = context.LocalAlloc.GetIntInputs(_block);
            long vecInputs = context.LocalAlloc.GetVecInputs(_block);

            LoadLocals(context, intInputs, RegisterType.Int);
            LoadLocals(context, vecInputs, RegisterType.Vector);
        }

        private void LoadLocals(ILMethodBuilder context, long inputs, RegisterType baseType)
        {
            for (int bit = 0; bit < 64; bit++)
            {
                long mask = 1L << bit;

                if ((inputs & mask) != 0)
                {
                    Register reg = ILMethodBuilder.GetRegFromBit(bit, baseType);

                    context.Generator.EmitLdarg(TranslatedSub.StateArgIdx);
                    context.Generator.Emit(OpCodes.Ldfld, reg.GetField());

                    context.Generator.EmitStloc(context.GetLocalIndex(reg));
                }
            }
        }
    }
}