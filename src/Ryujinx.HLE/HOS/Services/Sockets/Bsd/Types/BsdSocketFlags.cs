namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    enum BsdSocketFlags
    {
        None = 0,
        Oob = 0x1,
        Peek = 0x2,
        DontRoute = 0x4,
        Eor = 0x8,
        Trunc = 0x10,
        CTrunc = 0x20,
        WaitAll = 0x40,
        DontWait = 0x80,
        Eof = 0x100,
        Notification = 0x2000,
        Nbio = 0x4000,
        Compat = 0x8000,
        SoCallbck = 0x10000,
        NoSignal = 0x20000,
        CMsgCloExec = 0x40000,
    }
}
