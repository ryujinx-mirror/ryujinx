using ChocolArm64.Decoder;
using ChocolArm64.Instruction;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    class AILEmitterCtx
    {
        private ATranslator Translator;

        private Dictionary<long, AILLabel> Labels;

        private int BlkIndex;
        private int OpcIndex;

        private ABlock[] Graph;
        private ABlock   Root;
        public  ABlock   CurrBlock => Graph[BlkIndex];
        public  AOpCode  CurrOp    => Graph[BlkIndex].OpCodes[OpcIndex];

        private AILEmitter Emitter;

        private AILBlock ILBlock;

        private AOpCode OptOpLastCompare;
        private AOpCode OptOpLastFlagSet;

        //This is the index of the temporary register, used to store temporary
        //values needed by some functions, since IL doesn't have a swap instruction.
        //You can use any value here as long it doesn't conflict with the indices
        //for the other registers. Any value >= 64 or < 0 will do.
        private const int Tmp1Index = -1;
        private const int Tmp2Index = -2;
        private const int Tmp3Index = -3;
        private const int Tmp4Index = -4;
        private const int Tmp5Index = -5;

        public AILEmitterCtx(
            ATranslator Translator,
            ABlock[]    Graph,
            ABlock      Root,
            string      SubName)
        {
            if (Translator == null)
            {
                throw new ArgumentNullException(nameof(Translator));
            }

            if (Graph == null)
            {
                throw new ArgumentNullException(nameof(Graph));
            }

            if (Root == null)
            {
                throw new ArgumentNullException(nameof(Root));
            }

            this.Translator = Translator;
            this.Graph      = Graph;
            this.Root       = Root;

            Labels = new Dictionary<long, AILLabel>();

            Emitter = new AILEmitter(Graph, Root, SubName);

            ILBlock = Emitter.GetILBlock(0);

            OpcIndex = -1;

            if (Graph.Length == 0 || !AdvanceOpCode())
            {
                throw new ArgumentException(nameof(Graph));
            }
        }

        public ATranslatedSub GetSubroutine()
        {
            return Emitter.GetSubroutine();
        }

        public bool AdvanceOpCode()
        {
            if (OpcIndex + 1 == CurrBlock.OpCodes.Count &&
                BlkIndex + 1 == Graph.Length)
            {
                return false;
            }

            while (++OpcIndex >= (CurrBlock?.OpCodes.Count ?? 0))
            {
                BlkIndex++;
                OpcIndex = -1;

                OptOpLastFlagSet = null;
                OptOpLastCompare = null;

                ILBlock = Emitter.GetILBlock(BlkIndex);
            }

            return true;
        }

        public void EmitOpCode()
        {
            if (OpcIndex == 0)
            {
                MarkLabel(GetLabel(CurrBlock.Position));
            }

            CurrOp.Emitter(this);

            ILBlock.Add(new AILBarrier());
        }

        public bool TryOptEmitSubroutineCall()
        {
            if (CurrBlock.Next == null)
            {
                return false;
            }

            if (!Translator.TryGetCachedSub(CurrOp, out ATranslatedSub Sub))
            {
                return false;
            }

            for (int Index = 0; Index < ATranslatedSub.FixedArgTypes.Length; Index++)
            {
                EmitLdarg(Index);
            }

            foreach (ARegister Reg in Sub.Params)
            {
                switch (Reg.Type)
                {
                    case ARegisterType.Flag:   Ldloc(Reg.Index, AIoType.Flag);   break;
                    case ARegisterType.Int:    Ldloc(Reg.Index, AIoType.Int);    break;
                    case ARegisterType.Vector: Ldloc(Reg.Index, AIoType.Vector); break;
                }
            }

            EmitCall(Sub.Method);

            Sub.AddCaller(Root.Position);

            return true;
        }

        public void TryOptMarkCondWithoutCmp()
        {
            OptOpLastCompare = CurrOp;

            AInstEmitAluHelper.EmitDataLoadOpers(this);

            Stloc(Tmp4Index, AIoType.Int);
            Stloc(Tmp3Index, AIoType.Int);
        }

        private Dictionary<ACond, OpCode> BranchOps = new Dictionary<ACond, OpCode>()
        {
            { ACond.Eq,    OpCodes.Beq    },
            { ACond.Ne,    OpCodes.Bne_Un },
            { ACond.Ge_Un, OpCodes.Bge_Un },
            { ACond.Lt_Un, OpCodes.Blt_Un },
            { ACond.Gt_Un, OpCodes.Bgt_Un },
            { ACond.Le_Un, OpCodes.Ble_Un },
            { ACond.Ge,    OpCodes.Bge    },
            { ACond.Lt,    OpCodes.Blt    },
            { ACond.Gt,    OpCodes.Bgt    },
            { ACond.Le,    OpCodes.Ble    }
        };

        public void EmitCondBranch(AILLabel Target, ACond Cond)
        {
            OpCode ILOp;

            int IntCond = (int)Cond;

            if (OptOpLastCompare != null &&
                OptOpLastCompare == OptOpLastFlagSet && BranchOps.ContainsKey(Cond))
            {
                Ldloc(Tmp3Index, AIoType.Int, OptOpLastCompare.RegisterSize);
                Ldloc(Tmp4Index, AIoType.Int, OptOpLastCompare.RegisterSize);

                if (OptOpLastCompare.Emitter == AInstEmit.Adds)
                {
                    Emit(OpCodes.Neg);
                }

                ILOp = BranchOps[Cond];
            }
            else if (IntCond < 14)
            {
                int CondTrue = IntCond >> 1;

                switch (CondTrue)
                {
                    case 0: EmitLdflg((int)APState.ZBit); break;
                    case 1: EmitLdflg((int)APState.CBit); break;
                    case 2: EmitLdflg((int)APState.NBit); break;
                    case 3: EmitLdflg((int)APState.VBit); break;

                    case 4:
                        EmitLdflg((int)APState.CBit);
                        EmitLdflg((int)APState.ZBit);

                        Emit(OpCodes.Not);
                        Emit(OpCodes.And);
                        break;

                    case 5:
                    case 6:
                        EmitLdflg((int)APState.NBit);
                        EmitLdflg((int)APState.VBit);

                        Emit(OpCodes.Ceq);

                        if (CondTrue == 6)
                        {
                            EmitLdflg((int)APState.ZBit);

                            Emit(OpCodes.Not);
                            Emit(OpCodes.And);
                        }
                        break;
                }

                ILOp = (IntCond & 1) != 0
                    ? OpCodes.Brfalse
                    : OpCodes.Brtrue;
            }
            else
            {
                ILOp = OpCodes.Br;
            }

            Emit(ILOp, Target);
        }

        public void EmitCast(AIntType IntType)
        {
            switch (IntType)
            {
                case AIntType.UInt8:  Emit(OpCodes.Conv_U1); break;
                case AIntType.UInt16: Emit(OpCodes.Conv_U2); break;
                case AIntType.UInt32: Emit(OpCodes.Conv_U4); break;
                case AIntType.UInt64: Emit(OpCodes.Conv_U8); break;
                case AIntType.Int8:   Emit(OpCodes.Conv_I1); break;
                case AIntType.Int16:  Emit(OpCodes.Conv_I2); break;
                case AIntType.Int32:  Emit(OpCodes.Conv_I4); break;
                case AIntType.Int64:  Emit(OpCodes.Conv_I8); break;
            }

            bool Sz64 = CurrOp.RegisterSize != ARegisterSize.Int32;

            if (Sz64 == (IntType == AIntType.UInt64 ||
                         IntType == AIntType.Int64))
            {
                return;
            }

            if (Sz64)
            {
                Emit(IntType >= AIntType.Int8
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);
            }
            else
            {
                Emit(OpCodes.Conv_U4);
            }
        }

        public void EmitLsl(int Amount) => EmitILShift(Amount, OpCodes.Shl);
        public void EmitLsr(int Amount) => EmitILShift(Amount, OpCodes.Shr_Un);
        public void EmitAsr(int Amount) => EmitILShift(Amount, OpCodes.Shr);

        private void EmitILShift(int Amount, OpCode ILOp)
        {
            if (Amount > 0)
            {
                EmitLdc_I4(Amount);

                Emit(ILOp);
            }
        }

        public void EmitRor(int Amount)
        {
            if (Amount > 0)
            {
                Stloc(Tmp2Index, AIoType.Int);
                Ldloc(Tmp2Index, AIoType.Int);

                EmitLdc_I4(Amount);

                Emit(OpCodes.Shr_Un);

                Ldloc(Tmp2Index, AIoType.Int);

                EmitLdc_I4(CurrOp.GetBitsCount() - Amount);

                Emit(OpCodes.Shl);
                Emit(OpCodes.Or);
            }
        }

        public AILLabel GetLabel(long Position)
        {
            if (!Labels.TryGetValue(Position, out AILLabel Output))
            {
                Output = new AILLabel();

                Labels.Add(Position, Output);
            }

            return Output;
        }

        public void MarkLabel(AILLabel Label)
        {
            ILBlock.Add(Label);
        }

        public void Emit(OpCode ILOp)
        {
            ILBlock.Add(new AILOpCode(ILOp));
        }

        public void Emit(OpCode ILOp, AILLabel Label)
        {
            ILBlock.Add(new AILOpCodeBranch(ILOp, Label));
        }

        public void Emit(string Text)
        {
            ILBlock.Add(new AILOpCodeLog(Text));
        }

        public void EmitLdarg(int Index)
        {
            ILBlock.Add(new AILOpCodeLoad(Index, AIoType.Arg));
        }

        public void EmitLdintzr(int Index)
        {
            if (Index != AThreadState.ZRIndex)
            {
                EmitLdint(Index);
            }
            else
            {
                EmitLdc_I(0);
            }
        }

        public void EmitStintzr(int Index)
        {
            if (Index != AThreadState.ZRIndex)
            {
                EmitStint(Index);
            }
            else
            {
                Emit(OpCodes.Pop);
            }
        }

        public void EmitLoadState(ABlock RetBlk)
        {
            ILBlock.Add(new AILOpCodeLoad(Array.IndexOf(Graph, RetBlk), AIoType.Fields));
        }

        public void EmitStoreState()
        {
            ILBlock.Add(new AILOpCodeStore(Array.IndexOf(Graph, CurrBlock), AIoType.Fields));
        }

        public void EmitLdtmp() => EmitLdint(Tmp1Index);
        public void EmitSttmp() => EmitStint(Tmp1Index);

        public void EmitLdvectmp() => EmitLdvec(Tmp5Index);
        public void EmitStvectmp() => EmitStvec(Tmp5Index);

        public void EmitLdint(int Index) => Ldloc(Index, AIoType.Int);
        public void EmitStint(int Index) => Stloc(Index, AIoType.Int);

        public void EmitLdvec(int Index) => Ldloc(Index, AIoType.Vector);
        public void EmitStvec(int Index) => Stloc(Index, AIoType.Vector);

        public void EmitLdflg(int Index) => Ldloc(Index, AIoType.Flag);
        public void EmitStflg(int Index)
        {
            OptOpLastFlagSet = CurrOp;

            Stloc(Index, AIoType.Flag);
        }

        private void Ldloc(int Index, AIoType IoType)
        {
            ILBlock.Add(new AILOpCodeLoad(Index, IoType, CurrOp.RegisterSize));
        }

        private void Ldloc(int Index, AIoType IoType, ARegisterSize RegisterSize)
        {
            ILBlock.Add(new AILOpCodeLoad(Index, IoType, RegisterSize));
        }

        private void Stloc(int Index, AIoType IoType)
        {
            ILBlock.Add(new AILOpCodeStore(Index, IoType, CurrOp.RegisterSize));
        }

        public void EmitCallPropGet(Type ObjType, string PropName)
        {
            if (ObjType == null)
            {
                throw new ArgumentNullException(nameof(ObjType));
            }

            if (PropName == null)
            {
                throw new ArgumentNullException(nameof(PropName));
            }

            EmitCall(ObjType.GetMethod($"get_{PropName}"));
        }

        public void EmitCallPropSet(Type ObjType, string PropName)
        {
            if (ObjType == null)
            {
                throw new ArgumentNullException(nameof(ObjType));
            }

            if (PropName == null)
            {
                throw new ArgumentNullException(nameof(PropName));
            }

            EmitCall(ObjType.GetMethod($"set_{PropName}"));
        }

        public void EmitCall(Type ObjType, string MthdName)
        {
            if (ObjType == null)
            {
                throw new ArgumentNullException(nameof(ObjType));
            }

            if (MthdName == null)
            {
                throw new ArgumentNullException(nameof(MthdName));
            }

            EmitCall(ObjType.GetMethod(MthdName));
        }

        public void EmitPrivateCall(Type ObjType, string MthdName)
        {
            if (ObjType == null)
            {
                throw new ArgumentNullException(nameof(ObjType));
            }

            if (MthdName == null)
            {
                throw new ArgumentNullException(nameof(MthdName));
            }

            EmitCall(ObjType.GetMethod(MthdName, BindingFlags.Instance | BindingFlags.NonPublic));
        }

        public void EmitCall(MethodInfo MthdInfo)
        {
            if (MthdInfo == null)
            {
                throw new ArgumentNullException(nameof(MthdInfo));
            }

            ILBlock.Add(new AILOpCodeCall(MthdInfo));
        }

        public void EmitLdc_I(long Value)
        {
            if (CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                EmitLdc_I4((int)Value);
            }
            else
            {
                EmitLdc_I8(Value);
            }
        }

        public void EmitLdc_I4(int Value)
        {
            ILBlock.Add(new AILOpCodeConst(Value));
        }

        public void EmitLdc_I8(long Value)
        {
            ILBlock.Add(new AILOpCodeConst(Value));
        }

        public void EmitLdc_R4(float Value)
        {
            ILBlock.Add(new AILOpCodeConst(Value));
        }

        public void EmitLdc_R8(double Value)
        {
            ILBlock.Add(new AILOpCodeConst(Value));
        }

        public void EmitZNFlagCheck()
        {
            EmitZNCheck(OpCodes.Ceq, (int)APState.ZBit);
            EmitZNCheck(OpCodes.Clt, (int)APState.NBit);
        }

        private void EmitZNCheck(OpCode ILCmpOp, int Flag)
        {
            Emit(OpCodes.Dup);
            Emit(OpCodes.Ldc_I4_0);

            if (CurrOp.RegisterSize != ARegisterSize.Int32)
            {
                Emit(OpCodes.Conv_I8);
            }

            Emit(ILCmpOp);

            EmitStflg(Flag);
        }
    }
}