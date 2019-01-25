namespace ChocolArm64.Decoders
{
    interface IOpCode32 : IOpCode64
    {
        Condition Cond { get; }

        uint GetPc();
    }
}