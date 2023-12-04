using Ryujinx.Common.Memory;
using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xaa)]
    public struct ProxySetting
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool Enabled;
        private readonly byte _padding;
        public short Port;
        private NameStruct _name;
        [MarshalAs(UnmanagedType.I1)]
        public bool AutoAuthEnabled;
        public Array32<byte> User;
        public Array32<byte> Pass;
        private readonly byte _padding2;

        [StructLayout(LayoutKind.Sequential, Size = 0x64)]
        private struct NameStruct { }

        public Span<byte> Name => SpanHelpers.AsSpan<NameStruct, byte>(ref _name);
    }
}
