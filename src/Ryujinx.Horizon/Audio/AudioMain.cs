namespace Ryujinx.Horizon.Audio
{
    class AudioMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            AudioUserIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
