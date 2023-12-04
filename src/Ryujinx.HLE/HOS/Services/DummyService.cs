namespace Ryujinx.HLE.HOS.Services
{
    class DummyService : IpcService
    {
        public string ServiceName { get; set; }

        public DummyService(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}
