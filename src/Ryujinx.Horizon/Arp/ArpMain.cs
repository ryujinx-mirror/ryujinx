namespace Ryujinx.Horizon.Arp
{
    class ArpMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            ArpIpcServer arpIpcServer = new();

            arpIpcServer.Initialize();

            serviceTable.ArpReader = arpIpcServer.Reader;
            serviceTable.ArpWriter = arpIpcServer.Writer;

            serviceTable.SignalServiceReady();

            arpIpcServer.ServiceRequests();
            arpIpcServer.Shutdown();
        }
    }
}
