namespace Ryujinx.Core.OsHle
{
    static class ErrorCode
    {
        public static uint MakeError(ErrorModule Module, int Code)
        {
            return (uint)Module | ((uint)Code << 9);
        }
    }
}