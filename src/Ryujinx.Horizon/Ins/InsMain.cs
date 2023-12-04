namespace Ryujinx.Horizon.Ins
{
    class InsMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            InsIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
