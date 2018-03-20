namespace Ryujinx.Core.OsHle.Services.Nv
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