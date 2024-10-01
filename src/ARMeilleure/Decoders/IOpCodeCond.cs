namespace ARMeilleure.Decoders
{
    interface IOpCodeCond : IOpCode
    {
        Condition Cond { get; }
    }
}
