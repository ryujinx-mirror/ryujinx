using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    ref struct CmifRequest
    {
        public HipcMessageData Hipc;
        public Span<byte> Data;
        public Span<ushort> OutPointerSizes;
        public Span<uint> Objects;
        public int ServerPointerSize;
    }
}
