namespace ChocolArm64.Decoders
{
    interface IOpCodeCond64 : IOpCode64
    {
        Cond Cond { get; }
    }
}