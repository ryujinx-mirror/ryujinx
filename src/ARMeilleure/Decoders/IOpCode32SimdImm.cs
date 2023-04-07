namespace ARMeilleure.Decoders
{
    interface IOpCode32SimdImm : IOpCode32Simd
    {
        int Vd { get; }
        long Immediate { get; }
        int Elems { get; }
    }
}
