using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.InlineToMemory;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Represents a 3D engine class.
    /// </summary>
    class ThreedClass : IDeviceState
    {
        private readonly GpuContext _context;
        private readonly DeviceStateWithShadow<ThreedClassState> _state;

        private readonly InlineToMemoryClass _i2mClass;
        private readonly DrawManager _drawManager;
        private readonly SemaphoreUpdater _semaphoreUpdater;
        private readonly ConstantBufferUpdater _cbUpdater;
        private readonly StateUpdater _stateUpdater;

        /// <summary>
        /// Creates a new instance of the 3D engine class.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        public ThreedClass(GpuContext context, GpuChannel channel)
        {
            _context = context;
            _state = new DeviceStateWithShadow<ThreedClassState>(new Dictionary<string, RwCallback>
            {
                { nameof(ThreedClassState.LaunchDma), new RwCallback(LaunchDma, null) },
                { nameof(ThreedClassState.LoadInlineData), new RwCallback(LoadInlineData, null) },
                { nameof(ThreedClassState.SyncpointAction), new RwCallback(IncrementSyncpoint, null) },
                { nameof(ThreedClassState.TextureBarrier), new RwCallback(TextureBarrier, null) },
                { nameof(ThreedClassState.TextureBarrierTiled), new RwCallback(TextureBarrierTiled, null) },
                { nameof(ThreedClassState.VbElementU8), new RwCallback(VbElementU8, null) },
                { nameof(ThreedClassState.VbElementU16), new RwCallback(VbElementU16, null) },
                { nameof(ThreedClassState.VbElementU32), new RwCallback(VbElementU32, null) },
                { nameof(ThreedClassState.ResetCounter), new RwCallback(ResetCounter, null) },
                { nameof(ThreedClassState.RenderEnableCondition), new RwCallback(null, Zero) },
                { nameof(ThreedClassState.DrawEnd), new RwCallback(DrawEnd, null) },
                { nameof(ThreedClassState.DrawBegin), new RwCallback(DrawBegin, null) },
                { nameof(ThreedClassState.DrawIndexedSmall), new RwCallback(DrawIndexedSmall, null) },
                { nameof(ThreedClassState.DrawIndexedSmall2), new RwCallback(DrawIndexedSmall2, null) },
                { nameof(ThreedClassState.DrawIndexedSmallIncInstance), new RwCallback(DrawIndexedSmallIncInstance, null) },
                { nameof(ThreedClassState.DrawIndexedSmallIncInstance2), new RwCallback(DrawIndexedSmallIncInstance2, null) },
                { nameof(ThreedClassState.IndexBufferCount), new RwCallback(SetIndexBufferCount, null) },
                { nameof(ThreedClassState.Clear), new RwCallback(Clear, null) },
                { nameof(ThreedClassState.SemaphoreControl), new RwCallback(Report, null) },
                { nameof(ThreedClassState.SetFalcon04), new RwCallback(SetFalcon04, null) },
                { nameof(ThreedClassState.UniformBufferUpdateData), new RwCallback(ConstantBufferUpdate, null) },
                { nameof(ThreedClassState.UniformBufferBindVertex), new RwCallback(ConstantBufferBindVertex, null) },
                { nameof(ThreedClassState.UniformBufferBindTessControl), new RwCallback(ConstantBufferBindTessControl, null) },
                { nameof(ThreedClassState.UniformBufferBindTessEvaluation), new RwCallback(ConstantBufferBindTessEvaluation, null) },
                { nameof(ThreedClassState.UniformBufferBindGeometry), new RwCallback(ConstantBufferBindGeometry, null) },
                { nameof(ThreedClassState.UniformBufferBindFragment), new RwCallback(ConstantBufferBindFragment, null) }
            });

            _i2mClass = new InlineToMemoryClass(context, channel, initializeState: false);

            var drawState = new DrawState();

            _drawManager = new DrawManager(context, channel, _state, drawState);
            _semaphoreUpdater = new SemaphoreUpdater(context, channel, _state);
            _cbUpdater = new ConstantBufferUpdater(channel, _state);
            _stateUpdater = new StateUpdater(context, channel, _state, drawState);

            // This defaults to "always", even without any register write.
            // Reads just return 0, regardless of what was set there.
            _state.State.RenderEnableCondition = Condition.Always;
        }

        /// <summary>
        /// Reads data from the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <returns>Data at the specified offset</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int offset) => _state.Read(offset);

        /// <summary>
        /// Writes data to the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <param name="data">Data to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int offset, int data)
        {
            _state.WriteWithRedundancyCheck(offset, data, out bool valueChanged);

            if (valueChanged)
            {
                _stateUpdater.SetDirty(offset);
            }
        }

        /// <summary>
        /// Sets the shadow ram control value of all sub-channels.
        /// </summary>
        /// <param name="control">New shadow ram control value</param>
        public void SetShadowRamControl(int control)
        {
            _state.State.SetMmeShadowRamControl = (uint)control;
        }

        /// <summary>
        /// Updates current host state for all registers modified since the last call to this method.
        /// </summary>
        public void UpdateState()
        {
            _cbUpdater.FlushUboDirty();
            _stateUpdater.Update();
        }

        /// <summary>
        /// Updates current host state for all registers modified since the last call to this method.
        /// </summary>
        /// <param name="mask">Mask where each bit set indicates that the respective state group index should be checked</param>
        public void UpdateState(ulong mask)
        {
            _stateUpdater.Update(mask);
        }

        /// <summary>
        /// Updates render targets (color and depth-stencil buffers) based on current render target state.
        /// </summary>
        /// <param name="useControl">Use draw buffers information from render target control register</param>
        /// <param name="singleUse">If this is not -1, it indicates that only the given indexed target will be used.</param>
        public void UpdateRenderTargetState(bool useControl, int singleUse = -1)
        {
            _stateUpdater.UpdateRenderTargetState(useControl, singleUse);
        }

        /// <summary>
        /// Marks the entire state as dirty, forcing a full host state update before the next draw.
        /// </summary>
        public void ForceStateDirty()
        {
            _stateUpdater.SetAllDirty();
        }

        /// <summary>
        /// Forces the shaders to be rebound on the next draw.
        /// </summary>
        public void ForceShaderUpdate()
        {
            _stateUpdater.ForceShaderUpdate();
        }

        /// <summary>
        /// Flushes any queued UBO updates.
        /// </summary>
        public void FlushUboDirty()
        {
            _cbUpdater.FlushUboDirty();
        }

        /// <summary>
        /// Perform any deferred draws.
        /// </summary>
        public void PerformDeferredDraws()
        {
            _drawManager.PerformDeferredDraws();
        }

        /// <summary>
        /// Updates the currently bound constant buffer.
        /// </summary>
        /// <param name="data">Data to be written to the buffer</param>
        public void ConstantBufferUpdate(ReadOnlySpan<int> data)
        {
            _cbUpdater.Update(data);
        }

        /// <summary>
        /// Launches the Inline-to-Memory DMA copy operation.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void LaunchDma(int argument)
        {
            _i2mClass.LaunchDma(ref Unsafe.As<ThreedClassState, InlineToMemoryClassState>(ref _state.State), argument);
        }

        /// <summary>
        /// Pushes a block of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="data">Data to push</param>
        public void LoadInlineData(ReadOnlySpan<int> data)
        {
            _i2mClass.LoadInlineData(data);
        }

        /// <summary>
        /// Pushes a word of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void LoadInlineData(int argument)
        {
            _i2mClass.LoadInlineData(argument);
        }

        /// <summary>
        /// Performs an incrementation on a syncpoint.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void IncrementSyncpoint(int argument)
        {
            uint syncpointId = (uint)argument & 0xFFFF;

            _context.CreateHostSyncIfNeeded();
            _context.Renderer.UpdateCounters(); // Poll the query counters, the game may want an updated result.
            _context.Synchronization.IncrementSyncpoint(syncpointId);
        }

        /// <summary>
        /// Issues a texture barrier.
        /// This waits until previous texture writes from the GPU to finish, before
        /// performing new operations with said textures.
        /// </summary>
        /// <param name="argument">Method call argument (unused)</param>
        private void TextureBarrier(int argument)
        {
            _context.Renderer.Pipeline.TextureBarrier();
        }

        /// <summary>
        /// Issues a texture barrier.
        /// This waits until previous texture writes from the GPU to finish, before
        /// performing new operations with said textures.
        /// This performs a per-tile wait, it is only valid if both the previous write
        /// and current access has the same access patterns.
        /// This may be faster than the regular barrier on tile-based rasterizers.
        /// </summary>
        /// <param name="argument">Method call argument (unused)</param>
        private void TextureBarrierTiled(int argument)
        {
            _context.Renderer.Pipeline.TextureBarrierTiled();
        }

        /// <summary>
        /// Pushes four 8-bit index buffer elements.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void VbElementU8(int argument)
        {
            _drawManager.VbElementU8(argument);
        }

        /// <summary>
        /// Pushes two 16-bit index buffer elements.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void VbElementU16(int argument)
        {
            _drawManager.VbElementU16(argument);
        }

        /// <summary>
        /// Pushes one 32-bit index buffer element.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void VbElementU32(int argument)
        {
            _drawManager.VbElementU32(argument);
        }

        /// <summary>
        /// Resets the value of an internal GPU counter back to zero.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void ResetCounter(int argument)
        {
            _semaphoreUpdater.ResetCounter(argument);
        }

        /// <summary>
        /// Finishes the draw call.
        /// This draws geometry on the bound buffers based on the current GPU state.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void DrawEnd(int argument)
        {
            _drawManager.DrawEnd(this, argument);
        }

        /// <summary>
        /// Starts draw.
        /// This sets primitive type and instanced draw parameters.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void DrawBegin(int argument)
        {
            _drawManager.DrawBegin(argument);
        }

        /// <summary>
        /// Sets the index buffer count.
        /// This also sets internal state that indicates that the next draw is an indexed draw.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void SetIndexBufferCount(int argument)
        {
            _drawManager.SetIndexBufferCount(argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmall(int argument)
        {
            _drawManager.DrawIndexedSmall(this, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmall2(int argument)
        {
            _drawManager.DrawIndexedSmall2(this, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmallIncInstance(int argument)
        {
            _drawManager.DrawIndexedSmallIncInstance(this, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmallIncInstance2(int argument)
        {
            _drawManager.DrawIndexedSmallIncInstance2(this, argument);
        }

        /// <summary>
        /// Clears the current color and depth-stencil buffers.
        /// Which buffers should be cleared is also specified on the argument.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void Clear(int argument)
        {
            _drawManager.Clear(this, argument);
        }

        /// <summary>
        /// Writes a GPU counter to guest memory.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void Report(int argument)
        {
            _semaphoreUpdater.Report(argument);
        }

        /// <summary>
        /// Performs high-level emulation of Falcon microcode function number "4".
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void SetFalcon04(int argument)
        {
            _state.State.SetMmeShadowScratch[0] = 1;
        }

        /// <summary>
        /// Updates the uniform buffer data with inline data.
        /// </summary>
        /// <param name="argument">New uniform buffer data word</param>
        private void ConstantBufferUpdate(int argument)
        {
            _cbUpdater.Update(argument);
        }

        /// <summary>
        /// Binds a uniform buffer for the vertex shader stage.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void ConstantBufferBindVertex(int argument)
        {
            _cbUpdater.BindVertex(argument);
        }

        /// <summary>
        /// Binds a uniform buffer for the tessellation control shader stage.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void ConstantBufferBindTessControl(int argument)
        {
            _cbUpdater.BindTessControl(argument);
        }

        /// <summary>
        /// Binds a uniform buffer for the tessellation evaluation shader stage.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void ConstantBufferBindTessEvaluation(int argument)
        {
            _cbUpdater.BindTessEvaluation(argument);
        }

        /// <summary>
        /// Binds a uniform buffer for the geometry shader stage.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void ConstantBufferBindGeometry(int argument)
        {
            _cbUpdater.BindGeometry(argument);
        }

        /// <summary>
        /// Binds a uniform buffer for the fragment shader stage.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void ConstantBufferBindFragment(int argument)
        {
            _cbUpdater.BindFragment(argument);
        }

        /// <summary>
        /// Generic register read function that just returns 0.
        /// </summary>
        /// <returns>Zero</returns>
        private static int Zero()
        {
            return 0;
        }

        /// <summary>
        /// Performs a indirect multi-draw, with parameters from a GPU buffer.
        /// </summary>
        /// <param name="indexCount">Index Buffer Count</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="indirectBuffer">GPU buffer with the draw parameters, such as count, first index, etc</param>
        /// <param name="parameterBuffer">GPU buffer with the draw count</param>
        /// <param name="maxDrawCount">Maximum number of draws that can be made</param>
        /// <param name="stride">Distance in bytes between each element on the <paramref name="indirectBuffer"/> array</param>
        public void MultiDrawIndirectCount(
            int indexCount,
            PrimitiveTopology topology,
            BufferRange indirectBuffer,
            BufferRange parameterBuffer,
            int maxDrawCount,
            int stride)
        {
            _drawManager.MultiDrawIndirectCount(this, indexCount, topology, indirectBuffer, parameterBuffer, maxDrawCount, stride);
        }
    }
}
