using Ryujinx.Common.Memory;
using Ryujinx.Horizon.Sdk.Settings;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Horizon.Sdk.Friends
{
    [StructLayout(LayoutKind.Sequential, Size = 0x48)]
    struct InAppScreenName
    {
        public Array64<byte> Name;
        public LanguageCode LanguageCode;

        public override readonly string ToString()
        {
            int length = Name.AsSpan().IndexOf((byte)0);
            if (length < 0)
            {
                length = 64;
            }

            return Encoding.UTF8.GetString(Name.AsSpan()[..length]);
        }
    }
}
