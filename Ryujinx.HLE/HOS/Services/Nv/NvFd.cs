namespace Ryujinx.HLE.HOS.Services.Nv
{
    class NvFd
    {
        public string Name { get; private set; }

        public NvFd(string name)
        {
            Name = name;
        }
    }
}