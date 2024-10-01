using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Shader;
using System;

namespace Ryujinx.Graphics.Gpu.Engine.Threed.ComputeDraw
{
    /// <summary>
    /// Vertex, tessellation and geometry as compute shader draw manager.
    /// </summary>
    class VtgAsCompute : IDisposable
    {
        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly DeviceStateWithShadow<ThreedClassState> _state;
        private readonly VtgAsComputeContext _vacContext;

        /// <summary>
        /// Creates a new instance of the vertex, tessellation and geometry as compute shader draw manager.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="state">3D engine state</param>
        public VtgAsCompute(GpuContext context, GpuChannel channel, DeviceStateWithShadow<ThreedClassState> state)
        {
            _context = context;
            _channel = channel;
            _state = state;
            _vacContext = new(context);
        }

        /// <summary>
        /// Emulates the pre-rasterization stages of a draw operation using a compute shader.
        /// </summary>
        /// <param name="engine">3D engine</param>
        /// <param name="vertexAsCompute">Vertex shader converted to compute</param>
        /// <param name="geometryAsCompute">Optional geometry shader converted to compute</param>
        /// <param name="vertexPassthroughProgram">Fragment shader with a vertex passthrough shader to feed the compute output into the fragment stage</param>
        /// <param name="topology">Primitive topology of the draw</param>
        /// <param name="count">Index or vertex count of the draw</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="firstIndex">First index on the index buffer, for indexed draws</param>
        /// <param name="firstVertex">First vertex on the vertex buffer</param>
        /// <param name="firstInstance">First instance</param>
        /// <param name="indexed">Whether the draw is indexed</param>
        public void DrawAsCompute(
            ThreedClass engine,
            ShaderAsCompute vertexAsCompute,
            ShaderAsCompute geometryAsCompute,
            IProgram vertexPassthroughProgram,
            PrimitiveTopology topology,
            int count,
            int instanceCount,
            int firstIndex,
            int firstVertex,
            int firstInstance,
            bool indexed)
        {
            VtgAsComputeState state = new(
                _context,
                _channel,
                _state,
                _vacContext,
                engine,
                vertexAsCompute,
                geometryAsCompute,
                vertexPassthroughProgram,
                topology,
                count,
                instanceCount,
                firstIndex,
                firstVertex,
                firstInstance,
                indexed);

            state.RunVertex();
            state.RunGeometry();
            state.RunFragment();

            _vacContext.FreeBuffers();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _vacContext.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
