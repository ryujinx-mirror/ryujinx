namespace ChocolArm64.Decoder
{
    interface IAOpCodeAluRs : IAOpCodeAlu
    {
        int Shift { get; }
        int Rm    { get; }

        AShiftType ShiftType { get; }
    }
}