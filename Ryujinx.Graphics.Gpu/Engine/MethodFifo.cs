using Ryujinx.Graphics.Gpu.State;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Writes a GPU counter to guest memory.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void Semaphore(GpuState state, int argument)
        {
            FifoSemaphoreOperation op = (FifoSemaphoreOperation)(argument & 3);

            var semaphore = state.Get<SemaphoreState>(MethodOffset.Semaphore);

            int value = semaphore.Payload;

            if (op == FifoSemaphoreOperation.Counter)
            {
                // TODO: There's much more that should be done here.
                // NVN only supports the "Accumulate" mode, so we
                // can't currently guess which bits specify the
                // reduction operation.
                value += _context.MemoryAccessor.Read<int>(semaphore.Address.Pack());
            }

            _context.MemoryAccessor.Write(semaphore.Address.Pack(), value);

            _context.AdvanceSequence();
        }

        /// <summary>
        /// Waits for the GPU to be idle.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void WaitForIdle(GpuState state, int argument)
        {
            PerformDeferredDraws();

            _context.Renderer.Pipeline.Barrier();
        }

        /// <summary>
        /// Send macro code/data to the MME.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void SendMacroCodeData(GpuState state, int argument)
        {
            int macroUploadAddress = state.Get<int>(MethodOffset.MacroUploadAddress);

            _context.Fifo.SendMacroCodeData(macroUploadAddress++, argument);

            state.Write((int)MethodOffset.MacroUploadAddress, macroUploadAddress);
        }

        /// <summary>
        /// Bind a macro index to a position for the MME.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void BindMacro(GpuState state, int argument)
        {
            int macroBindingIndex = state.Get<int>(MethodOffset.MacroBindingIndex);

            _context.Fifo.BindMacro(macroBindingIndex++, argument);

            state.Write((int)MethodOffset.MacroBindingIndex, macroBindingIndex);
        }

        public void SetMmeShadowRamControl(GpuState state, int argument)
        {
            _context.Fifo.SetMmeShadowRamControl((ShadowRamControl)argument);
        }

        /// <summary>
        /// Apply a fence operation on a syncpoint.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void FenceAction(GpuState state, int argument)
        {
            uint threshold = state.Get<uint>(MethodOffset.FenceValue);

            FenceActionOperation operation = (FenceActionOperation)(argument & 1);

            uint syncpointId = (uint)(argument >> 8) & 0xFF;

            if (operation == FenceActionOperation.Acquire)
            {
                _context.Synchronization.WaitOnSyncpoint(syncpointId, threshold, Timeout.InfiniteTimeSpan);
            }
            else if (operation == FenceActionOperation.Increment)
            {
                _context.Synchronization.IncrementSyncpoint(syncpointId);
            }
        }
    }
}
