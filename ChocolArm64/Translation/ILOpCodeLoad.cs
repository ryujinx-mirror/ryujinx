using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeLoad : IILEmit
    {
        public int Index { get; private set; }

        public IoType IoType { get; private set; }

        public RegisterSize RegisterSize { get; private set; }

        public ILOpCodeLoad(int index, IoType ioType, RegisterSize registerSize = 0)
        {
            Index        = index;
            IoType       = ioType;
            RegisterSize = registerSize;
        }

        public void Emit(ILMethodBuilder context)
        {
            switch (IoType)
            {
                case IoType.Arg: context.Generator.EmitLdarg(Index); break;

                case IoType.Flag:   EmitLdloc(context, Index, RegisterType.Flag);   break;
                case IoType.Int:    EmitLdloc(context, Index, RegisterType.Int);    break;
                case IoType.Vector: EmitLdloc(context, Index, RegisterType.Vector); break;
            }
        }

        private void EmitLdloc(ILMethodBuilder context, int index, RegisterType registerType)
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