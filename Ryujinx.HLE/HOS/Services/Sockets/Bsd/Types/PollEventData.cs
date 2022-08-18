namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    struct PollEventData
    {
#pragma warning disable CS0649
        public int SocketFd;
        public PollEventTypeMask InputEvents;
#pragma warning restore CS0649
        public PollEventTypeMask OutputEvents;
    }
}
