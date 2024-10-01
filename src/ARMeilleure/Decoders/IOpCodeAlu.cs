namespace ARMeilleure.Decoders
{
    interface IOpCodeAlu : IOpCode
    {
        int Rd { get; }
        int Rn { get; }

        DataOp DataOp { get; }
    }
}
