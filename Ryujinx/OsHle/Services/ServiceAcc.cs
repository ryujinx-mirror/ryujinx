using Ryujinx.OsHle.Objects;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long AccU0ListOpenUsers(ServiceCtx Context)
        {
            return 0;
        }

        public static long AccU0GetProfile(ServiceCtx Context)
        {
            MakeObject(Context, new AccIProfile());

            return 0;
        }

        public static long AccU0InitializeApplicationInfo(ServiceCtx Context)
        {
            return 0;
        }

        public static long AccU0GetBaasAccountManagerForApplication(ServiceCtx Context)
        {
            MakeObject(Context, new AccIManagerForApplication());

            return 0;
        }
    }
}