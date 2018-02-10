using Ryujinx.OsHle.Objects.Am;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
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