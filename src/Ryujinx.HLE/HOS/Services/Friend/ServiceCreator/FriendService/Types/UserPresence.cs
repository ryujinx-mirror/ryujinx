using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.FriendService
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x8)]
    struct UserPresence
    {
        public UserId UserId;
        public long LastTimeOnlineTimestamp;
        public PresenceStatus Status;

        [MarshalAs(UnmanagedType.I1)]
        public bool SamePresenceGroupApplication;

        public Array3<byte> Unknown;
        private AppKeyValueStorageHolder _appKeyValueStorage;

        public Span<byte> AppKeyValueStorage => MemoryMarshal.Cast<AppKeyValueStorageHolder, byte>(MemoryMarshal.CreateSpan(ref _appKeyValueStorage, AppKeyValueStorageHolder.Size));

        [StructLayout(LayoutKind.Sequential, Pack = 0x1, Size = Size)]
        private struct AppKeyValueStorageHolder
        {
            public const int Size = 0xC0;
        }

        public readonly override string ToString()
        {
            return $"UserPresence {{ UserId: {UserId}, LastTimeOnlineTimestamp: {LastTimeOnlineTimestamp}, Status: {Status} }}";
        }
    }
}
