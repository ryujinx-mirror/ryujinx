namespace ARMeilleure.Decoders
{
    class OpCodeBfm : OpCodeAlu
    {
        public long WMask { get; private set; }
        public long TMask { get; private set; }
        public int  Pos   { get; private set; }
        public int  Shift { get; private set; }

        public OpCodeBfm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            var bm = DecoderHelper.DecodeBitMask(opCode, false);

            if (bm.IsUndefined)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            WMask = bm.WMask;
            TMask = bm.TMask;
            Pos   = bm.Pos;
            Shift = bm.Shift;
        }
    }
}