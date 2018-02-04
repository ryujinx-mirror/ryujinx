using Ryujinx.OsHle.Objects;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long FriendCreateFriendService(ServiceCtx Context)
        {
            MakeObject(Context, new FriendIFriendService());

            return 0;
        }
    }
}