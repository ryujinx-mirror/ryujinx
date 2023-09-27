namespace Ryujinx.Horizon.Ngc
{
    class NgcMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            NgcIpcServer ipcServer = new();

            ipcServer.Initialize(HorizonStatic.Options.FsClient);

            // TODO: Notification thread, requires implementing OpenSystemDataUpdateEventNotifier on FS.
            // The notification thread seems to wait until the event returned by OpenSystemDataUpdateEventNotifier is signalled
            // in a loop. When it receives the signal, it calls ContentsReader.Reload and then waits for the next signal.

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
