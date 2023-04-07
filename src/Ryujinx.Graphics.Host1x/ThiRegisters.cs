using Ryujinx.Common.Memory;

namespace Ryujinx.Graphics.Host1x
{
    struct ThiRegisters
    {
#pragma warning disable CS0649
        public uint IncrSyncpt;
        public uint Reserved4;
        public uint IncrSyncptErr;
        public uint CtxswIncrSyncpt;
        public Array4<uint> Reserved10;
        public uint Ctxsw;
        public uint Reserved24;
        public uint ContSyncptEof;
        public Array5<uint> Reserved2C;
        public uint Method0;
        public uint Method1;
        public Array12<uint> Reserved48;
        public uint IntStatus;
        public uint IntMask;
#pragma warning restore CS0649
    }
}
