using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct IlOpCodeStore : IILEmit
    {
        public int Index { get; private set; }

        public IoType IoType { get; private set; }

        public RegisterSize RegisterSize { get; private set; }

        public IlOpCodeStore(int index, IoType ioType, RegisterSize registerSize = 0)
        {
            Index        = index;
            IoType       = ioType;
            RegisterSize = registerSize;
        }

        public void Emit(ILEmitter context)
        {
            switch (IoType)
            {
                case IoType.Arg: context.Generator.EmitStarg(Index); break;

                case IoType.Fields:
                {
                    long intOutputs = context.LocalAlloc.GetIntOutputs(context.GetIlBlock(Index));
                    long vecOutputs = context.LocalAlloc.GetVecOutputs(context.GetIlBlock(Index));

                    StoreLocals(context, intOutputs, RegisterType.Int);
                    StoreLocals(context, vecOutputs, RegisterType.Vector);
                    
                    break;
                }

                case IoType.Flag:   EmitStloc(context, Index, RegisterType.Flag);   break;
                case IoType.Int:    EmitStloc(context, Index, RegisterType.Int);    break;
                case IoType.Vector: EmitStloc(context, Index, RegisterType.Vector); break;
            }
        }

        private void StoreLocals(ILEmitter context, long outputs, RegisterType baseType)
        {
            for (int bit = 0; bit < 64; bit++)
            {
                long mask = 1L << bit;

                if ((outputs & mask) != 0)
                {
                    Register reg = ILEmitter.GetRegFromBit(bit, baseType);

                    context.Generator.EmitLdarg(TranslatedSub.StateArgIdx);
                    context.Generator.EmitLdloc(context.GetLocalIndex(reg));

                    context.Generator.Emit(OpCodes.Stfld, reg.GetField());
                }
            }
        }

        private void EmitStloc(ILEmitter context, int index, RegisterType registerType)
        {
            Register reg = new Register(index, registerType);

            if (registerType == RegisterType.Int &&
                RegisterSize == RegisterSize.Int32)
            {
                context.Generator.Emit(OpCodes.Conv_U8);
            }

            context.Generator.EmitStloc(context.GetLocalIndex(reg));
        }
    }
}