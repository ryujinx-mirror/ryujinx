using Ryujinx.Core.OsHle.Objects.Apm;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Services
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