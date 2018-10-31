using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBfm64 : OpCodeAlu64
    {
        public long WMask { get; private set; }
        public long TMask { get; private set; }
        public int  Pos   { get; private set; }
        public int  Shift { get; private set; }

        public OpCodeBfm64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            var bm = DecoderHelper.DecodeBitMask(opCode, false);

            if (bm.IsUndefined)
            {
                Emitter = InstEmit.Und;

                return;
            }

            WMask = bm.WMask;
            TMask = bm.TMask;
            Pos   = bm.Pos;
            Shift = bm.Shift;
        }
    }
}