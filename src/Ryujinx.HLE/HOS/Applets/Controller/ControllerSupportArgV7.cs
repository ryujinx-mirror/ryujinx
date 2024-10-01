using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets
{
#pragma warning disable CS0649 // Field is never assigned to
    // (8.0.0+ version)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ControllerSupportArgV7
    {
        public ControllerSupportArgHeader Header;
        public Array8<uint> IdentificationColor;
        public byte EnableExplainText;
        public ExplainTextStruct ExplainText;

        [StructLayout(LayoutKind.Sequential, Size = 8 * 0x81)]
        public struct ExplainTextStruct
        {
            private byte element;

            public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref element, 8 * 0x81);
        }
    }
#pragma warning restore CS0649
}
