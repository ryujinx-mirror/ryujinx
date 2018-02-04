using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Objects
{
    class AmIApplicationProxy
    {
        public static long GetCommonStateGetter(ServiceCtx Context)
        {
            MakeObject(Context, new AmICommonStateGetter());

            return 0;
        }

        public static long GetSelfController(ServiceCtx Context)
        {
            MakeObject(Context, new AmISelfController());

            return 0;
        }

        public static long GetWindowController(ServiceCtx Context)
        {
            MakeObject(Context, new AmIWindowController());

            return 0;
        }

        public static long GetAudioController(ServiceCtx Context)
        {
            MakeObject(Context, new AmIAudioController());

            return 0;
        }

        public static long GetDisplayController(ServiceCtx Context)
        {
            MakeObject(Context, new AmIDisplayController());

            return 0;
        }

        public static long GetLibraryAppletCreator(ServiceCtx Context)
        {
            MakeObject(Context, new AmILibraryAppletCreator());

            return 0;
        }

        public static long GetApplicationFunctions(ServiceCtx Context)
        {
            MakeObject(Context, new AmIApplicationFunctions());

            return 0;
        }

        public static long GetDebugFunctions(ServiceCtx Context)
        {
            MakeObject(Context, new AmIDebugFunctions());

            return 0;
        }
    }
}