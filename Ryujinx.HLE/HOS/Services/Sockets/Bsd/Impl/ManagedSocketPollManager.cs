using Ryujinx.Common.Logging;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    class ManagedSocketPollManager : IPollManager
    {
        private static ManagedSocketPollManager _instance;

        public static ManagedSocketPollManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ManagedSocketPollManager();
                }

                return _instance;
            }
        }

        public bool IsCompatible(PollEvent evnt)
        {
            return evnt.FileDescriptor is ManagedSocket;
        }

        public LinuxError Poll(List<PollEvent> events, int timeoutMilliseconds, out int updatedCount)
        {
            List<Socket> readEvents = new List<Socket>();
            List<Socket> writeEvents = new List<Socket>();
            List<Socket> errorEvents = new List<Socket>();

            updatedCount = 0;

            foreach (PollEvent evnt in events)
            {
                ManagedSocket socket = (ManagedSocket)evnt.FileDescriptor;

                bool isValidEvent = false;

                if ((evnt.Data.InputEvents & PollEventTypeMask.Input) != 0)
                {
                    readEvents.Add(socket.Socket);
                    errorEvents.Add(socket.Socket);

                    isValidEvent = true;
                }

                if ((evnt.Data.InputEvents & PollEventTypeMask.UrgentInput) != 0)
                {
                    readEvents.Add(socket.Socket);
                    errorEvents.Add(socket.Socket);

                    isValidEvent = true;
                }

                if ((evnt.Data.InputEvents & PollEventTypeMask.Output) != 0)
                {
                    writeEvents.Add(socket.Socket);
                    errorEvents.Add(socket.Socket);

                    isValidEvent = true;
                }

                if ((evnt.Data.InputEvents & PollEventTypeMask.Error) != 0)
                {
                    errorEvents.Add(socket.Socket);

                    isValidEvent = true;
                }

                if (!isValidEvent)
                {
                    Logger.Warning?.Print(LogClass.ServiceBsd, $"Unsupported Poll input event type: {evnt.Data.InputEvents}");
                    return LinuxError.EINVAL;
                }
            }

            try
            {
                int actualTimeoutMicroseconds = timeoutMilliseconds == -1 ? -1 : timeoutMilliseconds * 1000;

                Socket.Select(readEvents, writeEvents, errorEvents, actualTimeoutMicroseconds);
            }
            catch (SocketException exception)
            {
                return WinSockHelper.ConvertError((WsaError)exception.ErrorCode);
            }

            foreach (PollEvent evnt in events)
            {
                Socket socket = ((ManagedSocket)evnt.FileDescriptor).Socket;

                PollEventTypeMask outputEvents = 0;

                if (errorEvents.Contains(socket))
                {
                    outputEvents |= PollEventTypeMask.Error;

                    if (!socket.Connected || !socket.IsBound)
                    {
                        outputEvents |= PollEventTypeMask.Disconnected;
                    }
                }

                if (readEvents.Contains(socket))
                {
                    if ((evnt.Data.InputEvents & PollEventTypeMask.Input) != 0)
                    {
                        outputEvents |= PollEventTypeMask.Input;
                    }
                }

                if (writeEvents.Contains(socket))
                {
                    outputEvents |= PollEventTypeMask.Output;
                }

                evnt.Data.OutputEvents = outputEvents;
            }

            updatedCount = readEvents.Count + writeEvents.Count + errorEvents.Count;

            return LinuxError.SUCCESS;
        }
    }
}
