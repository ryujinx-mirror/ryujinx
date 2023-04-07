namespace ARMeilleure.Decoders
{
    interface IOpCode32Adr
    {
        int Rd { get; }

        int Immediate { get; }
    }
}
