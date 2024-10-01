namespace ARMeilleure.Decoders
{
    interface IOpCode32AluImm : IOpCode32Alu
    {
        int Immediate { get; }

        bool IsRotated { get; }
    }
}
