using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetViewportsCommand : IGALCommand, IGALCommand<SetViewportsCommand>
    {
        public readonly CommandType CommandType => CommandType.SetViewports;
        private SpanRef<Viewport> _viewports;

        public void Set(SpanRef<Viewport> viewports)
        {
            _viewports = viewports;
        }

        public static void Run(ref SetViewportsCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ReadOnlySpan<Viewport> viewports = command._viewports.Get(threaded);
            renderer.Pipeline.SetViewports(viewports);
            command._viewports.Dispose(threaded);
        }
    }
}
