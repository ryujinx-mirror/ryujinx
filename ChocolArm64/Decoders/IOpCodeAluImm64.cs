namespace ChocolArm64.Decoders
{
    interface IOpCodeAluImm64 : IOpCodeAlu64
    {
        long Imm { get; }
    }
}