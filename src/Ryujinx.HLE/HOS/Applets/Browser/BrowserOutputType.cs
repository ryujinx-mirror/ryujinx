namespace Ryujinx.HLE.HOS.Applets.Browser
{
    enum BrowserOutputType : ushort
    {
        ExitReason                        = 0x1,
        LastUrl                           = 0x2,
        LastUrlSize                       = 0x3,
        SharePostResult                   = 0x4,
        PostServiceName                   = 0x5,
        PostServiceNameSize               = 0x6,
        PostId                            = 0x7,
        MediaPlayerAutoClosedByCompletion = 0x8
    }
}
