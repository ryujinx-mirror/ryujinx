namespace ARMeilleure.Decoders
{
    interface IOpCode32AluImm16 : IOpCode32Alu
    {
        int Immediate { get; }
    }
}
