using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeCall : IILEmit
    {
        public MethodInfo Info { get; }

        public bool IsVirtual { get; }

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