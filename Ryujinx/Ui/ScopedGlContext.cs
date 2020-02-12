using OpenTK.Graphics;
using OpenTK.Platform;
using System;
using System.Threading;

namespace Ryujinx.Ui
{
    class ScopedGlContext : IDisposable
    {
        private IGraphicsContext _graphicsContext;

        private static readonly object _lock = new object();

        public ScopedGlContext(IWindowInfo windowInfo, IGraphicsContext graphicsContext)
        {
            _graphicsContext = graphicsContext;

            Monitor.Enter(_lock);

            MakeCurrent(windowInfo);
        }

        private void MakeCurrent(IWindowInfo windowInfo)
        {
            _graphicsContext.MakeCurrent(windowInfo);
        }

        public void Dispose()
        {
            MakeCurrent(null);

            Monitor.Exit(_lock);
        }
    }
}
