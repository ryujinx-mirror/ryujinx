using ChocolArm64.Decoders;
using ChocolArm64.Instructions;
using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class ILEmitterCtx
    {
        public MemoryManager Memory { get; }

        private TranslatorCache _cache;
        private TranslatorQueue _queue;

        private Block _currBlock;

        public Block CurrBlock
        {
            get
            {
                return _currBlock;
            }
            set
            {
                _currBlock = value;

                ResetBlockState();
            }
        }

        public OpCode64 CurrOp { get; set; }

        public TranslationTier Tier { get; }

        public Aarch32Mode Mode { get; } = Aarch32Mode.User; //TODO

        public bool HasIndirectJump { get; set; }

        public bool HasSlowCall { get; set; }

        private Dictionary<long, ILLabel> _labels;

        private Dictionary<ILLabel, BasicBlock> _irLabels;

        private List<BasicBlock> _irBlocks;

        private BasicBlock _irBlock;

        private bool _needsNewBlock;

        private OpCode64 _optOpLastCompare;
        private OpCode64 _optOpLastFlagSet;

        //This is the index of the temporary register, used to store temporary
        //values needed by some functions, since IL doesn't have a swap instruction.
        //You can use any value here as long it doesn't conflict with the indices
        //for the other registers. Any value >= 64 or < 0 will do.
        private const int ReservedLocalsCount = 64;

        private const int RorTmpIndex      = ReservedLocalsCount + 0;
        private const int CmpOptTmp1Index  = ReservedLocalsCount + 1;
        private const int CmpOptTmp2Index  = ReservedLocalsCount + 2;
        private const int IntGpTmp1Index   = ReservedLocalsCount + 3;
        private const int IntGpTmp2Index   = ReservedLocalsCount + 4;
        private const int UserIntTempStart = ReservedLocalsCount + 5;

        //Vectors are part of another "set" of locals.
        private const int VecGpTmp1Index   = ReservedLocalsCount + 0;
        private const int VecGpTmp2Index   = ReservedLocalsCount + 1;
        private const int VecGpTmp3Index   = ReservedLocalsCount + 2;
        private const int UserVecTempStart = ReservedLocalsCount + 3;

        private static int _userIntTempCount;
        private static int _userVecTempCount;

        public ILEmitterCtx(
            MemoryManager   memory,
            TranslatorCache cache,
            TranslatorQueue queue,
            TranslationTier tier)
        {
            Memory = memory ?? throw new ArgumentNullException(nameof(memory));
            _cache = cache  ?? throw new ArgumentNullException(nameof(cache));
            _queue = queue  ?? throw new ArgumentNullException(nameof(queue));

            Tier = tier;

            _labels = new Dictionary<long, ILLabel>();

            _irLabels = new Dictionary<ILLabel, BasicBlock>();

            _irBlocks = new List<BasicBlock>();

            NewNextBlock();

            EmitSynchronization();

            EmitLoadContext();
        }

        public static int GetIntTempIndex()
        {
            return UserIntTempStart + _userIntTempCount++;
        }

        public static int GetVecTempIndex()
        {
            return UserVecTempStart + _userVecTempCount++;
        }

        public BasicBlock[] GetBlocks()
        {
            return _irBlocks.ToArray();
        }

        public void EmitSynchronization()
        {
            EmitLdarg(TranslatedSub.StateArgIdx);

            EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.Synchronize));

            EmitLdc_I4(0);

            ILLabel lblContinue = new ILLabel();

            Emit(OpCodes.Bne_Un_S, lblContinue);

            EmitLdc_I8(0);

            Emit(OpCodes.Ret);

            MarkLabel(lblContinue);
        }

        public void ResetBlockStateForPredicatedOp()
        {
            //Check if this is a predicated instruction that modifies flags,
            //in this case the value of the flags is unknown as we don't know
            //in advance if the instruction is going to be executed or not.
            //So, we reset the block state to prevent an invalid optimization.
            if (CurrOp == _optOpLastFlagSet)
            {
                ResetBlockState();
            }
        }

        private void ResetBlockState()
        {
            _optOpLastFlagSet = null;
            _optOpLastCompare = null;
        }

        public void TranslateAhead(long position, ExecutionMode mode = ExecutionMode.Aarch64)
        {
            if (_cache.TryGetSubroutine(position, out TranslatedSub sub) && sub.Tier != TranslationTier.Tier0)
            {
                return;
            }

            _queue.Enqueue(position, mode, TranslationTier.Tier1, isComplete: true);
        }

        public bool TryOptEmitSubroutineCall()
        {
            //Calls should always have a next block, unless
            //we're translating a single basic block.
            if (_currBlock.Next == null)
            {
                return false;
            }

            if (!(CurrOp is IOpCodeBImm op))
            {
                return false;
            }

            if (!_cache.TryGetSubroutine(op.Imm, out TranslatedSub sub) || sub.Tier != TranslationTier.Tier0)
            {
                return false;
            }

            EmitStoreContext();

            for (int index = 0; index < TranslatedSub.FixedArgTypes.Length; index++)
            {
                EmitLdarg(index);
            }

            EmitCall(sub.Method);

            return true;
        }

        public void TryOptMarkCondWithoutCmp()
        {
            _optOpLastCompare = CurrOp;

            InstEmitAluHelper.EmitAluLoadOpers(this);

            Stloc(CmpOptTmp2Index, RegisterType.Int);
            Stloc(CmpOptTmp1Index, RegisterType.Int);
        }

        private Dictionary<Condition, OpCode> _branchOps = new Dictionary<Condition, OpCode>()
        {
            { Condition.Eq,   OpCodes.Beq    },
            { Condition.Ne,   OpCodes.Bne_Un },
            { Condition.GeUn, OpCodes.Bge_Un },
            { Condition.LtUn, OpCodes.Blt_Un },
            { Condition.GtUn, OpCodes.Bgt_Un },
            { Condition.LeUn, OpCodes.Ble_Un },
            { Condition.Ge,   OpCodes.Bge    },
            { Condition.Lt,   OpCodes.Blt    },
            { Condition.Gt,   OpCodes.Bgt    },
            { Condition.Le,   OpCodes.Ble    }
        };

        public void EmitCondBranch(ILLabel target, Condition cond)
        {
            if (_optOpLastCompare != null &&
                _optOpLastCompare == _optOpLastFlagSet && _branchOps.ContainsKey(cond))
            {
                if (_optOpLastCompare.Emitter == InstEmit.Subs)
                {
                    Ldloc(CmpOptTmp1Index, RegisterType.Int, _optOpLastCompare.RegisterSize);
                    Ldloc(CmpOptTmp2Index, RegisterType.Int, _optOpLastCompare.RegisterSize);

                    Emit(_branchOps[cond], target);

                    return;
                }
                else if (_optOpLastCompare.Emitter == InstEmit.Adds && cond != Condition.GeUn
                                                                    && cond != Condition.LtUn
                                                                    && cond != Condition.GtUn
                                                                    && cond != Condition.LeUn)
                {
                    //There are several limitations that needs to be taken into account for CMN comparisons:
                    //- The unsigned comparisons are not valid, as they depend on the
                    //carry flag value, and they will have different values for addition and
                    //subtraction. For addition, it's carry, and for subtraction, it's borrow.
                    //So, we need to make sure we're not doing a unsigned compare for the CMN case.
                    //- We can only do the optimization for the immediate variants,
                    //because when the second operand value is exactly INT_MIN, we can't
                    //negate the value as theres no positive counterpart.
                    //Such invalid values can't be encoded on the immediate encodings.
                    if (_optOpLastCompare is IOpCodeAluImm64 op)
                    {
                        Ldloc(CmpOptTmp1Index, RegisterType.Int, _optOpLastCompare.RegisterSize);

                        if (_optOpLastCompare.RegisterSize == RegisterSize.Int32)
                        {
                            EmitLdc_I4((int)-op.Imm);
                        }
                        else
                        {
                            EmitLdc_I8(-op.Imm);
                        }

                        Emit(_branchOps[cond], target);

                        return;
                    }
                }
            }

            OpCode ilOp;

            int intCond = (int)cond;

            if (intCond < 14)
            {
                int condTrue = intCond >> 1;

                switch (condTrue)
                {
                    case 0: EmitLdflg((int)PState.ZBit); break;
                    case 1: EmitLdflg((int)PState.CBit); break;
                    case 2: EmitLdflg((int)PState.NBit); break;
                    case 3: EmitLdflg((int)PState.VBit); break;

                    case 4:
                        EmitLdflg((int)PState.CBit);
                        EmitLdflg((int)PState.ZBit);

                        Emit(OpCodes.Not);
                        Emit(OpCodes.And);
                        break;

                    case 5:
                    case 6:
                        EmitLdflg((int)PState.NBit);
                        EmitLdflg((int)PState.VBit);

                        Emit(OpCodes.Ceq);

                        if (condTrue == 6)
                        {
                            EmitLdflg((int)PState.ZBit);

                            Emit(OpCodes.Not);
                            Emit(OpCodes.And);
                        }
                        break;
                }

                ilOp = (intCond & 1) != 0 ? OpCodes.Brfalse : OpCodes.Brtrue;
            }
            else
            {
                ilOp = OpCodes.Br;
            }

            Emit(ilOp, target);
        }

        public void EmitCast(IntType intType)
        {
            switch (intType)
            {
                case IntType.UInt8:  Emit(OpCodes.Conv_U1); break;
                case IntType.UInt16: Emit(OpCodes.Conv_U2); break;
                case IntType.UInt32: Emit(OpCodes.Conv_U4); break;
                case IntType.UInt64: Emit(OpCodes.Conv_U8); break;
                case IntType.Int8:   Emit(OpCodes.Conv_I1); break;
                case IntType.Int16:  Emit(OpCodes.Conv_I2); break;
                case IntType.Int32:  Emit(OpCodes.Conv_I4); break;
                case IntType.Int64:  Emit(OpCodes.Conv_I8); break;
            }

            bool sz64 = CurrOp.RegisterSize != RegisterSize.Int32;

            if (sz64 == (intType == IntType.UInt64 || intType == IntType.Int64))
            {
                return;
            }

            if (sz64)
            {
                Emit(intType >= IntType.Int8 ? OpCodes.Conv_I8 : OpCodes.Conv_U8);
            }
            else
            {
                Emit(OpCodes.Conv_U4);
            }
        }

        public void EmitLsl(int amount) => EmitILShift(amount, OpCodes.Shl);
        public void EmitLsr(int amount) => EmitILShift(amount, OpCodes.Shr_Un);
        public void EmitAsr(int amount) => EmitILShift(amount, OpCodes.Shr);

        private void EmitILShift(int amount, OpCode ilOp)
        {
            if (amount > 0)
            {
                EmitLdc_I4(amount);

                Emit(ilOp);
            }
        }

        public void EmitRor(int amount)
        {
            if (amount > 0)
            {
                Stloc(RorTmpIndex, RegisterType.Int);
                Ldloc(RorTmpIndex, RegisterType.Int);

                EmitLdc_I4(amount);

                Emit(OpCodes.Shr_Un);

                Ldloc(RorTmpIndex, RegisterType.Int);

                EmitLdc_I4(CurrOp.GetBitsCount() - amount);

                Emit(OpCodes.Shl);
                Emit(OpCodes.Or);
            }
        }

        public ILLabel GetLabel(long position)
        {
            if (!_labels.TryGetValue(position, out ILLabel output))
            {
                output = new ILLabel();

                _labels.Add(position, output);
            }

            return output;
        }

        public void MarkLabel(ILLabel label)
        {
            if (_irLabels.TryGetValue(label, out BasicBlock nextBlock))
            {
                nextBlock.Index = _irBlocks.Count;

                _irBlocks.Add(nextBlock);

                NextBlock(nextBlock);
            }
            else
            {
                NewNextBlock();

                _irLabels.Add(label, _irBlock);
            }

            AddOperation(Operation.MarkLabel(label));
        }

        public void Emit(OpCode ilOp)
        {
            AddOperation(Operation.IL(ilOp));

            if (ilOp == OpCodes.Ret)
            {
                NextBlock(null);

                _needsNewBlock = true;
            }
        }

        public void Emit(OpCode ilOp, ILLabel label)
        {
            AddOperation(Operation.ILBranch(ilOp, label));

            _needsNewBlock = true;

            if (!_irLabels.TryGetValue(label, out BasicBlock branchBlock))
            {
                branchBlock = new BasicBlock();

                _irLabels.Add(label, branchBlock);
            }

            _irBlock.Branch = branchBlock;
        }

        public void EmitLdfld(FieldInfo info)
        {
            AddOperation(Operation.LoadField(info));
        }

        public void EmitLdarg(int index)
        {
            AddOperation(Operation.LoadArgument(index));
        }

        public void EmitLdintzr(int index)
        {
            if (index != RegisterAlias.Zr)
            {
                EmitLdint(index);
            }
            else
            {
                EmitLdc_I(0);
            }
        }

        public void EmitStintzr(int index)
        {
            if (index != RegisterAlias.Zr)
            {
                EmitStint(index);
            }
            else
            {
                Emit(OpCodes.Pop);
            }
        }

        public void EmitLoadContext()
        {
            _needsNewBlock = true;

            AddOperation(Operation.LoadContext());
        }

        public void EmitStoreContext()
        {
            AddOperation(Operation.StoreContext());
        }

        public void EmitLdtmp() => EmitLdint(IntGpTmp1Index);
        public void EmitSttmp() => EmitStint(IntGpTmp1Index);

        public void EmitLdtmp2() => EmitLdint(IntGpTmp2Index);
        public void EmitSttmp2() => EmitStint(IntGpTmp2Index);

        public void EmitLdvectmp() => EmitLdvec(VecGpTmp1Index);
        public void EmitStvectmp() => EmitStvec(VecGpTmp1Index);

        public void EmitLdvectmp2() => EmitLdvec(VecGpTmp2Index);
        public void EmitStvectmp2() => EmitStvec(VecGpTmp2Index);

        public void EmitLdvectmp3() => EmitLdvec(VecGpTmp3Index);
        public void EmitStvectmp3() => EmitStvec(VecGpTmp3Index);

        public void EmitLdint(int index) => Ldloc(index, RegisterType.Int);
        public void EmitStint(int index) => Stloc(index, RegisterType.Int);

        public void EmitLdvec(int index) => Ldloc(index, RegisterType.Vector);
        public void EmitStvec(int index) => Stloc(index, RegisterType.Vector);

        public void EmitLdflg(int index) => Ldloc(index, RegisterType.Flag);
        public void EmitStflg(int index)
        {
            //Set this only if any of the NZCV flag bits were modified.
            //This is used to ensure that when emiting a direct IL branch
            //instruction for compare + branch sequences, we're not expecting
            //to use comparison values from an old instruction, when in fact
            //the flags were already overwritten by another instruction further along.
            if (index >= (int)PState.VBit)
            {
                _optOpLastFlagSet = CurrOp;
            }

            Stloc(index, RegisterType.Flag);
        }

        private void Ldloc(int index, RegisterType type)
        {
            AddOperation(Operation.LoadLocal(index, type, CurrOp.RegisterSize));
        }

        private void Ldloc(int index, RegisterType type, RegisterSize size)
        {
            AddOperation(Operation.LoadLocal(index, type, size));
        }

        private void Stloc(int index, RegisterType type)
        {
            AddOperation(Operation.StoreLocal(index, type, CurrOp.RegisterSize));
        }

        public void EmitCallPropGet(Type objType, string propName)
        {
            EmitCall(objType, $"get_{propName}");
        }

        public void EmitCallPropSet(Type objType, string propName)
        {
            EmitCall(objType, $"set_{propName}");
        }

        public void EmitCall(Type objType, string mthdName)
        {
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType));
            }

            if (mthdName == null)
            {
                throw new ArgumentNullException(nameof(mthdName));
            }

            EmitCall(objType.GetMethod(mthdName));
        }

        public void EmitCallPrivatePropGet(Type objType, string propName)
        {
            EmitPrivateCall(objType, $"get_{propName}");
        }

        public void EmitCallPrivatePropSet(Type objType, string propName)
        {
            EmitPrivateCall(objType, $"set_{propName}");
        }

        public void EmitPrivateCall(Type objType, string mthdName)
        {
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType));
            }

            if (mthdName == null)
            {
                throw new ArgumentNullException(nameof(mthdName));
            }

            EmitCall(objType.GetMethod(mthdName, BindingFlags.Instance | BindingFlags.NonPublic));
        }

        public void EmitCall(MethodInfo mthdInfo, bool isVirtual = false)
        {
            if (mthdInfo == null)
            {
                throw new ArgumentNullException(nameof(mthdInfo));
            }

            if (isVirtual)
            {
                AddOperation(Operation.CallVirtual(mthdInfo));
            }
            else
            {
                AddOperation(Operation.Call(mthdInfo));
            }
        }

        public void EmitLdc_I(long value)
        {
            if (CurrOp.RegisterSize == RegisterSize.Int32)
            {
                EmitLdc_I4((int)value);
            }
            else
            {
                EmitLdc_I8(value);
            }
        }

        public void EmitLdc_I4(int value)
        {
            AddOperation(Operation.LoadConstant(value));
        }

        public void EmitLdc_I8(long value)
        {
            AddOperation(Operation.LoadConstant(value));
        }

        public void EmitLdc_R4(float value)
        {
            AddOperation(Operation.LoadConstant(value));
        }

        public void EmitLdc_R8(double value)
        {
            AddOperation(Operation.LoadConstant(value));
        }

        public void EmitZnFlagCheck()
        {
            EmitZnCheck(OpCodes.Ceq, (int)PState.ZBit);
            EmitZnCheck(OpCodes.Clt, (int)PState.NBit);
        }

        private void EmitZnCheck(OpCode ilCmpOp, int flag)
        {
            Emit(OpCodes.Dup);
            Emit(OpCodes.Ldc_I4_0);

            if (CurrOp.RegisterSize != RegisterSize.Int32)
            {
                Emit(OpCodes.Conv_I8);
            }

            Emit(ilCmpOp);

            EmitStflg(flag);
        }

        private void AddOperation(Operation operation)
        {
            if (_needsNewBlock)
            {
                NewNextBlock();
            }

            _irBlock.Add(operation);
        }

        private void NewNextBlock()
        {
            BasicBlock block = new BasicBlock(_irBlocks.Count);

            _irBlocks.Add(block);

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
            Operation lastOp = block.GetLastOp();

            if (lastOp == null || lastOp.Type != OperationType.ILBranch)
            {
                return false;
            }

            OpCode opCode = lastOp.GetArg<OpCode>(0);

            return opCode == OpCodes.Br || opCode == OpCodes.Br_S;
        }
    }
}
