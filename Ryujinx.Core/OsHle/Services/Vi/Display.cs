namespace Ryujinx.Core.OsHle.IpcServices.Vi
{
    class Display
    {
        public string Name { get; private set; }

        public Display(string Name)
        {
            this.Name = Name;
        }
    }
}