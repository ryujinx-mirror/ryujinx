namespace Ryujinx.Horizon.Hshl
{
    class HshlMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            HshlIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
