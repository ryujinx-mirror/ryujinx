using Ryujinx.Core.OsHle.Objects.Friend;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Services
{
    static partial class Service
    {
        public static long FriendCreateFriendService(ServiceCtx Context)
        {
            MakeObject(Context, new IFriendService());

            return 0;
        }
    }
}