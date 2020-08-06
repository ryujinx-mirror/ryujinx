namespace Ryujinx.Graphics.Nvdec.Types.Vp9
{
    struct FrameStats
    {
#pragma warning disable CS0649
        public uint Unknown0;
        public uint Unknown4;
        public uint Pass2CycleCount;
        public uint ErrorStatus;
        public uint FrameStatusIntraCnt;
        public uint FrameStatusInterCnt;
        public uint FrameStatusSkipCtuCount;
        public uint FrameStatusFwdMvxCnt;
        public uint FrameStatusFwdMvyCnt;
        public uint FrameStatusBwdMvxCnt;
        public uint FrameStatusBwdMvyCnt;
        public uint ErrorCtbPos;
        public uint ErrorSlicePos;
        public uint Unknown34;
#pragma warning restore CS0649
    }
}
