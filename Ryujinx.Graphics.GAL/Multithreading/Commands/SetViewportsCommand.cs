using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;
using System.Buffers;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetViewportsCommand : IGALCommand
    {
        public CommandType CommandType => CommandType.SetViewports;
        private int _first;
        private SpanRef<Viewport> _viewports;

        public void Set(int first, SpanRef<Viewport> viewports)
        {
            _first = first;
            _viewports = viewports;
        }

        public static void Run(ref SetViewportsCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ReadOnlySpan<Viewport> viewports = command._viewports.Get(threaded);
            renderer.Pipeline.SetViewports(command._first, viewports);
            command._viewports.Dispose(threaded);
        }
    }
}
