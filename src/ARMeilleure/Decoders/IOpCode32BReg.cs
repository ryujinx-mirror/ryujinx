namespace ARMeilleure.Decoders
{
    interface IOpCode32BReg : IOpCode32
    {
        int Rm { get; }
    }
}
