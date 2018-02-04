namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long LmInitialize(ServiceCtx Context)
        {
            Context.Session.Initialize();

            return 0;
        }
    }
}