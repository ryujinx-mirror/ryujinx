namespace Ryujinx.Horizon.Lbl
{
    class LblMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            LblIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
