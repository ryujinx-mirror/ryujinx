using ARMeilleure.Common;

namespace ARMeilleure.IntermediateRepresentation
{
    static class OperandHelper
    {
        public static Operand Const(OperandType type, long value)
        {
            return type == OperandType.I32 ? Operand().With((int)value) : Operand().With(value);
        }

        public static Operand Const(bool value)
        {
            return Operand().With(value ? 1 : 0);
        }

        public static Operand Const(int value)
        {
            return Operand().With(value);
        }

        public static Operand Const(uint value)
        {
            return Operand().With(value);
        }

        public static Operand Const(long value, bool relocatable = false, int? index = null)
        {
            return Operand().With(value, relocatable, index);
        }

        public static Operand Const(ulong value)
        {
            return Operand().With(value);
        }

        public static Operand ConstF(float value)
        {
            return Operand().With(value);
        }

        public static Operand ConstF(double value)
        {
            return Operand().With(value);
        }

        public static Operand Label()
        {
            return Operand().With(OperandKind.Label);
        }

        public static Operand Local(OperandType type)
        {
            return Operand().With(OperandKind.LocalVariable, type);
        }

        public static Operand Register(int index, RegisterType regType, OperandType type)
        {
            return Operand().With(index, regType, type);
        }

        public static Operand Undef()
        {
            return Operand().With(OperandKind.Undefined);
        }

        public static MemoryOperand MemoryOp(
            OperandType type,
            Operand baseAddress,
            Operand index = null,
            Multiplier scale = Multiplier.x1,
            int displacement = 0)
        {
            return MemoryOperand().With(type, baseAddress, index, scale, displacement);
        }

        #region "ThreadStaticPool"
        public static void PrepareOperandPool(int groupId = 0)
        {
            ThreadStaticPool<Operand>.PreparePool(groupId, ChunkSizeLimit.Large);
            ThreadStaticPool<MemoryOperand>.PreparePool(groupId, ChunkSizeLimit.Small);
        }

        private static Operand Operand()
        {
            return ThreadStaticPool<Operand>.Instance.Allocate();
        }

        private static MemoryOperand MemoryOperand()
        {
            return ThreadStaticPool<MemoryOperand>.Instance.Allocate();
        }

        public static void ResetOperandPool(int groupId = 0)
        {
            ThreadStaticPool<MemoryOperand>.ResetPool(groupId);
            ThreadStaticPool<Operand>.ResetPool(groupId);
        }

        public static void DisposeOperandPools()
        {
            ThreadStaticPool<Operand>.DisposePools();
            ThreadStaticPool<MemoryOperand>.DisposePools();
        }
        #endregion
    }
}
