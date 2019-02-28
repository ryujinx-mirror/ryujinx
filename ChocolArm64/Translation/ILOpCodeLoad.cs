using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeLoad : IILEmit
    {
        public int Index { get; }

        public VarType VarType { get; }

        public RegisterSize RegisterSize { get; }

        public ILOpCodeLoad(int index, VarType varType, RegisterSize registerSize = 0)
        {
            Index        = index;
            VarType      = varType;
            RegisterSize = registerSize;
        }

        public void Emit(ILMethodBuilder context)
        {
            switch (VarType)
            {
                case VarType.Arg: context.Generator.EmitLdarg(Index); break;

                case VarType.Flag:   EmitLdloc(context, Index, RegisterType.Flag);   break;
                case VarType.Int:    EmitLdloc(context, Index, RegisterType.Int);    break;
                case VarType.Vector: EmitLdloc(context, Index, RegisterType.Vector); break;
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