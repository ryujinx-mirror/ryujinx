// ReSharper disable InconsistentNaming
namespace Ryujinx.Tests.Unicorn
{
    public enum UnicornError
    {
        UC_ERR_OK = 0,             // No error: everything was fine
        UC_ERR_NOMEM,              // Out-Of-Memory error: uc_open(), uc_emulate()
        UC_ERR_ARCH,               // Unsupported architecture: uc_open()
        UC_ERR_HANDLE,             // Invalid handle
        UC_ERR_MODE,               // Invalid/unsupported mode: uc_open()
        UC_ERR_VERSION,            // Unsupported version (bindings)
        UC_ERR_READ_UNMAPPED,      // Quit emulation due to READ on unmapped memory: uc_emu_start()
        UC_ERR_WRITE_UNMAPPED,     // Quit emulation due to WRITE on unmapped memory: uc_emu_start()
        UC_ERR_FETCH_UNMAPPED,     // Quit emulation due to FETCH on unmapped memory: uc_emu_start()
        UC_ERR_HOOK,               // Invalid hook type: uc_hook_add()
        UC_ERR_INSN_INVALID,       // Quit emulation due to invalid instruction: uc_emu_start()
        UC_ERR_MAP,                // Invalid memory mapping: uc_mem_map()
        UC_ERR_WRITE_PROT,         // Quit emulation due to UC_MEM_WRITE_PROT violation: uc_emu_start()
        UC_ERR_READ_PROT,          // Quit emulation due to UC_MEM_READ_PROT violation: uc_emu_start()
        UC_ERR_FETCH_PROT,         // Quit emulation due to UC_MEM_FETCH_PROT violation: uc_emu_start()
        UC_ERR_ARG,                // Invalid argument provided to uc_xxx function (See specific function API)
        UC_ERR_READ_UNALIGNED,     // Unaligned read
        UC_ERR_WRITE_UNALIGNED,    // Unaligned write
        UC_ERR_FETCH_UNALIGNED,    // Unaligned fetch
        UC_ERR_HOOK_EXIST,         // hook for this event already existed
        UC_ERR_RESOURCE,           // Insufficient resource: uc_emu_start()
        UC_ERR_EXCEPTION           // Unhandled CPU exception
    }
}
