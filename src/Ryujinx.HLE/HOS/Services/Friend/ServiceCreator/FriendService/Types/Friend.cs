using Ryujinx.HLE.HOS.Services.Account.Acc;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.FriendService
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x8, Size = 0x200, CharSet = CharSet.Ansi)]
    struct Friend
    {
        public UserId  UserId;
        public long    NetworkUserId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x21)]
        public string Nickname;

        public UserPresence presence;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsFavourite;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsNew;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x6)]
        char[] Unknown;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsValid;
    }
}