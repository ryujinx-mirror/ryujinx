using Ryujinx.HLE.HOS.Services.Account.Acc;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.FriendService
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x8, CharSet = CharSet.Ansi)]
    struct UserPresence
    {
        public UserId         UserId;
        public long           LastTimeOnlineTimestamp;
        public PresenceStatus Status;

        [MarshalAs(UnmanagedType.I1)]
        public bool SamePresenceGroupApplication;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3)]
        public char[] Unknown;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xC0)]
        public char[] AppKeyValueStorage;

        public override string ToString()
        {
            return $"UserPresence {{ UserId: {UserId}, LastTimeOnlineTimestamp: {LastTimeOnlineTimestamp}, Status: {Status}, AppKeyValueStorage: {AppKeyValueStorage} }}";
        }
    }
}