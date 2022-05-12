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
        private bool _disableTransform;

        public void Set(int first, SpanRef<Viewport> viewports, bool disableTransform)
        {
            _first = first;
            _viewports = viewports;
            _disableTransform = disableTransform;
        }

        public static void Run(ref SetViewportsCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ReadOnlySpan<Viewport> viewports = command._viewports.Get(threaded);
            renderer.Pipeline.SetViewports(command._first, viewports, command._disableTransform);
            command._viewports.Dispose(threaded);
        }
    }
}
