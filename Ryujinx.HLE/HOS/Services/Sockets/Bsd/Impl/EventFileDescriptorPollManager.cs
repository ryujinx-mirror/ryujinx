using Ryujinx.Common.Logging;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    class EventFileDescriptorPollManager : IPollManager
    {
        private static EventFileDescriptorPollManager _instance;

        public static EventFileDescriptorPollManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventFileDescriptorPollManager();
                }

                return _instance;
            }
        }

        public bool IsCompatible(PollEvent evnt)
        {
            return evnt.FileDescriptor is EventFileDescriptor;
        }

        public LinuxError Poll(List<PollEvent> events, int timeoutMilliseconds, out int updatedCount)
        {
            updatedCount = 0;

            List<ManualResetEvent> waiters = new List<ManualResetEvent>();

            for (int i = 0; i < events.Count; i++)
            {
                PollEvent evnt = events[i];

                EventFileDescriptor socket = (EventFileDescriptor)evnt.FileDescriptor;

                bool isValidEvent = false;

                if (evnt.Data.InputEvents.HasFlag(PollEventTypeMask.Input) ||
                    evnt.Data.InputEvents.HasFlag(PollEventTypeMask.UrgentInput))
                {
                    waiters.Add(socket.ReadEvent);

                    isValidEvent = true;
                }
                if (evnt.Data.InputEvents.HasFlag(PollEventTypeMask.Output))
                {
                    waiters.Add(socket.WriteEvent);

                    isValidEvent = true;
                }

                if (!isValidEvent)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Poll input event type: {evnt.Data.InputEvents}");

                    return LinuxError.EINVAL;
                }
            }

            int index = WaitHandle.WaitAny(waiters.ToArray(), timeoutMilliseconds);

            if (index != WaitHandle.WaitTimeout)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    PollEvent evnt = events[i];

                    EventFileDescriptor socket = (EventFileDescriptor)evnt.FileDescriptor;

                    if ((evnt.Data.InputEvents.HasFlag(PollEventTypeMask.Input) ||
                        evnt.Data.InputEvents.HasFlag(PollEventTypeMask.UrgentInput))
                        && socket.ReadEvent.WaitOne(0))
                    {
                        waiters.Add(socket.ReadEvent);
                    }
                    if ((evnt.Data.InputEvents.HasFlag(PollEventTypeMask.Output))
                        && socket.WriteEvent.WaitOne(0))
                    {
                        waiters.Add(socket.WriteEvent);
                    }
                }
            }
            else
            {
                return LinuxError.ETIMEDOUT;
            }

            return LinuxError.SUCCESS;
        }
    }
}
