using ChocolArm64.State;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.IntermediateRepresentation
{
    class Operation
    {
        public BasicBlock Parent { get; set; }

        public OperationType Type { get; }

        private object[] _arguments { get; }

        private Operation(OperationType type, params object[] arguments)
        {
            Type       = type;
            _arguments = arguments;
        }

        public T GetArg<T>(int index)
        {
            return (T)GetArg(index);
        }

        public object GetArg(int index)
        {
            if ((uint)index >= _arguments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _arguments[index];
        }

        public static Operation Call(MethodInfo info)
        {
            return new Operation(OperationType.Call, info);
        }

        public static Operation CallVirtual(MethodInfo info)
        {
            return new Operation(OperationType.CallVirtual, info);
        }

        public static Operation IL(OpCode ilOp)
        {
            return new Operation(OperationType.IL, ilOp);
        }

        public static Operation ILBranch(OpCode ilOp, ILLabel target)
        {
            return new Operation(OperationType.ILBranch, ilOp, target);
        }

        public static Operation LoadArgument(int index)
        {
            return new Operation(OperationType.LoadArgument, index);
        }

        public static Operation LoadConstant(int value)
        {
            return new Operation(OperationType.LoadConstant, value);
        }

        public static Operation LoadConstant(long value)
        {
            return new Operation(OperationType.LoadConstant, value);
        }

        public static Operation LoadConstant(float value)
        {
            return new Operation(OperationType.LoadConstant, value);
        }

        public static Operation LoadConstant(double value)
        {
            return new Operation(OperationType.LoadConstant, value);
        }

        public static Operation LoadContext()
        {
            return new Operation(OperationType.LoadContext);
        }

        public static Operation LoadField(FieldInfo info)
        {
            return new Operation(OperationType.LoadField, info);
        }

        public static Operation LoadLocal(int index, RegisterType type, RegisterSize size)
        {
            return new Operation(OperationType.LoadLocal, index, type, size);
        }

        public static Operation MarkLabel(ILLabel label)
        {
            return new Operation(OperationType.MarkLabel, label);
        }

        public static Operation StoreContext()
        {
            return new Operation(OperationType.StoreContext);
        }

        public static Operation StoreLocal(int index, RegisterType type, RegisterSize size)
        {
            return new Operation(OperationType.StoreLocal, index, type, size);
        }
    }
}