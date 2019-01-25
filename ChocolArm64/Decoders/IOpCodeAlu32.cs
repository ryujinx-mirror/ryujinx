namespace ChocolArm64.Decoders
{
    interface IOpCodeAlu32 : IOpCode32
    {
        int Rd { get; }
        int Rn { get; }

        bool SetFlags { get; }
    }
}