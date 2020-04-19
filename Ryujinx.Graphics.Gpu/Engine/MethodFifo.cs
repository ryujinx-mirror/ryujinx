using Ryujinx.Graphics.Gpu.State;
using System;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
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
