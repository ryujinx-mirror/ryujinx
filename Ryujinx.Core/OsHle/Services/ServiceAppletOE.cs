using Ryujinx.Core.OsHle.Objects.Am;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Services
{
    static partial class Service
    {
        public static long AppletOpenApplicationProxy(ServiceCtx Context)
        {
            MakeObject(Context, new IApplicationProxy());

            return 0;
        }
    }
}