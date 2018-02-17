using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCodeLoad : IAILEmit
    {
        public int Index { get; private set; }

        public AIoType IoType { get; private set; }

        public ARegisterSize RegisterSize { get; private set; }

        public AILOpCodeLoad(int Index, AIoType IoType) : this(Index, IoType, ARegisterSize.Int64) { }

        public AILOpCodeLoad(int Index, AIoType IoType, ARegisterSize RegisterSize)
        {
            this.IoType       = IoType;
            this.Index        = Index;
            this.RegisterSize = RegisterSize;
        }

        public void Emit(AILEmitter Context)
        {
            switch (IoType & AIoType.Mask)
            {
                case AIoType.Arg: Context.Generator.EmitLdarg(Index); break;

                case AIoType.Fields:
                {
                    long IntInputs = Context.LocalAlloc.GetIntInputs(Context.GetILBlock(Index));
                    long VecInputs = Context.LocalAlloc.GetVecInputs(Context.GetILBlock(Index));

                    LoadLocals(Context, IntInputs, ARegisterType.Int);
                    LoadLocals(Context, VecInputs, ARegisterType.Vector);

                    break;
                }

                case AIoType.Flag:   EmitLdloc(Context, Index, ARegisterType.Flag);   break;
                case AIoType.Int:    EmitLdloc(Context, Index, ARegisterType.Int);    break;
                case AIoType.Vector: EmitLdloc(Context, Index, ARegisterType.Vector); break;
            }
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

                    Context.Generator.EmitStloc(Context.GetLocalIndex(Reg));
                }
            }
        }

        private void EmitLdloc(AILEmitter Context, int Index, ARegisterType RegisterType)
        {
            ARegister Reg = new ARegister(Index, RegisterType);

            Context.Generator.EmitLdloc(Context.GetLocalIndex(Reg));

            if (RegisterType == ARegisterType.Int &&
                RegisterSize == ARegisterSize.Int32)
            {
                Context.Generator.Emit(OpCodes.Conv_U4);
            }
        }
    }
}