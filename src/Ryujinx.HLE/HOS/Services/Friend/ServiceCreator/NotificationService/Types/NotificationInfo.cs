using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.NotificationService
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    struct NotificationInfo
    {
        public NotificationEventType Type;
        private Array4<byte> _padding;
        public long NetworkUserIdPlaceholder;
    }
}