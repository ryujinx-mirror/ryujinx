namespace Ryujinx.Core.OsHle
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