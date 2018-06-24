namespace Ryujinx.HLE.Gpu.Exceptions
{
    static class GpuExceptionHelper
    {
        private const string CallCountExceeded = "Method call count exceeded the limit allowed per run!";

        public static void ThrowCallCoundExceeded()
        {
            throw new GpuException(CallCountExceeded);
        }
    }
}