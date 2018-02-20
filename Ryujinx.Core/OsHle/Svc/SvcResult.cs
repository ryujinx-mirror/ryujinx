namespace Ryujinx.Core.OsHle.Svc
{
    enum SvcResult
    {
        Success      = 0,
        ErrBadHandle = 0xe401,
        ErrTimeout   = 0xea01,
        ErrBadInfo   = 0xf001,
        ErrBadIpcReq = 0xf601
    }
}