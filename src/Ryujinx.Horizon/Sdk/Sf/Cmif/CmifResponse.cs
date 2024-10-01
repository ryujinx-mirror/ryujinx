using System;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    ref struct CmifResponse
    {
        public ReadOnlySpan<byte> Data;
        public ReadOnlySpan<uint> Objects;
        public ReadOnlySpan<int> CopyHandles;
        public ReadOnlySpan<int> MoveHandles;
    }
}
