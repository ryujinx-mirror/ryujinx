namespace ChocolArm64.Decoders
{
    interface IOpCodeBReg32 : IOpCode32
    {
        int Rm { get; }
    }
}