namespace ARMeilleure.Decoders
{
    interface IOpCode32AluUx : IOpCode32AluReg
    {
        int RotateBits { get; }
        bool Add { get; }
    }
}
