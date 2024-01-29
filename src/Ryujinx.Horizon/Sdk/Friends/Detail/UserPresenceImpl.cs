using Ryujinx.Common.Memory;
using Ryujinx.Horizon.Sdk.Account;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Friends.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0xE0)]
    struct UserPresenceImpl
    {
        public Uid UserId;
        public long LastTimeOnlineTimestamp;
        public PresenceStatus Status;
        public bool SamePresenceGroupApplication;
        public Array3<byte> Unknown;
        public AppKeyValueStorageHolder AppKeyValueStorage;

        [InlineArray(0xC0)]
        public struct AppKeyValueStorageHolder
        {
            public byte Value;
        }

        public readonly override string ToString()
        {
            return $"{{ UserId: {UserId}, LastTimeOnlineTimestamp: {LastTimeOnlineTimestamp}, Status: {Status} }}";
        }
    }
}
