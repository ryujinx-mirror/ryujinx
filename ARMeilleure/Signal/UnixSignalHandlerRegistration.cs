using System;
using System.Runtime.InteropServices;

namespace ARMeilleure.Signal
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct SigSet
    {
        fixed long sa_mask[16];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SigAction
    {
        public IntPtr sa_handler;
        public SigSet sa_mask;
        public int sa_flags;
        public IntPtr sa_restorer;
    }

    static class UnixSignalHandlerRegistration
    {
        private const int SIGSEGV = 11;
        private const int SIGBUS = 10;
        private const int SA_SIGINFO = 0x00000004;

        [DllImport("libc", SetLastError = true)]
        private static extern int sigaction(int signum, ref SigAction sigAction, out SigAction oldAction);

        [DllImport("libc", SetLastError = true)]
        private static extern int sigemptyset(ref SigSet set);

        public static SigAction RegisterExceptionHandler(IntPtr action)
        {
            SigAction sig = new SigAction
            {
                sa_handler = action,
                sa_flags = SA_SIGINFO
            };

            sigemptyset(ref sig.sa_mask);

            int result = sigaction(SIGSEGV, ref sig, out SigAction old);

            if (result != 0)
            {
                throw new InvalidOperationException($"Could not register SIGSEGV sigaction. Error: {result}");
            }

            if (OperatingSystem.IsMacOS())
            {
                result = sigaction(SIGBUS, ref sig, out SigAction oldb);

                if (result != 0)
                {
                    throw new InvalidOperationException($"Could not register SIGBUS sigaction. Error: {result}");
                }
            }

            return old;
        }

        public static bool RestoreExceptionHandler(SigAction oldAction)
        {
            return sigaction(SIGSEGV, ref oldAction, out SigAction _) == 0 && (!OperatingSystem.IsMacOS() || sigaction(SIGBUS, ref oldAction, out SigAction _) == 0);
        }
    }
}
