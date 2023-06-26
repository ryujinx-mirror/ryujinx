namespace ARMeilleure.Decoders
{
    interface IOpCode32MemRsImm : IOpCode32Mem
    {
        int Rm { get; }
        ShiftType ShiftType { get; }
    }
}
