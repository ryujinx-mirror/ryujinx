namespace ARMeilleure.Decoders
{
    interface IOpCode32AluMla : IOpCode32AluReg
    {
        int Ra { get; }

        bool NHigh { get; }
        bool MHigh { get; }
        bool R { get; }
    }
}
