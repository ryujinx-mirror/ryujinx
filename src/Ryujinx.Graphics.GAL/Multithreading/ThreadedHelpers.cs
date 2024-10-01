using System;
using System.Threading;

namespace Ryujinx.Graphics.GAL.Multithreading
{
    static class ThreadedHelpers
    {
        public static void SpinUntilNonNull<T>(ref T obj) where T : class
        {
            Span<SpinWait> spinWait = stackalloc SpinWait[1];

            while (obj == null)
            {
                spinWait[0].SpinOnce(-1);
            }
        }

        public static void SpinUntilExchange(ref int target, int value, int comparand)
        {
            Span<SpinWait> spinWait = stackalloc SpinWait[1];

            while (Interlocked.CompareExchange(ref target, value, comparand) != comparand)
            {
                spinWait[0].SpinOnce(-1);
            }
        }
    }
}
