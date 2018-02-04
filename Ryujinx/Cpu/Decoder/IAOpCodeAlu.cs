namespace ChocolArm64.Decoder
{
    interface IAOpCodeAlu : IAOpCode
    {
        int Rd { get; }
        int Rn { get; }

        ADataOp DataOp { get; }
    }
}