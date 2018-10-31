using ChocolArm64.Decoders;
using ChocolArm64.Instructions;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class ILEmitterCtx
    {
        private TranslatorCache _cache;

        private Dictionary<long, ILLabel> _labels;

        private int _blkIndex;
        private int _opcIndex;

        private Block[]   _graph;
        private Block     _root;
        public  Block     CurrBlock => _graph[_blkIndex];
        public  OpCode64  CurrOp    => _graph[_blkIndex].OpCodes[_opcIndex];

        private ILEmitter _emitter;

        private ILBlock _ilBlock;

        private OpCode64 _optOpLastCompare;
        private OpCode64 _optOpLastFlagSet;

        //This is the index of the temporary register, used to store temporary
        //values needed by some functions, since IL doesn't have a swap instruction.
        //You can use any value here as long it doesn't conflict with the indices
        //for the other registers. Any value >= 64 or < 0 will do.
        private const int Tmp1Index = -1;
        private const int Tmp2Index = -2;
        private const int Tmp3Index = -3;
        private const int Tmp4Index = -4;
        private const int Tmp5Index = -5;
        private const int Tmp6Index = -6;

        public ILEmitterCtx(
            TranslatorCache cache,
            Block[]         graph,
            Block           root,
            string          subName)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _root  = root  ?? throw new ArgumentNullException(nameof(root));

            _labels = new Dictionary<long, ILLabel>();

            _emitter = new ILEmitter(graph, root, subName);

            _ilBlock = _emitter.GetIlBlock(0);

            _opcIndex = -1;

            if (graph.Length == 0 || !AdvanceOpCode())
            {
                throw new ArgumentException(nameof(graph));
            }
        }

        public TranslatedSub GetSubroutine()
        {
            return _emitter.GetSubroutine();
        }

        public bool AdvanceOpCode()
        {
            if (_opcIndex + 1 == CurrBlock.OpCodes.Count &&
                _blkIndex + 1 == _graph.Length)
            {
                return false;
            }

            while (++_opcIndex >= (CurrBlock?.OpCodes.Count ?? 0))
            {
                _blkIndex++;
                _opcIndex = -1;

                _optOpLastFlagSet = null;
                _optOpLastCompare = null;

                _ilBlock = _emitter.GetIlBlock(_blkIndex);
            }

            return true;
        }

        public void EmitOpCode()
        {
            if (_opcIndex == 0)
            {
                MarkLabel(GetLabel(CurrBlock.Position));

                EmitSynchronization();
            }

            CurrOp.Emitter(this);

            _ilBlock.Add(new ILBarrier());
        }

        private void EmitSynchronization()
        {
            EmitLdarg(TranslatedSub.StateArgIdx);

            EmitLdc_I4(CurrBlock.OpCodes.Count);

            EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.Synchronize));

            EmitLdc_I4(0);

            ILLabel lblContinue = new ILLabel();

            Emit(OpCodes.Bne_Un_S, lblContinue);

            EmitLdc_I8(0);

            Emit(OpCodes.Ret);

            MarkLabel(lblContinue);
        }

        public bool TryOptEmitSubroutineCall()
        {
            if (CurrBlock.Next == null)
            {
                return false;
            }

            if (CurrOp.Emitter != InstEmit.Bl)
            {
                return false;
            }

            if (!_cache.TryGetSubroutine(((OpCodeBImmAl64)CurrOp).Imm, out TranslatedSub subroutine))
            {
                return false;
            }

            for (int index = 0; index < TranslatedSub.FixedArgTypes.Length; index++)
            {
                EmitLdarg(index);
            }

            foreach (Register reg in subroutine.Params)
            {
                switch (reg.Type)
                {
                    case RegisterType.Flag:   Ldloc(reg.Index, IoType.Flag);   break;
                    case RegisterType.Int:    Ldloc(reg.Index, IoType.Int);    break;
                    case RegisterType.Vector: Ldloc(reg.Index, IoType.Vector); break;
                }
            }

            EmitCall(subroutine.Method);

            subroutine.AddCaller(_root.Position);

            return true;
        }

        public void TryOptMarkCondWithoutCmp()
        {
            _optOpLastCompare = CurrOp;

            InstEmitAluHelper.EmitDataLoadOpers(this);

            Stloc(Tmp4Index, IoType.Int);
            Stloc(Tmp3Index, IoType.Int);
        }

        private Dictionary<Cond, System.Reflection.Emit.OpCode> _branchOps = new Dictionary<Cond, System.Reflection.Emit.OpCode>()
        {
            { Cond.Eq,   OpCodes.Beq    },
            { Cond.Ne,   OpCodes.Bne_Un },
            { Cond.GeUn, OpCodes.Bge_Un },
            { Cond.LtUn, OpCodes.Blt_Un },
            { Cond.GtUn, OpCodes.Bgt_Un },
            { Cond.LeUn, OpCodes.Ble_Un },
            { Cond.Ge,   OpCodes.Bge    },
            { Cond.Lt,   OpCodes.Blt    },
            { Cond.Gt,   OpCodes.Bgt    },
            { Cond.Le,   OpCodes.Ble    }
        };

        public void EmitCondBranch(ILLabel target, Cond cond)
        {
            System.Reflection.Emit.OpCode ilOp;

            int intCond = (int)cond;

            if (_optOpLastCompare != null &&
                _optOpLastCompare == _optOpLastFlagSet && _branchOps.ContainsKey(cond))
            {
                Ldloc(Tmp3Index, IoType.Int, _optOpLastCompare.RegisterSize);
                Ldloc(Tmp4Index, IoType.Int, _optOpLastCompare.RegisterSize);

                ilOp = _branchOps[cond];
            }
            else if (intCond < 14)
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

        public void EmitLsl(int amount) => EmitIlShift(amount, OpCodes.Shl);
        public void EmitLsr(int amount) => EmitIlShift(amount, OpCodes.Shr_Un);
        public void EmitAsr(int amount) => EmitIlShift(amount, OpCodes.Shr);

        private void EmitIlShift(int amount, System.Reflection.Emit.OpCode ilOp)
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
                Stloc(Tmp2Index, IoType.Int);
                Ldloc(Tmp2Index, IoType.Int);

                EmitLdc_I4(amount);

                Emit(OpCodes.Shr_Un);

                Ldloc(Tmp2Index, IoType.Int);

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

        public void Emit(System.Reflection.Emit.OpCode ilOp)
        {
            _ilBlock.Add(new ILOpCode(ilOp));
        }

        public void Emit(System.Reflection.Emit.OpCode ilOp, ILLabel label)
        {
            _ilBlock.Add(new ILOpCodeBranch(ilOp, label));
        }

        public void Emit(string text)
        {
            _ilBlock.Add(new ILOpCodeLog(text));
        }

        public void EmitLdarg(int index)
        {
            _ilBlock.Add(new IlOpCodeLoad(index, IoType.Arg));
        }

        public void EmitLdintzr(int index)
        {
            if (index != CpuThreadState.ZrIndex)
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
            if (index != CpuThreadState.ZrIndex)
            {
                EmitStint(index);
            }
            else
            {
                Emit(OpCodes.Pop);
            }
        }

        public void EmitLoadState(Block retBlk)
        {
            _ilBlock.Add(new IlOpCodeLoad(Array.IndexOf(_graph, retBlk), IoType.Fields));
        }

        public void EmitStoreState()
        {
            _ilBlock.Add(new IlOpCodeStore(Array.IndexOf(_graph, CurrBlock), IoType.Fields));
        }

        public void EmitLdtmp() => EmitLdint(Tmp1Index);
        public void EmitSttmp() => EmitStint(Tmp1Index);

        public void EmitLdvectmp() => EmitLdvec(Tmp5Index);
        public void EmitStvectmp() => EmitStvec(Tmp5Index);

        public void EmitLdvectmp2() => EmitLdvec(Tmp6Index);
        public void EmitStvectmp2() => EmitStvec(Tmp6Index);

        public void EmitLdint(int index) => Ldloc(index, IoType.Int);
        public void EmitStint(int index) => Stloc(index, IoType.Int);

        public void EmitLdvec(int index) => Ldloc(index, IoType.Vector);
        public void EmitStvec(int index) => Stloc(index, IoType.Vector);

        public void EmitLdflg(int index) => Ldloc(index, IoType.Flag);
        public void EmitStflg(int index)
        {
            _optOpLastFlagSet = CurrOp;

            Stloc(index, IoType.Flag);
        }

        private void Ldloc(int index, IoType ioType)
        {
            _ilBlock.Add(new IlOpCodeLoad(index, ioType, CurrOp.RegisterSize));
        }

        private void Ldloc(int index, IoType ioType, RegisterSize registerSize)
        {
            _ilBlock.Add(new IlOpCodeLoad(index, ioType, registerSize));
        }

        private void Stloc(int index, IoType ioType)
        {
            _ilBlock.Add(new IlOpCodeStore(index, ioType, CurrOp.RegisterSize));
        }

        public void EmitCallPropGet(Type objType, string propName)
        {
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType));
            }

            if (propName == null)
            {
                throw new ArgumentNullException(nameof(propName));
            }

            EmitCall(objType.GetMethod($"get_{propName}"));
        }

        public void EmitCallPropSet(Type objType, string propName)
        {
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType));
            }

            if (propName == null)
            {
                throw new ArgumentNullException(nameof(propName));
            }

            EmitCall(objType.GetMethod($"set_{propName}"));
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

        public void EmitCall(MethodInfo mthdInfo)
        {
            if (mthdInfo == null)
            {
                throw new ArgumentNullException(nameof(mthdInfo));
            }

            _ilBlock.Add(new ILOpCodeCall(mthdInfo));
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

        private void EmitZnCheck(System.Reflection.Emit.OpCode ilCmpOp, int flag)
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
