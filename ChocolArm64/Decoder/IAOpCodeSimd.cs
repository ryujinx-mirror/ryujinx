namespace ChocolArm64.Decoder
{
    interface IAOpCodeSimd : IAOpCode
    {
        int Size { get; }
    }
}