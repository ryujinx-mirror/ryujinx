using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeStore : IILEmit
    {
        public int Index { get; private set; }

        public IoType IoType { get; private set; }

        public RegisterSize RegisterSize { get; private set; }

        public ILOpCodeStore(int index, IoType ioType, RegisterSize registerSize = 0)
        {
            Index        = index;
            IoType       = ioType;
            RegisterSize = registerSize;
        }

        public void Emit(ILMethodBuilder context)
        {
            switch (IoType)
            {
                case IoType.Arg: context.Generator.EmitStarg(Index); break;

                case IoType.Flag:   EmitStloc(context, Index, RegisterType.Flag);   break;
                case IoType.Int:    EmitStloc(context, Index, RegisterType.Int);    break;
                case IoType.Vector: EmitStloc(context, Index, RegisterType.Vector); break;
            }
        }

        private void EmitStloc(ILMethodBuilder context, int index, RegisterType registerType)
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