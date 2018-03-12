namespace Ryujinx.Core.OsHle.IpcServices.NvServices
{
    class NvFd
    {
        public string Name { get; private set; }

        public NvFd(string Name)
        {
            this.Name = Name;
        }
    }
}