namespace Ryujinx.Graphics.GAL
{
    public readonly struct DeviceInfo
    {
        public readonly string Id;
        public readonly string Vendor;
        public readonly string Name;
        public readonly bool IsDiscrete;

        public DeviceInfo(string id, string vendor, string name, bool isDiscrete)
        {
            Id = id;
            Vendor = vendor;
            Name = name;
            IsDiscrete = isDiscrete;
        }
    }
}
