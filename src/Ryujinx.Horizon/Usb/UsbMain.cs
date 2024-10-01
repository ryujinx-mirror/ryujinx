namespace Ryujinx.Horizon.Usb
{
    class UsbMain : IService
    {
        public static void Main(ServiceTable serviceTable)
        {
            UsbIpcServer ipcServer = new();

            ipcServer.Initialize();

            serviceTable.SignalServiceReady();

            ipcServer.ServiceRequests();
            ipcServer.Shutdown();
        }
    }
}
