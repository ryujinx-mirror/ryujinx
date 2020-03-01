using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Mii.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = Size)]
    struct Ver3StoreData
    {
        public const int Size = 0x60;

        private byte _storage;

        public Span<byte> Storage => MemoryMarshal.CreateSpan(ref _storage, Size);

        // TODO: define all getters/setters
    }
}
