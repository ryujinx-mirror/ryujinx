using ChocolArm64.Decoders;
using ChocolArm64.Instructions;
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

        private Dictionary<long, ILLabel> _labels;

        private long _subPosition;

        private int _opcIndex;

        private Block _currBlock;

        public Block    CurrBlock => _currBlock;
        public OpCode64 CurrOp    => _currBlock?.OpCodes[_opcIndex];

        public TranslationTier Tier { get; }

        public Aarch32Mode Mode { get; } = Aarch32Mode.User; //TODO

        public bool HasIndirectJump { get; set; }

        public bool HasSlowCall { get; set; }

        private Dictionary<Block, ILBlock> _visitedBlocks;

        private Queue<Block> _branchTargets;

        private List<ILBlock> _ilBlocks;

        private ILBlock _ilBlock;

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
        private const int UserVecTempStart = ReservedLocalsCount + 1;

        private static int _userIntTempCount;
        private static int _userVecTempCount;

        public ILEmitterCtx(
            MemoryManager   memory,
            TranslatorCache cache,
            TranslatorQueue queue,
            TranslationTier tier,
            Block           graph)
        {
            Memory     = memory ?? throw new ArgumentNullException(nameof(memory));
            _cache     = cache  ?? throw new ArgumentNullException(nameof(cache));
            _queue     = queue  ?? throw new ArgumentNullException(nameof(queue));
            _currBlock = graph  ?? throw new ArgumentNullException(nameof(graph));

            Tier = tier;

            _labels = new Dictionary<long, ILLabel>();

            _visitedBlocks = new Dictionary<Block, ILBlock>();

            _visitedBlocks.Add(graph, new ILBlock());

            _branchTargets = new Queue<Block>();

            _ilBlocks = new List<ILBlock>();

            _subPosition = graph.Position;

            ResetBlockState();

            if (AdvanceOpCode())
            {
                EmitSynchronization();

                _ilBlock.Add(new ILOpCodeLoadState(_ilBlock, isSubEntry: true));
            }
        }

        public static int GetIntTempIndex()
        {
            return UserIntTempStart + _userIntTempCount++;
        }

        public static int GetVecTempIndex()
        {
            return UserVecTempStart + _userVecTempCount++;
        }

        public ILBlock[] GetILBlocks()
        {
            EmitAllOpCodes();

            return _ilBlocks.ToArray();
        }

        private void EmitAllOpCodes()
        {
            do
            {
                EmitOpCode();
            }
            while (AdvanceOpCode());
        }

        private void EmitOpCode()
        {
            if (_currBlock == null)
            {
                return;
            }

            int opcIndex = _opcIndex;

            if (opcIndex == 0)
            {
                MarkLabel(GetLabel(_currBlock.Position));
            }

            bool isLastOp = opcIndex == CurrBlock.OpCodes.Count - 1;

            if (isLastOp && CurrBlock.Branch != null &&
                     (ulong)CurrBlock.Branch.Position <= (ulong)CurrBlock.Position)
            {
                EmitSynchronization();
            }

            //On AARCH32 mode, (almost) all instruction can be conditionally
            //executed, and the required condition is encoded on the opcode.
            //We handle that here, skipping the instruction if the condition
            //is not met. We can just ignore it when the condition is "Always",
            //because in this case the instruction is always going to be executed.
            //Condition "Never" is also ignored because this is a special encoding
            //used by some unconditional instructions.
            ILLabel lblSkip = null;

            if (CurrOp is OpCode32 op && op.Cond < Condition.Al)
            {
                lblSkip = new ILLabel();

                EmitCondBranch(lblSkip, GetInverseCond(op.Cond));
            }

            CurrOp.Emitter(this);

            if (lblSkip != null)
            {
                MarkLabel(lblSkip);

                //If this is the last op on the block, and there's no "next" block
                //after this one, then we have to return right now, with the address
                //of the next instruction to be executed (in the case that the condition
                //is false, and the branch was not taken, as all basic blocks should end with
                //some kind of branch).
                if (isLastOp && CurrBlock.Next == null)
                {
                    EmitStoreState();
                    EmitLdc_I8(CurrOp.Position + CurrOp.OpCodeSizeInBytes);

                    Emit(OpCodes.Ret);
                }
            }

            _ilBlock.Add(new ILBarrier());
        }

        private static Condition GetInverseCond(Condition cond)
        {
            //Bit 0 of all conditions is basically a negation bit, so
            //inverting this bit has the effect of inverting the condition.
            return (Condition)((int)cond ^ 1);
        }

        private void EmitSynchronization()
        {
            EmitLdarg(TranslatedSub.StateArgIdx);

            EmitLdc_I4(_currBlock.OpCodes.Count);

            EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.Synchronize));

            EmitLdc_I4(0);

            ILLabel lblContinue = new ILLabel();

            Emit(OpCodes.Bne_Un_S, lblContinue);

            EmitLdc_I8(0);

            Emit(OpCodes.Ret);

            MarkLabel(lblContinue);
        }

        private bool AdvanceOpCode()
        {
            if (_currBlock == null)
            {
                return false;
            }

            while (++_opcIndex >= _currBlock.OpCodes.Count)
            {
                if (!AdvanceBlock())
                {
                    return false;
                }

                ResetBlockState();
            }

            return true;
        }

        private bool AdvanceBlock()
        {
            if (_currBlock.Branch != null)
            {
                if (_visitedBlocks.TryAdd(_currBlock.Branch, _ilBlock.Branch))
                {
                    _branchTargets.Enqueue(_currBlock.Branch);
                }
            }

            if (_currBlock.Next != null)
            {
                if (_visitedBlocks.TryAdd(_currBlock.Next, _ilBlock.Next))
                {
                    _currBlock = _currBlock.Next;

                    return true;
                }
                else
                {
                    Emit(OpCodes.Br, GetLabel(_currBlock.Next.Position));
                }
            }

            return _branchTargets.TryDequeue(out _currBlock);
        }

        private void ResetBlockState()
        {
            _ilBlock = _visitedBlocks[_currBlock];

            _ilBlocks.Add(_ilBlock);

            _ilBlock.Next   = GetOrCreateILBlock(_currBlock.Next);
            _ilBlock.Branch = GetOrCreateILBlock(_currBlock.Branch);

            _opcIndex = -1;

            _optOpLastFlagSet = null;
            _optOpLastCompare = null;
        }

        private ILBlock GetOrCreateILBlock(Block block)
        {
            if (block == null)
            {
                return null;
            }

            if (_visitedBlocks.TryGetValue(block, out ILBlock ilBlock))
            {
                return ilBlock;
            }

            return new ILBlock();
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

            if (!_cache.TryGetSubroutine(op.Imm, out TranslatedSub sub))
            {
                return false;
            }

            //It's not worth to call a Tier0 method, because
            //it contains slow code, rather than the entire function.
            if (sub.Tier == TranslationTier.Tier0)
            {
                return false;
            }

            EmitStoreState(sub);

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

            Stloc(CmpOptTmp2Index, VarType.Int);
            Stloc(CmpOptTmp1Index, VarType.Int);
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
                    Ldloc(CmpOptTmp1Index, VarType.Int, _optOpLastCompare.RegisterSize);
                    Ldloc(CmpOptTmp2Index, VarType.Int, _optOpLastCompare.RegisterSize);

                    Emit(_branchOps[cond], target);

                    return;
                }
                else if (_optOpLastCompare.Emitter == InstEmit.Adds && cond != Condition.GeUn
                                                                    && cond != Condition.LtUn
                                                                    && cond != Condition.GtUn
                                                                    && cond != Condition.LeUn)
                {
                    //There are several limitations that needs to be taken into account for CMN comparisons:
                    //* The unsigned comparisons are not valid, as they depend on the
                    //carry flag value, and they will have different values for addition and
                    //subtraction. For addition, it's carry, and for subtraction, it's borrow.
                    //So, we need to make sure we're not doing a unsigned compare for the CMN case.
                    //* We can only do the optimization for the immediate variants,
                    //because when the second operand value is exactly INT_MIN, we can't
                    //negate the value as theres no positive counterpart.
                    //Such invalid values can't be encoded on the immediate encodings.
                    if (_optOpLastCompare is IOpCodeAluImm64 op)
                    {
                        Ldloc(CmpOptTmp1Index, VarType.Int, _optOpLastCompare.RegisterSize);

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

                ilOp = (intCond & 1) != 0
                    ? OpCodes.Brfalse
                    : OpCodes.Brtrue;
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

            if (sz64 == (intType == IntType.UInt64 ||
                         intType == IntType.Int64))
            {
                return;
            }

            if (sz64)
            {
                Emit(intType >= IntType.Int8
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);
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
                Stloc(RorTmpIndex, VarType.Int);
                Ldloc(RorTmpIndex, VarType.Int);

                EmitLdc_I4(amount);

                Emit(OpCodes.Shr_Un);

                Ldloc(RorTmpIndex, VarType.Int);

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
            _ilBlock.Add(label);
        }

        public void Emit(OpCode ilOp)
        {
            _ilBlock.Add(new ILOpCode(ilOp));
        }

        public void Emit(OpCode ilOp, ILLabel label)
        {
            _ilBlock.Add(new ILOpCodeBranch(ilOp, label));
        }

        public void EmitFieldLoad(FieldInfo info)
        {
            _ilBlock.Add(new ILOpCodeLoadField(info));
        }

        public void EmitPrint(string text)
        {
            _ilBlock.Add(new ILOpCodeLog(text));
        }

        public void EmitLdarg(int index)
        {
            _ilBlock.Add(new ILOpCodeLoad(index, VarType.Arg));
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

        public void EmitLoadState()
        {
            if (_ilBlock.Next == null)
            {
                throw new InvalidOperationException("Can't load state for next block, because there's no next block.");
            }

            _ilBlock.Add(new ILOpCodeLoadState(_ilBlock.Next));
        }

        public void EmitStoreState()
        {
            _ilBlock.Add(new ILOpCodeStoreState(_ilBlock));
        }

        private void EmitStoreState(TranslatedSub callSub)
        {
            _ilBlock.Add(new ILOpCodeStoreState(_ilBlock, callSub));
        }

        public void EmitLdtmp() => EmitLdint(IntGpTmp1Index);
        public void EmitSttmp() => EmitStint(IntGpTmp1Index);

        public void EmitLdtmp2() => EmitLdint(IntGpTmp2Index);
        public void EmitSttmp2() => EmitStint(IntGpTmp2Index);

        public void EmitLdvectmp() => EmitLdvec(VecGpTmp1Index);
        public void EmitStvectmp() => EmitStvec(VecGpTmp1Index);

        public void EmitLdint(int index) => Ldloc(index, VarType.Int);
        public void EmitStint(int index) => Stloc(index, VarType.Int);

        public void EmitLdvec(int index) => Ldloc(index, VarType.Vector);
        public void EmitStvec(int index) => Stloc(index, VarType.Vector);

        public void EmitLdflg(int index) => Ldloc(index, VarType.Flag);
        public void EmitStflg(int index)
        {
            //Set this only if any of the NZCV flag bits were modified.
            //This is used to ensure that, when emiting a direct IL branch
            //instruction for compare + branch sequences, we're not expecting
            //to use comparison values from an old instruction, when in fact
            //the flags were already overwritten by another instruction further along.
            if (index >= (int)PState.VBit)
            {
                _optOpLastFlagSet = CurrOp;
            }

            Stloc(index, VarType.Flag);
        }

        private void Ldloc(int index, VarType varType)
        {
            _ilBlock.Add(new ILOpCodeLoad(index, varType, CurrOp.RegisterSize));
        }

        private void Ldloc(int index, VarType varType, RegisterSize registerSize)
        {
            _ilBlock.Add(new ILOpCodeLoad(index, varType, registerSize));
        }

        private void Stloc(int index, VarType varType)
        {
            _ilBlock.Add(new ILOpCodeStore(index, varType, CurrOp.RegisterSize));
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
            _ilBlock.Add(new ILOpCodeCall(mthdInfo ?? throw new ArgumentNullException(nameof(mthdInfo)), isVirtual));
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
            _ilBlock.Add(new ILOpCodeConst(value));
        }

        public void EmitLdc_I8(long value)
        {
            _ilBlock.Add(new ILOpCodeConst(value));
        }

        public void EmitLdc_R4(float value)
        {
            _ilBlock.Add(new ILOpCodeConst(value));
        }

        public void EmitLdc_R8(double value)
        {
            _ilBlock.Add(new ILOpCodeConst(value));
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
    }
}
