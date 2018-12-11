using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeStoreState : IILEmit
    {
        private ILBlock _block;

        public ILOpCodeStoreState(ILBlock block)
        {
            _block = block;
        }

        public void Emit(ILMethodBuilder context)
        {
            long intOutputs = context.LocalAlloc.GetIntOutputs(_block);
            long vecOutputs = context.LocalAlloc.GetVecOutputs(_block);

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