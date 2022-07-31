using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;
using System.Buffers;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetViewportsCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.SetViewports;
        private SpanRef<Viewport> _viewports;
        private bool _disableTransform;

        public void Set(SpanRef<Viewport> viewports, bool disableTransform)
        {
            _viewports = viewports;
            _disableTransform = disableTransform;
        }

        public static void Run(ref SetViewportsCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ReadOnlySpan<Viewport> viewports = command._viewports.Get(threaded);
            renderer.Pipeline.SetViewports(viewports, command._disableTransform);
            command._viewports.Dispose(threaded);
        }
    }
}
