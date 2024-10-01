using Ryujinx.Common.Memory;
using Ryujinx.Horizon.Sdk.Account;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Friends.Detail
{
    [StructLayout(LayoutKind.Sequential, Size = 0x200, Pack = 0x8)]
    struct FriendImpl
    {
        public Uid UserId;
        public NetworkServiceAccountId NetworkUserId;
        public Nickname Nickname;
        public UserPresenceImpl Presence;
        public bool IsFavourite;
        public bool IsNew;
        public Array6<byte> Unknown;
        public bool IsValid;
    }
}
