namespace ARMeilleure.Decoders
{
    interface IOpCode32AluBf
    {
        int Rd { get; }
        int Rn { get; }

        int Msb { get; }
        int Lsb { get; }

        int SourceMask => (int)(0xFFFFFFFF >> (31 - Msb));
        int DestMask => SourceMask & (int)(0xFFFFFFFF << Lsb);
    }
}
