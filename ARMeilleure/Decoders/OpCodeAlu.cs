namespace ARMeilleure.Decoders
{
    class OpCodeAlu : OpCode, IOpCodeAlu
    {
        public int Rd { get; protected set; }
        public int Rn { get; private   set; }

        public DataOp DataOp { get; private set; }

        public OpCodeAlu(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd     =          (opCode >>  0) & 0x1f;
            Rn     =          (opCode >>  5) & 0x1f;
            DataOp = (DataOp)((opCode >> 24) & 0x3);

            RegisterSize = (opCode >> 31) != 0
                ? RegisterSize.Int64
                : RegisterSize.Int32;
        }
    }
}