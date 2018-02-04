namespace Ryujinx.OsHle.Objects
{
    class AccIManagerForApplication
    {
        public static long CheckAvailability(ServiceCtx Context)
        {           
            return 0;
        }

        public static long GetAccountId(ServiceCtx Context)
        {
            Context.ResponseData.Write(0xcafeL);

            return 0;
        }
    }
}