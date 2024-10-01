namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    enum BsdSocketType
    {
        Stream = 1,
        Dgram,
        Raw,
        Rdm,
        Seqpacket,

        TypeMask = 0xFFFFFFF,
    }
}
