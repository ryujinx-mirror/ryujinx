using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeCall : IILEmit
    {
        public MethodInfo Info { get; private set; }

        public bool IsVirtual { get; private set; }

        public ILOpCodeCall(MethodInfo info, bool isVirtual)
        {
            Info      = info;
            IsVirtual = isVirtual;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.Emit(IsVirtual ? OpCodes.Callvirt : OpCodes.Call, Info);
        }
    }
}