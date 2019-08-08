namespace ARMeilleure.Decoders
{
    class OpCodeMemEx : OpCodeMem
    {
        public int Rt2 { get; private set; }
        public int Rs  { get; private set; }

        public OpCodeMemEx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt2 = (opCode >> 10) & 0x1f;
            Rs  = (opCode >> 16) & 0x1f;
        }
    }
}