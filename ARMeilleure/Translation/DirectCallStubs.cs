using ARMeilleure.Instructions;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using System;
using System.Runtime.InteropServices;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    static class DirectCallStubs
    {
        private delegate long GuestFunction(IntPtr nativeContextPtr);

        private static GuestFunction _directCallStub;
        private static GuestFunction _directTailCallStub;
        private static GuestFunction _indirectCallStub;
        private static GuestFunction _indirectTailCallStub;

        private static object _lock;
        private static bool _initialized;

        static DirectCallStubs()
        {
            _lock = new object();
        }

        public static void InitializeStubs()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                _directCallStub = GenerateDirectCallStub(false);
                _directTailCallStub = GenerateDirectCallStub(true);
                _indirectCallStub = GenerateIndirectCallStub(false);
                _indirectTailCallStub = GenerateIndirectCallStub(true);
                _initialized = true;
            }
        }

        public static IntPtr DirectCallStub(bool tailCall)
        {
            return Marshal.GetFunctionPointerForDelegate(tailCall ? _directTailCallStub : _directCallStub);
        }

        public static IntPtr IndirectCallStub(bool tailCall)
        {
            return Marshal.GetFunctionPointerForDelegate(tailCall ? _indirectTailCallStub : _indirectCallStub);
        }

        private static void EmitCall(EmitterContext context, Operand address, bool tailCall)
        {
            if (tailCall)
            {
                context.Tailcall(address, context.LoadArgument(OperandType.I64, 0));
            }
            else
            {
                context.Return(context.Call(address, OperandType.I64, context.LoadArgument(OperandType.I64, 0)));
            }
        }

        /// <summary>
        /// Generates a stub that is used to find function addresses. Used for direct calls when their jump table does not have the host address yet.
        /// Takes a NativeContext like a translated guest function, and extracts the target address from the NativeContext.
        /// When the target function is compiled in highCq, all table entries are updated to point to that function instead of this stub by the translator.
        /// </summary>
        private static GuestFunction GenerateDirectCallStub(bool tailCall)
        {
            EmitterContext context = new EmitterContext();

            Operand nativeContextPtr = context.LoadArgument(OperandType.I64, 0);

            Operand address = context.Load(OperandType.I64, context.Add(nativeContextPtr, Const((long)NativeContext.GetCallAddressOffset())));

            address = context.BitwiseOr(address, Const(address.Type, 1)); // Set call flag.
            Operand functionAddr = context.Call(new _U64_U64(NativeInterface.GetFunctionAddress), address);
            EmitCall(context, functionAddr, tailCall);

            ControlFlowGraph cfg = context.GetControlFlowGraph();

            OperandType[] argTypes = new OperandType[]
            {
                OperandType.I64
            };

            return Compiler.Compile<GuestFunction>(
                cfg,
                argTypes,
                OperandType.I64,
                CompilerOptions.HighCq);
        }

        /// <summary>
        /// Generates a stub that is used to find function addresses and add them to an indirect table. 
        /// Used for indirect calls entries (already claimed) when their jump table does not have the host address yet.
        /// Takes a NativeContext like a translated guest function, and extracts the target indirect table entry from the NativeContext.
        /// If the function we find is highCq, the entry in the table is updated to point to that function rather than this stub.
        /// </summary>
        private static GuestFunction GenerateIndirectCallStub(bool tailCall)
        {
            EmitterContext context = new EmitterContext();

            Operand nativeContextPtr = context.LoadArgument(OperandType.I64, 0);

            Operand entryAddress = context.Load(OperandType.I64, context.Add(nativeContextPtr, Const((long)NativeContext.GetCallAddressOffset())));
            Operand address = context.Load(OperandType.I64, entryAddress);

            // We need to find the missing function. If the function is HighCq, then it replaces this stub in the indirect table.
            // Either way, we call it afterwards.
            Operand functionAddr = context.Call(new _U64_U64_U64(NativeInterface.GetIndirectFunctionAddress), address, entryAddress);

            // Call and save the function.
            EmitCall(context, functionAddr, tailCall);

            ControlFlowGraph cfg = context.GetControlFlowGraph();

            OperandType[] argTypes = new OperandType[]
            {
                OperandType.I64
            };

            return Compiler.Compile<GuestFunction>(
                cfg,
                argTypes,
                OperandType.I64,
                CompilerOptions.HighCq);
        }
    }
}
