using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBfm : AOpCodeAlu
    {
        public long WMask { get; private set; }
        public long TMask { get; private set; }
        public int  Pos   { get; private set; }
        public int  Shift { get; private set; }

        public AOpCodeBfm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            var BM = ADecoderHelper.DecodeBitMask(OpCode, false);

            if (BM.IsUndefined)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            WMask = BM.WMask;
            TMask = BM.TMask;
            Pos   = BM.Pos;
            Shift = BM.Shift;
        }
    }
}