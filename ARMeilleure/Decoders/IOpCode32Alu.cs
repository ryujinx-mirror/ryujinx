namespace ARMeilleure.Decoders
{
    interface IOpCode32Alu : IOpCode32
    {
        int Rd { get; }
        int Rn { get; }

        bool SetFlags { get; }
    }
}