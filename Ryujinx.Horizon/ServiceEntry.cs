using Ryujinx.Horizon.Common;
using Ryujinx.Memory;
using System;

namespace Ryujinx.Horizon
{
    public struct ServiceEntry
    {
        private readonly Action _entrypoint;
        private readonly HorizonOptions _options;

        internal ServiceEntry(Action entrypoint, HorizonOptions options)
        {
            _entrypoint = entrypoint;
            _options = options;
        }

        public void Start(ISyscallApi syscallApi, IVirtualMemoryManager addressSpace, IThreadContext threadContext)
        {
            HorizonStatic.Register(_options, syscallApi, addressSpace, threadContext, (int)threadContext.GetX(1));

            _entrypoint();
        }
    }
}
