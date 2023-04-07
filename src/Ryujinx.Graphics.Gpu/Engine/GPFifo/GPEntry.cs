// This file was auto-generated from NVIDIA official Maxwell definitions.

namespace Ryujinx.Graphics.Gpu.Engine.GPFifo
{
    enum Entry0Fetch
    {
        Unconditional = 0,
        Conditional = 1,
    }

    enum Entry1Priv
    {
        User = 0,
        Kernel = 1,
    }

    enum Entry1Level
    {
        Main = 0,
        Subroutine = 1,
    }

    enum Entry1Sync
    {
        Proceed = 0,
        Wait = 1,
    }

    enum Entry1Opcode
    {
        Nop = 0,
        Illegal = 1,
        Crc = 2,
        PbCrc = 3,
    }

    struct GPEntry
    {
#pragma warning disable CS0649
        public uint Entry0;
#pragma warning restore CS0649
        public Entry0Fetch Entry0Fetch => (Entry0Fetch)((Entry0 >> 0) & 0x1);
        public int Entry0Get => (int)((Entry0 >> 2) & 0x3FFFFFFF);
        public int Entry0Operand => (int)(Entry0);
#pragma warning disable CS0649
        public uint Entry1;
#pragma warning restore CS0649
        public int Entry1GetHi => (int)((Entry1 >> 0) & 0xFF);
        public Entry1Priv Entry1Priv => (Entry1Priv)((Entry1 >> 8) & 0x1);
        public Entry1Level Entry1Level => (Entry1Level)((Entry1 >> 9) & 0x1);
        public int Entry1Length => (int)((Entry1 >> 10) & 0x1FFFFF);
        public Entry1Sync Entry1Sync => (Entry1Sync)((Entry1 >> 31) & 0x1);
        public Entry1Opcode Entry1Opcode => (Entry1Opcode)((Entry1 >> 0) & 0xFF);
    }
}
