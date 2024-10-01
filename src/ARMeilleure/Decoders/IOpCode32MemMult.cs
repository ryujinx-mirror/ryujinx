namespace ARMeilleure.Decoders
{
    interface IOpCode32MemMult : IOpCode32
    {
        int Rn { get; }

        int RegisterMask { get; }

        int PostOffset { get; }

        bool IsLoad { get; }

        int Offset { get; }
    }
}
