namespace ChocolArm64.Decoder
{
    interface IAOpCodeAluImm : IAOpCodeAlu
    {
        long Imm { get; }
    }
}