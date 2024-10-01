using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [StructLayout(LayoutKind.Sequential, Size = 0x80)]
    struct DeviceNickName
    {
        public Array128<byte> Value;

        public DeviceNickName(string value)
        {
            int bytesWritten = Encoding.ASCII.GetBytes(value, Value.AsSpan());
            if (bytesWritten < 128)
            {
                Value[bytesWritten] = 0;
            }
            else
            {
                Value[127] = 0;
            }
        }
    }
}
