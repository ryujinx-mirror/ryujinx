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

        private AILEmitter Emitter;

        private AILBlock ILBlock;

        private AOpCode LastCmpOp;
        private AOpCode LastFlagOp;

        private int BlkIndex;
        private int OpcIndex;

        private ABlock[] Graph;
        private ABlock   Root;
        public  ABlock   CurrBlock => Graph[BlkIndex];
        public  AOpCode  CurrOp    => Graph[BlkIndex].OpCodes[OpcIndex];

        //This is the index of the temporary register, used to store temporary
        //values needed by some functions, since IL doesn't have a swap instruction.
        //You can use any value here as long it doesn't conflict with the indices
        //for the other registers. Any value >= 64 or < 0 will do.
        private const int Tmp1Index = -1;
        private const int Tmp2Index = -2;
        private const int Tmp3Index = -3;
        private const int Tmp4Index = -4;

        public AILEmitterCtx(ATranslator Translator, ABlock[] Graph, ABlock Root)
        {
            this.Translator = Translator;
            this.Graph      = Graph;
            this.Root       = Root;

            string SubName = $"Sub{Root.Position:X16}";

            Labels = new Dictionary<long, AILLabel>();

            Emitter = new AILEmitter(Graph, Root, SubName);

            ILBlock = Emitter.GetILBlock(0);          

            OpcIndex = -1;

            if (!AdvanceOpCode())
            {
                throw new ArgumentException(nameof(Graph));
            }
        }

        public ATranslatedSub GetSubroutine() => Emitter.GetSubroutine();

        public bool AdvanceOpCode()
        {
            while (++OpcIndex >= (CurrBlock?.OpCodes.Count ?? 0))
            {
                if (BlkIndex + 1 >= Graph.Length)
                {
                    return false;
                }

                BlkIndex++;
                OpcIndex = -1;

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
        }

        public bool TryOptEmitSubroutineCall()
        {           
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

            return true;
        }

        public void TryOptMarkCondWithoutCmp()
        {
            LastCmpOp = CurrOp;

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

            if (LastFlagOp == LastCmpOp && BranchOps.ContainsKey(Cond))
            {
                Ldloc(Tmp3Index, AIoType.Int, GetIntType(LastCmpOp));
                Ldloc(Tmp4Index, AIoType.Int, GetIntType(LastCmpOp));

                if (LastCmpOp.Emitter == AInstEmit.Adds)
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

            if (IntType == AIntType.UInt64 ||
                IntType == AIntType.Int64)
            {
                return;
            }

            if (CurrOp.RegisterSize != ARegisterSize.Int32)
            {
                Emit(IntType >= AIntType.Int8
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);
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
            if (Index != ARegisters.ZRIndex)
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
            if (Index != ARegisters.ZRIndex)
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

        public void EmitLdint(int Index) => Ldloc(Index, AIoType.Int);
        public void EmitStint(int Index) => Stloc(Index, AIoType.Int);

        public void EmitLdvec(int Index) => Ldloc(Index, AIoType.Vector);
        public void EmitStvec(int Index) => Stloc(Index, AIoType.Vector);

        public void EmitLdvecsi(int Index) => Ldloc(Index, AIoType.VectorI);
        public void EmitStvecsi(int Index) => Stloc(Index, AIoType.VectorI);

        public void EmitLdvecsf(int Index) => Ldloc(Index, AIoType.VectorF);
        public void EmitStvecsf(int Index) => Stloc(Index, AIoType.VectorF);

        public void EmitLdflg(int Index) => Ldloc(Index, AIoType.Flag);
        public void EmitStflg(int Index)
        {
            LastFlagOp = CurrOp;

            Stloc(Index, AIoType.Flag);
        }

        private void Ldloc(int Index, AIoType IoType)
        {
            ILBlock.Add(new AILOpCodeLoad(Index, IoType, GetOperType(IoType)));
        }

        private void Ldloc(int Index, AIoType IoType, Type Type)
        {
            ILBlock.Add(new AILOpCodeLoad(Index, IoType, Type));
        }

        private void Stloc(int Index, AIoType IoType)
        {
            ILBlock.Add(new AILOpCodeStore(Index, IoType, GetOutOperType(IoType)));
        }

        private Type GetOutOperType(AIoType IoType)
        {
            //This instruction is used to convert between floating point
            //types, so the input and output types are different.
            if (CurrOp.Emitter == AInstEmit.Fcvt_S)
            {
                return GetFloatType(((AOpCodeSimd)CurrOp).Opc);
            }
            else
            {
                return GetOperType(IoType);
            }
        }

        private Type GetOperType(AIoType IoType)
        {
            switch (IoType & AIoType.Mask)
            {
                case AIoType.Flag:   return typeof(bool);
                case AIoType.Int:    return GetIntType(CurrOp);
                case AIoType.Vector: return GetVecType(CurrOp, IoType);
            }

            throw new ArgumentException(nameof(IoType));
        }

        private Type GetIntType(AOpCode OpCode)
        {
            //Always default to 64-bits.
            return OpCode.RegisterSize == ARegisterSize.Int32
                ? typeof(uint)
                : typeof(ulong);
        }

        private Type GetVecType(AOpCode OpCode, AIoType IoType)
        {
            if (!(OpCode is IAOpCodeSimd Op))
            {
                return typeof(AVec);
            }

            int Size = Op.Size;

            if (Op.Emitter == AInstEmit.Fmov_Ftoi ||
                Op.Emitter == AInstEmit.Fmov_Itof)
            {
                Size |= 2;
            }

            if (Op is AOpCodeMem || Op is IAOpCodeLit)
            {
                return Size < 4 ? typeof(ulong) : typeof(AVec);
            }
            else if (IoType == AIoType.VectorI)
            {
                return GetIntType(Size);
            }
            else if (IoType == AIoType.VectorF)
            {
                return GetFloatType(Size);
            }

            return typeof(AVec);
        }

        private static Type GetIntType(int Size)
        {
            switch (Size)
            {
                case 0: return typeof(byte);
                case 1: return typeof(ushort);
                case 2: return typeof(uint);
                case 3: return typeof(ulong);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }

        private static Type GetFloatType(int Size)
        {
            switch (Size)
            {
                case 0: return typeof(float);
                case 1: return typeof(double);
            }

            throw new ArgumentOutOfRangeException(nameof(Size));
        }

        public void EmitCall(Type MthdType, string MthdName)
        {
            if (MthdType == null)
            {
                throw new ArgumentNullException(nameof(MthdType));
            }

            if (MthdName == null)
            {
                throw new ArgumentNullException(nameof(MthdName));
            }

            EmitCall(MthdType.GetMethod(MthdName));
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