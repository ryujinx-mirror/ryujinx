using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    class EmitterContext
    {
        private Dictionary<Operand, BasicBlock> _irLabels;

        private LinkedList<BasicBlock> _irBlocks;

        private BasicBlock _irBlock;

        private bool _needsNewBlock;

        public EmitterContext()
        {
            _irLabels = new Dictionary<Operand, BasicBlock>();

            _irBlocks = new LinkedList<BasicBlock>();

            _needsNewBlock = true;
        }

        public Operand Add(Operand op1, Operand op2)
        {
            return Add(Instruction.Add, Local(op1.Type), op1, op2);
        }

        public Operand BitwiseAnd(Operand op1, Operand op2)
        {
            return Add(Instruction.BitwiseAnd, Local(op1.Type), op1, op2);
        }

        public Operand BitwiseExclusiveOr(Operand op1, Operand op2)
        {
            return Add(Instruction.BitwiseExclusiveOr, Local(op1.Type), op1, op2);
        }

        public Operand BitwiseNot(Operand op1)
        {
            return Add(Instruction.BitwiseNot, Local(op1.Type), op1);
        }

        public Operand BitwiseOr(Operand op1, Operand op2)
        {
            return Add(Instruction.BitwiseOr, Local(op1.Type), op1, op2);
        }

        public void Branch(Operand label)
        {
            Add(Instruction.Branch, null);

            BranchToLabel(label);
        }

        public void BranchIfFalse(Operand label, Operand op1)
        {
            Add(Instruction.BranchIfFalse, null, op1);

            BranchToLabel(label);
        }

        public void BranchIfTrue(Operand label, Operand op1)
        {
            Add(Instruction.BranchIfTrue, null, op1);

            BranchToLabel(label);
        }

        public Operand ByteSwap(Operand op1)
        {
            return Add(Instruction.ByteSwap, Local(op1.Type), op1);
        }

        public Operand Call(Delegate func, params Operand[] callArgs)
        {
            // Add the delegate to the cache to ensure it will not be garbage collected.
            func = DelegateCache.GetOrAdd(func);

            IntPtr ptr = Marshal.GetFunctionPointerForDelegate<Delegate>(func);

            OperandType returnType = GetOperandType(func.Method.ReturnType);

            return Call(Const(ptr.ToInt64()), returnType, callArgs);
        }

        private static Dictionary<TypeCode, OperandType> _typeCodeToOperandTypeMap =
                   new Dictionary<TypeCode, OperandType>()
        {
            { TypeCode.Boolean, OperandType.I32  },
            { TypeCode.Byte,    OperandType.I32  },
            { TypeCode.Char,    OperandType.I32  },
            { TypeCode.Double,  OperandType.FP64 },
            { TypeCode.Int16,   OperandType.I32  },
            { TypeCode.Int32,   OperandType.I32  },
            { TypeCode.Int64,   OperandType.I64  },
            { TypeCode.SByte,   OperandType.I32  },
            { TypeCode.Single,  OperandType.FP32 },
            { TypeCode.UInt16,  OperandType.I32  },
            { TypeCode.UInt32,  OperandType.I32  },
            { TypeCode.UInt64,  OperandType.I64  }
        };

        private static OperandType GetOperandType(Type type)
        {
            if (_typeCodeToOperandTypeMap.TryGetValue(Type.GetTypeCode(type), out OperandType ot))
            {
                return ot;
            }
            else if (type == typeof(V128))
            {
                return OperandType.V128;
            }
            else if (type == typeof(void))
            {
                return OperandType.None;
            }

            throw new ArgumentException($"Invalid type \"{type.Name}\".");
        }

        public Operand Call(Operand address, OperandType returnType, params Operand[] callArgs)
        {
            Operand[] args = new Operand[callArgs.Length + 1];

            args[0] = address;

            Array.Copy(callArgs, 0, args, 1, callArgs.Length);

            if (returnType != OperandType.None)
            {
                return Add(Instruction.Call, Local(returnType), args);
            }
            else
            {
                return Add(Instruction.Call, null, args);
            }
        }

        public Operand CompareAndSwap128(Operand address, Operand expected, Operand desired)
        {
            return Add(Instruction.CompareAndSwap128, Local(OperandType.V128), address, expected, desired);
        }

        public Operand ConditionalSelect(Operand op1, Operand op2, Operand op3)
        {
            return Add(Instruction.ConditionalSelect, Local(op2.Type), op1, op2, op3);
        }

        public Operand ConvertI64ToI32(Operand op1)
        {
            if (op1.Type != OperandType.I64)
            {
                throw new ArgumentException($"Invalid operand type \"{op1.Type}\".");
            }

            return Add(Instruction.ConvertI64ToI32, Local(OperandType.I32), op1);
        }

        public Operand ConvertToFP(OperandType type, Operand op1)
        {
            return Add(Instruction.ConvertToFP, Local(type), op1);
        }

        public Operand ConvertToFPUI(OperandType type, Operand op1)
        {
            return Add(Instruction.ConvertToFPUI, Local(type), op1);
        }

        public Operand Copy(Operand op1)
        {
            return Add(Instruction.Copy, Local(op1.Type), op1);
        }

        public Operand Copy(Operand dest, Operand op1)
        {
            if (dest.Kind != OperandKind.Register)
            {
                throw new ArgumentException($"Invalid dest operand kind \"{dest.Kind}\".");
            }

            return Add(Instruction.Copy, dest, op1);
        }

        public Operand CountLeadingZeros(Operand op1)
        {
            return Add(Instruction.CountLeadingZeros, Local(op1.Type), op1);
        }

        internal Operand CpuId()
        {
            return Add(Instruction.CpuId, Local(OperandType.I64));
        }

        public Operand Divide(Operand op1, Operand op2)
        {
            return Add(Instruction.Divide, Local(op1.Type), op1, op2);
        }

        public Operand DivideUI(Operand op1, Operand op2)
        {
            return Add(Instruction.DivideUI, Local(op1.Type), op1, op2);
        }

        public Operand ICompareEqual(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareEqual, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareGreater(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareGreater, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareGreaterOrEqual(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareGreaterOrEqual, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareGreaterOrEqualUI(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareGreaterOrEqualUI, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareGreaterUI(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareGreaterUI, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareLess(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareLess, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareLessOrEqual(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareLessOrEqual, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareLessOrEqualUI(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareLessOrEqualUI, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareLessUI(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareLessUI, Local(OperandType.I32), op1, op2);
        }

        public Operand ICompareNotEqual(Operand op1, Operand op2)
        {
            return Add(Instruction.CompareNotEqual, Local(OperandType.I32), op1, op2);
        }

        public Operand Load(OperandType type, Operand address)
        {
            return Add(Instruction.Load, Local(type), address);
        }

        public Operand Load16(Operand address)
        {
            return Add(Instruction.Load16, Local(OperandType.I32), address);
        }

        public Operand Load8(Operand address)
        {
            return Add(Instruction.Load8, Local(OperandType.I32), address);
        }

        public Operand LoadArgument(OperandType type, int index)
        {
            return Add(Instruction.LoadArgument, Local(type), Const(index));
        }

        public void LoadFromContext()
        {
            _needsNewBlock = true;

            Add(Instruction.LoadFromContext);
        }

        public Operand Multiply(Operand op1, Operand op2)
        {
            return Add(Instruction.Multiply, Local(op1.Type), op1, op2);
        }

        public Operand Multiply64HighSI(Operand op1, Operand op2)
        {
            return Add(Instruction.Multiply64HighSI, Local(OperandType.I64), op1, op2);
        }

        public Operand Multiply64HighUI(Operand op1, Operand op2)
        {
            return Add(Instruction.Multiply64HighUI, Local(OperandType.I64), op1, op2);
        }

        public Operand Negate(Operand op1)
        {
            return Add(Instruction.Negate, Local(op1.Type), op1);
        }

        public void Return()
        {
            Add(Instruction.Return);

            _needsNewBlock = true;
        }

        public void Return(Operand op1)
        {
            Add(Instruction.Return, null, op1);

            _needsNewBlock = true;
        }

        public Operand RotateRight(Operand op1, Operand op2)
        {
            return Add(Instruction.RotateRight, Local(op1.Type), op1, op2);
        }

        public Operand ShiftLeft(Operand op1, Operand op2)
        {
            return Add(Instruction.ShiftLeft, Local(op1.Type), op1, op2);
        }

        public Operand ShiftRightSI(Operand op1, Operand op2)
        {
            return Add(Instruction.ShiftRightSI, Local(op1.Type), op1, op2);
        }

        public Operand ShiftRightUI(Operand op1, Operand op2)
        {
            return Add(Instruction.ShiftRightUI, Local(op1.Type), op1, op2);
        }

        public Operand SignExtend16(OperandType type, Operand op1)
        {
            return Add(Instruction.SignExtend16, Local(type), op1);
        }

        public Operand SignExtend32(OperandType type, Operand op1)
        {
            return Add(Instruction.SignExtend32, Local(type), op1);
        }

        public Operand SignExtend8(OperandType type, Operand op1)
        {
            return Add(Instruction.SignExtend8, Local(type), op1);
        }

        public void Store(Operand address, Operand value)
        {
            Add(Instruction.Store, null, address, value);
        }

        public void Store16(Operand address, Operand value)
        {
            Add(Instruction.Store16, null, address, value);
        }

        public void Store8(Operand address, Operand value)
        {
            Add(Instruction.Store8, null, address, value);
        }

        public void StoreToContext()
        {
            Add(Instruction.StoreToContext);

            _needsNewBlock = true;
        }

        public Operand Subtract(Operand op1, Operand op2)
        {
            return Add(Instruction.Subtract, Local(op1.Type), op1, op2);
        }

        public Operand VectorCreateScalar(Operand value)
        {
            return Add(Instruction.VectorCreateScalar, Local(OperandType.V128), value);
        }

        public Operand VectorExtract(OperandType type, Operand vector, int index)
        {
            return Add(Instruction.VectorExtract, Local(type), vector, Const(index));
        }

        public Operand VectorExtract16(Operand vector, int index)
        {
            return Add(Instruction.VectorExtract16, Local(OperandType.I32), vector, Const(index));
        }

        public Operand VectorExtract8(Operand vector, int index)
        {
            return Add(Instruction.VectorExtract8, Local(OperandType.I32), vector, Const(index));
        }

        public Operand VectorInsert(Operand vector, Operand value, int index)
        {
            return Add(Instruction.VectorInsert, Local(OperandType.V128), vector, value, Const(index));
        }

        public Operand VectorInsert16(Operand vector, Operand value, int index)
        {
            return Add(Instruction.VectorInsert16, Local(OperandType.V128), vector, value, Const(index));
        }

        public Operand VectorInsert8(Operand vector, Operand value, int index)
        {
            return Add(Instruction.VectorInsert8, Local(OperandType.V128), vector, value, Const(index));
        }

        public Operand VectorZero()
        {
            return Add(Instruction.VectorZero, Local(OperandType.V128));
        }

        public Operand VectorZeroUpper64(Operand vector)
        {
            return Add(Instruction.VectorZeroUpper64, Local(OperandType.V128), vector);
        }

        public Operand VectorZeroUpper96(Operand vector)
        {
            return Add(Instruction.VectorZeroUpper96, Local(OperandType.V128), vector);
        }

        public Operand ZeroExtend16(OperandType type, Operand op1)
        {
            return Add(Instruction.ZeroExtend16, Local(type), op1);
        }

        public Operand ZeroExtend32(OperandType type, Operand op1)
        {
            return Add(Instruction.ZeroExtend32, Local(type), op1);
        }

        public Operand ZeroExtend8(OperandType type, Operand op1)
        {
            return Add(Instruction.ZeroExtend8, Local(type), op1);
        }

        private Operand Add(Instruction inst, Operand dest = null, params Operand[] sources)
        {
            if (_needsNewBlock)
            {
                NewNextBlock();
            }

            Operation operation = new Operation(inst, dest, sources);

            _irBlock.Operations.AddLast(operation);

            return dest;
        }

        public Operand AddIntrinsic(Intrinsic intrin, params Operand[] args)
        {
            return Add(intrin, Local(OperandType.V128), args);
        }

        public Operand AddIntrinsicInt(Intrinsic intrin, params Operand[] args)
        {
            return Add(intrin, Local(OperandType.I32), args);
        }

        public Operand AddIntrinsicLong(Intrinsic intrin, params Operand[] args)
        {
            return Add(intrin, Local(OperandType.I64), args);
        }

        private Operand Add(Intrinsic intrin, Operand dest, params Operand[] sources)
        {
            if (_needsNewBlock)
            {
                NewNextBlock();
            }

            IntrinsicOperation operation = new IntrinsicOperation(intrin, dest, sources);

            _irBlock.Operations.AddLast(operation);

            return dest;
        }

        private void BranchToLabel(Operand label)
        {
            if (!_irLabels.TryGetValue(label, out BasicBlock branchBlock))
            {
                branchBlock = new BasicBlock();

                _irLabels.Add(label, branchBlock);
            }

            _irBlock.Branch = branchBlock;

            _needsNewBlock = true;
        }

        public void MarkLabel(Operand label)
        {
            if (_irLabels.TryGetValue(label, out BasicBlock nextBlock))
            {
                nextBlock.Index = _irBlocks.Count;
                nextBlock.Node  = _irBlocks.AddLast(nextBlock);

                NextBlock(nextBlock);
            }
            else
            {
                NewNextBlock();

                _irLabels.Add(label, _irBlock);
            }
        }

        private void NewNextBlock()
        {
            BasicBlock block = new BasicBlock(_irBlocks.Count);

            block.Node = _irBlocks.AddLast(block);

            NextBlock(block);
        }

        private void NextBlock(BasicBlock nextBlock)
        {
            if (_irBlock != null && !EndsWithUnconditional(_irBlock))
            {
                _irBlock.Next = nextBlock;
            }

            _irBlock = nextBlock;

            _needsNewBlock = false;
        }

        private static bool EndsWithUnconditional(BasicBlock block)
        {
            Operation lastOp = block.GetLastOp() as Operation;

            if (lastOp == null)
            {
                return false;
            }

            return lastOp.Instruction == Instruction.Branch ||
                   lastOp.Instruction == Instruction.Return;
        }

        public ControlFlowGraph GetControlFlowGraph()
        {
            return new ControlFlowGraph(_irBlocks.First.Value, _irBlocks);
        }
    }
}