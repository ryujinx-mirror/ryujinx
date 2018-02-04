using Ryujinx.OsHle.Objects;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long AppletOpenApplicationProxy(ServiceCtx Context)
        {
            MakeObject(Context, new AmIApplicationProxy());

            return 0;
        }
    }
}