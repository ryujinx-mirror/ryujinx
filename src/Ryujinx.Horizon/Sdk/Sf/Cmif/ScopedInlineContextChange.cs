using System;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    readonly struct ScopedInlineContextChange : IDisposable
    {
        private readonly int _previousContext;

        public ScopedInlineContextChange(int newContext)
        {
            _previousContext = InlineContext.Set(newContext);
        }

        public void Dispose()
        {
            InlineContext.Set(_previousContext);
        }
    }
}
