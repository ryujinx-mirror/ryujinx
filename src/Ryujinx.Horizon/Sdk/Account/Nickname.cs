using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Horizon.Sdk.Account
{
    [StructLayout(LayoutKind.Sequential, Size = 0x21, Pack = 0x1)]
    readonly struct Nickname
    {
        public readonly Array33<byte> Name;

        public Nickname(in Array33<byte> name)
        {
            Name = name;
        }

        public override string ToString()
        {
            int length = ((ReadOnlySpan<byte>)Name.AsSpan()).IndexOf((byte)0);
            if (length < 0)
            {
                length = 33;
            }

            return Encoding.UTF8.GetString(Name.AsSpan()[..length]);
        }
    }
}
