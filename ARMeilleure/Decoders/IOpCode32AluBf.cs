namespace ARMeilleure.Decoders
{
    interface IOpCode32AluBf
    {
        int Rd { get; }
        int Rn { get; }

        int Msb { get; }
        int Lsb { get; }
    }
}
