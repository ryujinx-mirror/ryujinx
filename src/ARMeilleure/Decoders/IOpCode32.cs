namespace ARMeilleure.Decoders
{
    interface IOpCode32 : IOpCode
    {
        Condition Cond { get; }

        uint GetPc();
    }
}
