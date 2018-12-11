using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ChocolArm64.Translation
{
    class ILOpCodeConst : IILEmit
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private struct ImmVal
        {
            [FieldOffset(0)] public int    I4;
            [FieldOffset(0)] public long   I8;
            [FieldOffset(0)] public float  R4;
            [FieldOffset(0)] public double R8;
        }

        private ImmVal _value;

        private enum ConstType
        {
            Int32,
            Int64,
            Single,
            Double
        }

        private ConstType _type;

        private ILOpCodeConst(ConstType type)
        {
            _type = type;
        }

        public ILOpCodeConst(int value) : this(ConstType.Int32)
        {
            _value = new ImmVal { I4 = value };
        }

        public ILOpCodeConst(long value) : this(ConstType.Int64)
        {
            _value = new ImmVal { I8 = value };
        }

        public ILOpCodeConst(float value) : this(ConstType.Single)
        {
            _value = new ImmVal { R4 = value };
        }

        public ILOpCodeConst(double value) : this(ConstType.Double)
        {
            _value = new ImmVal { R8 = value };
        }

        public void Emit(ILMethodBuilder context)
        {
            switch (_type)
            {
                case ConstType.Int32:  context.Generator.EmitLdc_I4(_value.I4);           break;
                case ConstType.Int64:  context.Generator.Emit(OpCodes.Ldc_I8, _value.I8); break;
                case ConstType.Single: context.Generator.Emit(OpCodes.Ldc_R4, _value.R4); break;
                case ConstType.Double: context.Generator.Emit(OpCodes.Ldc_R8, _value.R8); break;
            }
        }
    }
}