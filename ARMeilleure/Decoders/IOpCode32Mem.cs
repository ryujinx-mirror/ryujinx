namespace ARMeilleure.Decoders
{
    interface IOpCode32Mem : IOpCode32
    {
        int Rt { get; }
        int Rn { get; }

        bool WBack { get; }
        bool IsLoad { get; }
    }
}