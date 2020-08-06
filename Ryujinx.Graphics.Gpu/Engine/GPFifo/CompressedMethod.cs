// This file was auto-generated from NVIDIA official Maxwell definitions.

namespace Ryujinx.Graphics.Gpu.Engine.GPFifo
{
    enum TertOp
    {
        Grp0IncMethod = 0,
        Grp0SetSubDevMask = 1,
        Grp0StoreSubDevMask = 2,
        Grp0UseSubDevMask = 3,
        Grp2NonIncMethod = 0
    }

    enum SecOp
    {
        Grp0UseTert = 0,
        IncMethod = 1,
        Grp2UseTert = 2,
        NonIncMethod = 3,
        ImmdDataMethod = 4,
        OneInc = 5,
        Reserved6 = 6,
        EndPbSegment = 7
    }

    struct CompressedMethod
    {
#pragma warning disable CS0649
        public uint Method;
#pragma warning restore CS0649
        public int MethodAddressOld => (int)((Method >> 2) & 0x7FF);
        public int MethodAddress => (int)((Method >> 0) & 0xFFF);
        public int SubdeviceMask => (int)((Method >> 4) & 0xFFF);
        public int MethodSubchannel => (int)((Method >> 13) & 0x7);
        public TertOp TertOp => (TertOp)((Method >> 16) & 0x3);
        public int MethodCountOld => (int)((Method >> 18) & 0x7FF);
        public int MethodCount => (int)((Method >> 16) & 0x1FFF);
        public int ImmdData => (int)((Method >> 16) & 0x1FFF);
        public SecOp SecOp => (SecOp)((Method >> 29) & 0x7);
    }
}
