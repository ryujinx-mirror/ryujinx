namespace ChocolArm64.Decoders
{
    interface IOpCodeLit64 : IOpCode64
    {
        int  Rt       { get; }
        long Imm      { get; }
        int  Size     { get; }
        bool Signed   { get; }
        bool Prefetch { get; }
    }
}