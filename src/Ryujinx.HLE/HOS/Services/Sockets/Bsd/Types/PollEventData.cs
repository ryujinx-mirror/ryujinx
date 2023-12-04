namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    struct PollEventData
    {
#pragma warning disable CS0649 // Field is never assigned to
        public int SocketFd;
        public PollEventTypeMask InputEvents;
#pragma warning restore CS0649
        public PollEventTypeMask OutputEvents;
    }
}
