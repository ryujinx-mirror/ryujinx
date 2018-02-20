namespace ChocolArm64.Decoder
{
    interface IAOpCodeAluRx : IAOpCodeAlu
    {
        int Shift { get; }
        int Rm    { get; }

        AIntType IntType { get; }
    }
}