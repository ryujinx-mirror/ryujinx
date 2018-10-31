namespace ChocolArm64.Decoders
{
    interface IOpCodeAluRx64 : IOpCodeAlu64
    {
        int Shift { get; }
        int Rm    { get; }

        IntType IntType { get; }
    }
}