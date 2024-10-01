namespace Ryujinx.Horizon.LogManager
{
    class LmMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            LmIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
