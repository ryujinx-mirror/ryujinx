namespace Ryujinx.HLE.HOS
{
    static class ErrorCode
    {
        public static uint MakeError(ErrorModule module, int code)
        {
            return (uint)module | ((uint)code << 9);
        }
    }
}