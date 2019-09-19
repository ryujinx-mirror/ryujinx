namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    class PollEvent
    {
        public enum EventTypeMask
        {
            Input        = 1,
            UrgentInput  = 2,
            Output       = 4,
            Error        = 8,
            Disconnected = 0x10,
            Invalid      = 0x20
        }

        public int           SocketFd     { get; private set; }
        public BsdSocket     Socket       { get; private set; }
        public EventTypeMask InputEvents  { get; private set; }
        public EventTypeMask OutputEvents { get; private set; }

        public PollEvent(int socketFd, BsdSocket socket, EventTypeMask inputEvents, EventTypeMask outputEvents)
        {
            SocketFd     = socketFd;
            Socket       = socket;
            InputEvents  = inputEvents;
            OutputEvents = outputEvents;
        }
    }
}