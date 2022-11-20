// Constants for Unicorn Engine. AUTO-GENERATED FILE, DO NOT EDIT

// ReSharper disable InconsistentNaming
namespace Ryujinx.Tests.Unicorn.Native.Const
{
    public enum Hook
    {
        INTR = 1,
        INSN = 2,
        CODE = 4,
        BLOCK = 8,
        MEM_READ_UNMAPPED = 16,
        MEM_WRITE_UNMAPPED = 32,
        MEM_FETCH_UNMAPPED = 64,
        MEM_READ_PROT = 128,
        MEM_WRITE_PROT = 256,
        MEM_FETCH_PROT = 512,
        MEM_READ = 1024,
        MEM_WRITE = 2048,
        MEM_FETCH = 4096,
        MEM_READ_AFTER = 8192,
        INSN_INVALID = 16384,
        EDGE_GENERATED = 32768,
        TCG_OPCODE = 65536,
        MEM_UNMAPPED = 112,
        MEM_PROT = 896,
        MEM_READ_INVALID = 144,
        MEM_WRITE_INVALID = 288,
        MEM_FETCH_INVALID = 576,
        MEM_INVALID = 1008,
        MEM_VALID = 7168,
    }
}
