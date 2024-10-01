namespace ARMeilleure.Decoders
{
    interface IOpCodeAluImm : IOpCodeAlu
    {
        long Immediate { get; }
    }
}
