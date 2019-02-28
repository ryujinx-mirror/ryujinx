using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeStore : IILEmit
    {
        public int Index { get; }

        public VarType VarType { get; }

        public RegisterSize RegisterSize { get; }

        public ILOpCodeStore(int index, VarType varType, RegisterSize registerSize = 0)
        {
            Index        = index;
            VarType      = varType;
            RegisterSize = registerSize;
        }

        public void Emit(ILMethodBuilder context)
        {
            switch (VarType)
            {
                case VarType.Arg: context.Generator.EmitStarg(Index); break;

                case VarType.Flag:   EmitStloc(context, Index, RegisterType.Flag);   break;
                case VarType.Int:    EmitStloc(context, Index, RegisterType.Int);    break;
                case VarType.Vector: EmitStloc(context, Index, RegisterType.Vector); break;
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