namespace ChocolArm64.Decoder
{
    interface IAOpCodeCond : IAOpCode
    {
        ACond Cond { get; }
    }
}