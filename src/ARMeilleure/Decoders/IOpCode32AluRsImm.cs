namespace ARMeilleure.Decoders
{
    interface IOpCode32AluRsImm : IOpCode32Alu
    {
        int Rm { get; }
        int Immediate { get; }

        ShiftType ShiftType { get; }
    }
}
