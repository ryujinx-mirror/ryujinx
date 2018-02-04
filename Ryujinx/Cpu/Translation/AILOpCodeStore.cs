using ChocolArm64.State;
using System;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCodeStore : IAILEmit
    {
        public AIoType IoType { get; private set; }

        public Type OperType { get; private set; }

        public int Index { get; private set; }

        public AILOpCodeStore(int Index, AIoType IoType) : this(Index, IoType, null) { }

        public AILOpCodeStore(int Index, AIoType IoType, Type OperType)
        {
            this.IoType   = IoType;
            this.Index    = Index;
            this.OperType = OperType;
        }

        public void Emit(AILEmitter Context)
        {
            switch (IoType & AIoType.Mask)
            {
                case AIoType.Arg:    EmitStarg(Context, Index);                       break;
                case AIoType.Fields: EmitStfld(Context, Index);                       break;
                case AIoType.Flag:   EmitStloc(Context, Index, ARegisterType.Flag);   break;
                case AIoType.Int:    EmitStloc(Context, Index, ARegisterType.Int);    break;
                case AIoType.Vector: EmitStloc(Context, Index, ARegisterType.Vector); break;
            }
        }

        private void EmitStarg(AILEmitter Context, int Index)
        {
            Context.Generator.EmitStarg(Index);
        }

        private void EmitStfld(AILEmitter Context, int Index)
        {
            long IntOutputs = Context.LocalAlloc.GetIntOutputs(Context.GetILBlock(Index));
            long VecOutputs = Context.LocalAlloc.GetVecOutputs(Context.GetILBlock(Index));

            StoreLocals(Context, IntOutputs, ARegisterType.Int);
            StoreLocals(Context, VecOutputs, ARegisterType.Vector);
        }

        private void StoreLocals(AILEmitter Context, long Outputs, ARegisterType BaseType)
        {
            for (int Bit = 0; Bit < 64; Bit++)
            {
                long Mask = 1L << Bit;

                if ((Outputs & Mask) != 0)
                {
                    ARegister Reg = AILEmitter.GetRegFromBit(Bit, BaseType);

                    Context.Generator.EmitLdarg(ATranslatedSub.RegistersArgIdx);
                    Context.Generator.EmitLdloc(Context.GetLocalIndex(Reg));

                    AILConv.EmitConv(
                        Context,
                        Context.GetLocalType(Reg),
                        Context.GetFieldType(Reg.Type));

                    Context.Generator.Emit(OpCodes.Stfld, Reg.GetField());
                }
            }
        }

        private void EmitStloc(AILEmitter Context, int Index, ARegisterType Type)
        {
            ARegister Reg = new ARegister(Index, Type);

            AILConv.EmitConv(Context, OperType, Context.GetLocalType(Reg));

            Context.Generator.EmitStloc(Context.GetLocalIndex(Reg));
        }
    }
}