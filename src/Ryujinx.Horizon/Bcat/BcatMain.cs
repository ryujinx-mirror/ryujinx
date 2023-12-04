namespace Ryujinx.Horizon.Bcat
{
    internal class BcatMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            BcatIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
