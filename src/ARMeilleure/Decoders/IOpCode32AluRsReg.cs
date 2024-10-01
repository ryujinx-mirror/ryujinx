namespace ARMeilleure.Decoders
{
    interface IOpCode32AluRsReg : IOpCode32Alu
    {
        int Rm { get; }
        int Rs { get; }

        ShiftType ShiftType { get; }
    }
}
