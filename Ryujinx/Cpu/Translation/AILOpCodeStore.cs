using ChocolArm64.State;
using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCodeStore : IAILEmit
    {
        public int Index { get; private set; }

        public AIoType IoType { get; private set; }

        public ARegisterSize RegisterSize { get; private set; }
        
        public AILOpCodeStore(int Index, AIoType IoType) : this(Index, IoType, ARegisterSize.Int64) { }

        public AILOpCodeStore(int Index, AIoType IoType, ARegisterSize RegisterSize)
        {
            this.IoType       = IoType;
            this.Index        = Index;
            this.RegisterSize = RegisterSize;
        }

        public void Emit(AILEmitter Context)
        {
            switch (IoType & AIoType.Mask)
            {
                case AIoType.Arg: Context.Generator.EmitStarg(Index); break;

                case AIoType.Fields:
                {
                    long IntOutputs = Context.LocalAlloc.GetIntOutputs(Context.GetILBlock(Index));
                    long VecOutputs = Context.LocalAlloc.GetVecOutputs(Context.GetILBlock(Index));

                    StoreLocals(Context, IntOutputs, ARegisterType.Int);
                    StoreLocals(Context, VecOutputs, ARegisterType.Vector);
                    
                    break;
                }

                case AIoType.Flag:   EmitStloc(Context, Index, ARegisterType.Flag);   break;
                case AIoType.Int:    EmitStloc(Context, Index, ARegisterType.Int);    break;
                case AIoType.Vector: EmitStloc(Context, Index, ARegisterType.Vector); break;
            }
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

                    Context.Generator.Emit(OpCodes.Stfld, Reg.GetField());
                }
            }
        }

        private void EmitStloc(AILEmitter Context, int Index, ARegisterType RegisterType)
        {
            ARegister Reg = new ARegister(Index, RegisterType);

            if (RegisterType == ARegisterType.Int &&
                RegisterSize == ARegisterSize.Int32)
            {
                Context.Generator.Emit(OpCodes.Conv_U8);
            }

            Context.Generator.EmitStloc(Context.GetLocalIndex(Reg));
        }
    }
}