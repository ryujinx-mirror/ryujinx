using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct IlOpCodeLoad : IILEmit
    {
        public int Index { get; private set; }

        public IoType IoType { get; private set; }

        public RegisterSize RegisterSize { get; private set; }

        public IlOpCodeLoad(int index, IoType ioType, RegisterSize registerSize = 0)
        {
            Index        = index;
            IoType       = ioType;
            RegisterSize = registerSize;
        }

        public void Emit(ILEmitter context)
        {
            switch (IoType)
            {
                case IoType.Arg: context.Generator.EmitLdarg(Index); break;

                case IoType.Fields:
                {
                    long intInputs = context.LocalAlloc.GetIntInputs(context.GetIlBlock(Index));
                    long vecInputs = context.LocalAlloc.GetVecInputs(context.GetIlBlock(Index));

                    LoadLocals(context, intInputs, RegisterType.Int);
                    LoadLocals(context, vecInputs, RegisterType.Vector);

                    break;
                }

                case IoType.Flag:   EmitLdloc(context, Index, RegisterType.Flag);   break;
                case IoType.Int:    EmitLdloc(context, Index, RegisterType.Int);    break;
                case IoType.Vector: EmitLdloc(context, Index, RegisterType.Vector); break;
            }
        }

        private void LoadLocals(ILEmitter context, long inputs, RegisterType baseType)
        {
            for (int bit = 0; bit < 64; bit++)
            {
                long mask = 1L << bit;

                if ((inputs & mask) != 0)
                {
                    Register reg = ILEmitter.GetRegFromBit(bit, baseType);

                    context.Generator.EmitLdarg(TranslatedSub.StateArgIdx);
                    context.Generator.Emit(OpCodes.Ldfld, reg.GetField());

                    context.Generator.EmitStloc(context.GetLocalIndex(reg));
                }
            }
        }

        private void EmitLdloc(ILEmitter context, int index, RegisterType registerType)
        {
            Register reg = new Register(index, registerType);

            context.Generator.EmitLdloc(context.GetLocalIndex(reg));

            if (registerType == RegisterType.Int &&
                RegisterSize == RegisterSize.Int32)
            {
                context.Generator.Emit(OpCodes.Conv_U4);
            }
        }
    }
}