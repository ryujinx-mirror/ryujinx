using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Audio.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8, Pack = 0x1)]
    struct AudioInProtocol
    {
        public AudioInProtocolName Name;
        public Array7<byte> Padding;

        public AudioInProtocol(AudioInProtocolName name)
        {
            Name = name;
            Padding = new();
        }

        public override readonly string ToString()
        {
            return Name.ToString();
        }
    }
}
