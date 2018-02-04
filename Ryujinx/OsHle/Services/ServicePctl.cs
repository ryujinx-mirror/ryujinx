using Ryujinx.OsHle.Objects;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long PctlCreateService(ServiceCtx Context)
        {
            MakeObject(Context, new AmIParentalControlService());

            return 0;
        }
    }
}