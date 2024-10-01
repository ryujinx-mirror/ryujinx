namespace Ryujinx.Horizon.Wlan
{
    class WlanMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            WlanIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
