namespace Ryujinx.OsHle
{
    class FileDesc
    {
        public string Name { get; private set; }

        public FileDesc(string Name)
        {
            this.Name = Name;
        }
    }
}