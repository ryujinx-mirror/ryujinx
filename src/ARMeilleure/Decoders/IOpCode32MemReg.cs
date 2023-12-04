namespace ARMeilleure.Decoders
{
    interface IOpCode32MemReg : IOpCode32Mem
    {
        int Rm { get; }
    }
}
