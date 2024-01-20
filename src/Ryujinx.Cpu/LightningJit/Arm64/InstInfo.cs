using Ryujinx.Cpu.LightningJit.Graph;

namespace Ryujinx.Cpu.LightningJit.Arm64
{
    readonly struct InstInfo
    {
        public readonly uint Encoding;
        public readonly InstName Name;
        public readonly InstFlags Flags;
        public readonly AddressForm AddressForm;
        public readonly RegisterUse RegisterUse;

        public InstInfo(uint encoding, InstName name, InstFlags flags, AddressForm addressForm, in RegisterUse registerUse)
        {
            Encoding = encoding;
            Name = name;
            Flags = flags;
            AddressForm = addressForm;
            RegisterUse = registerUse;
        }
    }
}
