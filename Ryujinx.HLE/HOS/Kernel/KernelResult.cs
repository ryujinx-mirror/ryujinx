namespace Ryujinx.HLE.HOS.Kernel
{
    enum KernelResult
    {
        Success         = 0,
        HandleTableFull = 0xd201,
        InvalidHandle   = 0xe401,
        InvalidState    = 0xfa01
    }
}