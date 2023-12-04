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
        public uint Reserved1;
        public uint Reserved2;
        public uint Reserved3;
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
