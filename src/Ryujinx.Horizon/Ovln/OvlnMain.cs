namespace Ryujinx.Horizon.Ovln
{
    class OvlnMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            OvlnIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
