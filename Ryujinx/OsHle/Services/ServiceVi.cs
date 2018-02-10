using Ryujinx.OsHle.Objects.Vi;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long ViGetDisplayService(ServiceCtx Context)
        {
            int Unknown = Context.RequestData.ReadInt32();

            MakeObject(Context, new IApplicationDisplayService());

            return 0;
        }
    }
}