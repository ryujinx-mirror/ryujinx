namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
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
