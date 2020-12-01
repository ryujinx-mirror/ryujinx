using System;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    struct OnScopeExit : IDisposable
    {
        private readonly Action _action;
        public OnScopeExit(Action action) => _action = action;
        public void Dispose() => _action();
    }
}
