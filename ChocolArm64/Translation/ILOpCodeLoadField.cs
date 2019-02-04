using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeLoadField : IILEmit
    {
        public FieldInfo Info { get; private set; }

        public ILOpCodeLoadField(FieldInfo info)
        {
            Info = info;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.Emit(OpCodes.Ldfld, Info);
        }
    }
}