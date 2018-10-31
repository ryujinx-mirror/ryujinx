namespace ChocolArm64.Decoders
{
    interface IOpCodeSimd64 : IOpCode64
    {
        int Size { get; }
    }
}