namespace Ryujinx.Core.OsHle.IpcServices
{
    static class ErrorCode
    {
        public static long MakeError(ErrorModule Module, int Code)
        {
            return (int)Module | (Code << 9);
        }
    }
}