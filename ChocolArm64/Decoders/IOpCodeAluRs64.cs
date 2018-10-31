namespace ChocolArm64.Decoders
{
    interface IOpCodeAluRs64 : IOpCodeAlu64
    {
        int Shift { get; }
        int Rm    { get; }

        ShiftType ShiftType { get; }
    }
}