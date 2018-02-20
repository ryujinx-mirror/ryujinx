namespace ChocolArm64.Decoder
{
    interface IAOpCodeLit : IAOpCode
    {
        int  Rt       { get; }
        long Imm      { get; }
        int  Size     { get; }
        bool Signed   { get; }
        bool Prefetch { get; }
    }
}