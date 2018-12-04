namespace Ryujinx.HLE.HOS.Services.Bsd
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

        public int           SocketFd     { get; }
        public BsdSocket     Socket       { get; }
        public EventTypeMask InputEvents  { get; }
        public EventTypeMask OutputEvents { get; }

        public PollEvent(int socketFd, BsdSocket socket, EventTypeMask inputEvents, EventTypeMask outputEvents)
        {
            SocketFd     = socketFd;
            Socket       = socket;
            InputEvents  = inputEvents;
            OutputEvents = outputEvents;
        }
    }
}
