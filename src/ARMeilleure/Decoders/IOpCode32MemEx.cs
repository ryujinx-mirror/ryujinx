namespace ARMeilleure.Decoders
{
    interface IOpCode32MemEx : IOpCode32Mem
    {
        int Rd { get; }
    }
}
