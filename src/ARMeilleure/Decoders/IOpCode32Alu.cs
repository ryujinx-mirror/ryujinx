namespace ARMeilleure.Decoders
{
    interface IOpCode32Alu : IOpCode32, IOpCode32HasSetFlags
    {
        int Rd { get; }
        int Rn { get; }
    }
}
