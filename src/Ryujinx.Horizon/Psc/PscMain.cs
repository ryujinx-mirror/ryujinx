namespace Ryujinx.Horizon.Psc
{
    class PscMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            PscIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
