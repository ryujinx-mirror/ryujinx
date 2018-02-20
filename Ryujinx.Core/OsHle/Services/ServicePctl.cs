using Ryujinx.Core.OsHle.Objects.Am;

using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Services
{
    static partial class Service
    {
        public static long PctlCreateService(ServiceCtx Context)
        {
            MakeObject(Context, new IParentalControlService());

            return 0;
        }
    }
}