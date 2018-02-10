using Ryujinx.OsHle.Objects.Apm;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long ApmOpenSession(ServiceCtx Context)
        {
            MakeObject(Context, new ISession());

            return 0;
        }
    }
}