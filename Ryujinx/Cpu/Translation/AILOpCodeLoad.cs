using ChocolArm64.State;
using System;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCodeLoad : IAILEmit
    {
        public int Index { get; private set; }

        public AIoType IoType { get; private set; }

        public Type OperType { get; private set; }

        public AILOpCodeLoad(int Index, AIoType IoType) : this(Index, IoType, null) { }

        public AILOpCodeLoad(int Index, AIoType IoType, Type OperType)
        {
            this.IoType   = IoType;
            this.Index    = Index;
            this.OperType = OperType;
        }

        public void Emit(AILEmitter Context)
        {
            switch (IoType & AIoType.Mask)
            {
                case AIoType.Arg:    EmitLdarg(Context, Index);                       break;
                case AIoType.Fields: EmitLdfld(Context, Index);                       break;
                case AIoType.Flag:   EmitLdloc(Context, Index, ARegisterType.Flag);   break;
                case AIoType.Int:    EmitLdloc(Context, Index, ARegisterType.Int);    break;
                case AIoType.Vector: EmitLdloc(Context, Index, ARegisterType.Vector); break;
            }
        }

        private void EmitLdarg(AILEmitter Context, int Index)
        {
            Context.Generator.EmitLdarg(Index);
        }

        private void EmitLdfld(AILEmitter Context, int Index)
        {
            long IntInputs = Context.LocalAlloc.GetIntInputs(Context.GetILBlock(Index));
            long VecInputs = Context.LocalAlloc.GetVecInputs(Context.GetILBlock(Index));

            LoadLocals(Context, IntInputs, ARegisterType.Int);
            LoadLocals(Context, VecInputs, ARegisterType.Vector);
        }

        private void LoadLocals(AILEmitter Context, long Inputs, ARegisterType BaseType)
        {
            for (int Bit = 0; Bit < 64; Bit++)
            {
                long Mask = 1L << Bit;

                if ((Inputs & Mask) != 0)
                {
                    ARegister Reg = AILEmitter.GetRegFromBit(Bit, BaseType);

                    Context.Generator.EmitLdarg(ATranslatedSub.RegistersArgIdx);
                    Context.Generator.Emit(OpCodes.Ldfld, Reg.GetField());

                    AILConv.EmitConv(
                        Context,
                        Context.GetFieldType(Reg.Type),
                        Context.GetLocalType(Reg));

                    Context.Generator.EmitStloc(Context.GetLocalIndex(Reg));
                }
            }
        }

        private void EmitLdloc(AILEmitter Context, int Index, ARegisterType Type)
        {
            ARegister Reg = new ARegister(Index, Type);

            Context.Generator.EmitLdloc(Context.GetLocalIndex(Reg));

            AILConv.EmitConv(Context, Context.GetLocalType(Reg), OperType);
        }
    }
}