namespace ARMeilleure.Decoders
{
    interface IOpCodeLit : IOpCode
    {
        int Rt { get; }
        long Immediate { get; }
        int Size { get; }
        bool Signed { get; }
        bool Prefetch { get; }
    }
}
