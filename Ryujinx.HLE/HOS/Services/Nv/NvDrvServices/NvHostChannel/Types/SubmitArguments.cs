using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct CommandBuffer
    {
        public int Mem;
        public uint Offset;
        public int WordsCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Reloc
    {
        public int CmdbufMem;
        public int CmdbufOffset;
        public int Target;
        public int TargetOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SyncptIncr
    {
        public uint Id;
        public uint Incrs;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Fence
    {
        public uint Id;
        public uint Thresh;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SubmitArguments
    {
        public int CmdBufsCount;
        public int RelocsCount;
        public int SyncptIncrsCount;
        public int FencesCount;
    }
}
