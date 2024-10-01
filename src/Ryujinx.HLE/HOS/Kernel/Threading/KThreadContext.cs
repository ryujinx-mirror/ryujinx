using Ryujinx.Cpu;
using Ryujinx.Horizon.Common;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KThreadContext : IThreadContext
    {
        private readonly IExecutionContext _context;

        public bool Running => _context.Running;
        public ulong TlsAddress => (ulong)_context.TpidrroEl0;

        public ulong GetX(int index) => _context.GetX(index);

        private int _locked;

        public KThreadContext(IExecutionContext context)
        {
            _context = context;
        }

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
