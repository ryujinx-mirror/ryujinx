namespace ARMeilleure.Decoders
{
    interface IOpCode32AluUmull : IOpCode32, IOpCode32HasSetFlags
    {
        int RdLo { get; }
        int RdHi { get; }
        int Rn { get; }
        int Rm { get; }

        bool NHigh { get; }
        bool MHigh { get; }
    }
}
