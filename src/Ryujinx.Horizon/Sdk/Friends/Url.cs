using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Horizon.Sdk.Friends
{
    [StructLayout(LayoutKind.Sequential, Size = 0xA0, Pack = 0x1)]
    struct Url
    {
        public UrlStorage Path;

        [InlineArray(0xA0)]
        public struct UrlStorage
        {
            public byte Value;
        }

        public override readonly string ToString()
        {
            int length = ((ReadOnlySpan<byte>)Path).IndexOf((byte)0);
            if (length < 0)
            {
                length = 33;
            }

            return Encoding.UTF8.GetString(((ReadOnlySpan<byte>)Path)[..length]);
        }
    }
}
