using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;

namespace ARMeilleure.Memory
{
    static class MemoryManagerPal
    {
        private delegate V128 CompareExchange128(IntPtr address, V128 expected, V128 desired);

        private static CompareExchange128 _compareExchange128;

        private static object _lock;

        static MemoryManagerPal()
        {
            _lock = new object();
        }

        public static V128 AtomicLoad128(IntPtr address)
        {
            return GetCompareAndSwap128()(address, V128.Zero, V128.Zero);
        }

        public static V128 CompareAndSwap128(IntPtr address, V128 expected, V128 desired)
        {
            return GetCompareAndSwap128()(address, expected, desired);
        }

        private static CompareExchange128 GetCompareAndSwap128()
        {
            if (_compareExchange128 == null)
            {
                GenerateCompareAndSwap128();
            }

            return _compareExchange128;
        }

        private static void GenerateCompareAndSwap128()
        {
            lock (_lock)
            {
                if (_compareExchange128 != null)
                {
                    return;
                }

                EmitterContext context = new EmitterContext();

                Operand address  = context.LoadArgument(OperandType.I64,  0);
                Operand expected = context.LoadArgument(OperandType.V128, 1);
                Operand desired  = context.LoadArgument(OperandType.V128, 2);

                Operand result = context.CompareAndSwap(address, expected, desired);

                context.Return(result);

                ControlFlowGraph cfg = context.GetControlFlowGraph();

                OperandType[] argTypes = new OperandType[]
                {
                    OperandType.I64,
                    OperandType.V128,
                    OperandType.V128
                };

                _compareExchange128 = Compiler.Compile<CompareExchange128>(
                    cfg,
                    argTypes,
                    OperandType.V128,
                    CompilerOptions.HighCq);
            }
        }
    }
}