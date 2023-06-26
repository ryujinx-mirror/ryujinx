namespace ARMeilleure.Decoders
{
    interface IOpCodeBImm : IOpCode
    {
        long Immediate { get; }
    }
}
