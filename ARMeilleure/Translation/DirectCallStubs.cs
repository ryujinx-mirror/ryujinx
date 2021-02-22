using ARMeilleure.Instructions;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    static class DirectCallStubs
    {
        private delegate long GuestFunction(IntPtr nativeContextPtr);

        private static IntPtr _directCallStubPtr;
        private static IntPtr _directTailCallStubPtr;
        private static IntPtr _indirectCallStubPtr;
        private static IntPtr _indirectTailCallStubPtr;

        private static readonly object _lock = new object();
        private static bool _initialized;

        public static void InitializeStubs()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                Translator.PreparePool();

                _directCallStubPtr       = Marshal.GetFunctionPointerForDelegate<GuestFunction>(GenerateDirectCallStub(false));
                _directTailCallStubPtr   = Marshal.GetFunctionPointerForDelegate<GuestFunction>(GenerateDirectCallStub(true));
                _indirectCallStubPtr     = Marshal.GetFunctionPointerForDelegate<GuestFunction>(GenerateIndirectCallStub(false));
                _indirectTailCallStubPtr = Marshal.GetFunctionPointerForDelegate<GuestFunction>(GenerateIndirectCallStub(true));

                Translator.ResetPool();

                Translator.DisposePools();

                _initialized = true;
            }
        }

        public static IntPtr DirectCallStub(bool tailCall)
        {
            Debug.Assert(_initialized);

            return tailCall ? _directTailCallStubPtr : _directCallStubPtr;
        }

        public static IntPtr IndirectCallStub(bool tailCall)
        {
            Debug.Assert(_initialized);

            return tailCall ? _indirectTailCallStubPtr : _indirectCallStubPtr;
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

            Operand functionAddr = context.Call(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFunctionAddress)), address);
            EmitCall(context, functionAddr, tailCall);

            ControlFlowGraph cfg = context.GetControlFlowGraph();

            OperandType[] argTypes = new OperandType[] { OperandType.I64 };

            return Compiler.Compile<GuestFunction>(cfg, argTypes, OperandType.I64, CompilerOptions.HighCq);
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
            Operand functionAddr = context.Call(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetIndirectFunctionAddress)), address, entryAddress);

            // Call and save the function.
            EmitCall(context, functionAddr, tailCall);

            ControlFlowGraph cfg = context.GetControlFlowGraph();

            OperandType[] argTypes = new OperandType[] { OperandType.I64 };

            return Compiler.Compile<GuestFunction>(cfg, argTypes, OperandType.I64, CompilerOptions.HighCq);
        }
    }
}
