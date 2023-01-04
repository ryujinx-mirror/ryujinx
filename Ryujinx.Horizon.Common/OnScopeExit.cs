using System;

namespace Ryujinx.Horizon.Common
{
    public struct OnScopeExit : IDisposable
    {
        private readonly Action _action;

        public OnScopeExit(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}
