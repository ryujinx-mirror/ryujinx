using Ryujinx.Graphics.Nvdec.Types.Vp9;

namespace Ryujinx.Graphics.Nvdec
{
    struct NvdecStatus
    {
#pragma warning disable CS0649
        public uint MbsCorrectlyDecoded;
        public uint MbsInError;
        public uint Reserved;
        public uint ErrorStatus;
        public FrameStats Stats;
        public uint SliceHeaderErrorCode;
#pragma warning restore CS0649
    }
}