namespace Ryujinx.Cpu.LightningJit.Graph
{
    readonly struct RegisterUse
    {
        public readonly RegisterMask Read;
        public readonly RegisterMask Write;

        public RegisterUse(RegisterMask read, RegisterMask write)
        {
            Read = read;
            Write = write;
        }

        public RegisterUse(
            uint gprReadMask,
            uint gprWriteMask,
            uint fpSimdReadMask,
            uint fpSimdWriteMask,
            uint pStateReadMask,
            uint pStateWriteMask) : this(new(gprReadMask, fpSimdReadMask, pStateReadMask), new(gprWriteMask, fpSimdWriteMask, pStateWriteMask))
        {
        }
    }
}
