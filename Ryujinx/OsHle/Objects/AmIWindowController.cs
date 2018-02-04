namespace Ryujinx.OsHle.Objects
{
    class AmIWindowController
    {
        public static long GetAppletResourceUserId(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L);

            return 0;
        }

        public static long AcquireForegroundRights(ServiceCtx Context)
        {
            return 0;
        }
    }
}