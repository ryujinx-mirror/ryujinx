namespace Ryujinx.Horizon.Prepo
{
    class PrepoMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            PrepoIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
