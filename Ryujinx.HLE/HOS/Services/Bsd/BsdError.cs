namespace Ryujinx.HLE.HOS.Services.Bsd
{
    //bsd_errno == (SocketException.ErrorCode - 10000)
    enum BsdError
    {
        Timeout = 60
    }
}