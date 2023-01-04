namespace Ryujinx.Horizon.LogManager
{
    class LmMain : IService
    {
        public static void Main()
        {
            LmIpcServer ipcServer = new LmIpcServer();

            ipcServer.Initialize();
            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
