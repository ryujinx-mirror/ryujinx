namespace Ryujinx.Horizon.Friends
{
    class FriendsMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            FriendsIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
