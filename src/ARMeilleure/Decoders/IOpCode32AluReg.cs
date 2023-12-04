namespace ARMeilleure.Decoders
{
    interface IOpCode32AluReg : IOpCode32Alu
    {
        int Rm { get; }
    }
}
