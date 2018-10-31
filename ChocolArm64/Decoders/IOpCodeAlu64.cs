namespace ChocolArm64.Decoders
{
    interface IOpCodeAlu64 : IOpCode64
    {
        int Rd { get; }
        int Rn { get; }

        DataOp DataOp { get; }
    }
}