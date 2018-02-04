namespace Ryujinx.OsHle.Objects
{
    class AmICommonStateGetter
    {
        private enum FocusState
        {
            InFocus    = 1,
            OutOfFocus = 2
        }

        private enum OperationMode
        {
            Handheld = 0,
            Docked   = 1
        }

        public static long GetEventHandle(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L);

            return 0;
        }

        public static long ReceiveMessage(ServiceCtx Context)
        {
            //Program expects 0xF at 0x17ae70 on puyo sdk,
            //otherwise runs on a infinite loop until it reads said value.
            //What it means is still unknown.
            Context.ResponseData.Write(0xfL);

            return 0; //0x680;
        }

        public static long GetOperationMode(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)OperationMode.Handheld);

            return 0;
        }

        public static long GetPerformanceMode(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)0);

            return 0;
        }

        public static long GetCurrentFocusState(ServiceCtx Context)
        {
            Context.ResponseData.Write((byte)FocusState.InFocus);

            return 0;
        }
    }
}