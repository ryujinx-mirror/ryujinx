namespace Ryujinx.Horizon.Audio
{
    class HwopusMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            HwopusIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
