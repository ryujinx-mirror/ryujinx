using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.NotificationService
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x8, Size = 0x10)]
    struct NotificationInfo
    {
        public NotificationEventType Type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4)]
        public char[] Padding;

        public long NetworkUserIdPlaceholder;
    }
}