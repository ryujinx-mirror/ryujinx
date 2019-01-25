namespace ChocolArm64.Decoders
{
    interface IOpCodeBImm : IOpCode64
    {
        long Imm { get; }
    }
}