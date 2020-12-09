using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KThreadContext
    {
        private int _locked;

        public bool Lock()
        {
            return Interlocked.Exchange(ref _locked, 1) == 0;
        }

        public void Unlock()
        {
            Interlocked.Exchange(ref _locked, 0);
        }
    }
}
