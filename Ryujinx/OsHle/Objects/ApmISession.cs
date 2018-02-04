namespace Ryujinx.OsHle.Objects
{
    class ApmISession
    {
        public static long SetPerformanceConfiguration(ServiceCtx Context)
        {
            int PerfMode   = Context.RequestData.ReadInt32();
            int PerfConfig = Context.RequestData.ReadInt32();

            return 0;
        }
    }
}