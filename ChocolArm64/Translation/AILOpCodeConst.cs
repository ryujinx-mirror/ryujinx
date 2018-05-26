using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ChocolArm64.Translation
{
    class AILOpCodeConst : IAILEmit
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private struct ImmVal
        {
            [FieldOffset(0)] public int    I4;
            [FieldOffset(0)] public long   I8;
            [FieldOffset(0)] public float  R4;
            [FieldOffset(0)] public double R8;
        }

        private ImmVal Value;

        private enum ConstType
        {
            Int32,
            Int64,
            Single,
            Double
        }

        private ConstType Type;

        private AILOpCodeConst(ConstType Type)
        {
            this.Type = Type;
        }

        public AILOpCodeConst(int Value) : this(ConstType.Int32)
        {
            this.Value = new ImmVal { I4 = Value };
        }

        public AILOpCodeConst(long Value) : this(ConstType.Int64)
        {
            this.Value = new ImmVal { I8 = Value };
        }

        public AILOpCodeConst(float Value) : this(ConstType.Single)
        {
            this.Value = new ImmVal { R4 = Value };
        }

        public AILOpCodeConst(double Value) : this(ConstType.Double)
        {
            this.Value = new ImmVal { R8 = Value };
        }

        public void Emit(AILEmitter Context)
        {
            switch (Type)
            {
                case ConstType.Int32:  Context.Generator.EmitLdc_I4(Value.I4);           break;
                case ConstType.Int64:  Context.Generator.Emit(OpCodes.Ldc_I8, Value.I8); break;
                case ConstType.Single: Context.Generator.Emit(OpCodes.Ldc_R4, Value.R4); break;
                case ConstType.Double: Context.Generator.Emit(OpCodes.Ldc_R8, Value.R8); break;
            }
        }
    }
}