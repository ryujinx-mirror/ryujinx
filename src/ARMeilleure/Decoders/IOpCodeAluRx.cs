namespace ARMeilleure.Decoders
{
    interface IOpCodeAluRx : IOpCodeAlu
    {
        int Shift { get; }
        int Rm { get; }

        IntType IntType { get; }
    }
}
