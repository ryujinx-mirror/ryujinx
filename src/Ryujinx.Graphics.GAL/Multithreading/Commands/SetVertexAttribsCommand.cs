using Ryujinx.Graphics.GAL.Multithreading.Model;
using System;

namespace Ryujinx.Graphics.GAL.Multithreading.Commands
{
    struct SetVertexAttribsCommand : IGALCommand, IGALCommand<SetVertexAttribsCommand>
    {
        public readonly CommandType CommandType => CommandType.SetVertexAttribs;
        private SpanRef<VertexAttribDescriptor> _vertexAttribs;

        public void Set(SpanRef<VertexAttribDescriptor> vertexAttribs)
        {
            _vertexAttribs = vertexAttribs;
        }

        public static void Run(ref SetVertexAttribsCommand command, ThreadedRenderer threaded, IRenderer renderer)
        {
            ReadOnlySpan<VertexAttribDescriptor> vertexAttribs = command._vertexAttribs.Get(threaded);
            renderer.Pipeline.SetVertexAttribs(vertexAttribs);
            command._vertexAttribs.Dispose(threaded);
        }
    }
}
