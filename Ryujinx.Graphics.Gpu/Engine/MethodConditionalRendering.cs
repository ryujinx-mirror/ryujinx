using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Checks if draws and clears should be performed, according
        /// to currently set conditional rendering conditions.
        /// </summary>
        /// <param name="state">GPU state</param>
        /// <returns>True if rendering is enabled, false otherwise</returns>
        private ConditionalRenderEnabled GetRenderEnable(GpuState state)
        {
            ConditionState condState = state.Get<ConditionState>(MethodOffset.ConditionState);

            switch (condState.Condition)
            {
                case Condition.Always:
                    return ConditionalRenderEnabled.True;
                case Condition.Never:
                    return ConditionalRenderEnabled.False;
                case Condition.ResultNonZero:
                    return CounterNonZero(condState.Address.Pack());
                case Condition.Equal:
                    return CounterCompare(condState.Address.Pack(), true);
                case Condition.NotEqual:
                    return CounterCompare(condState.Address.Pack(), false);
            }

            Logger.Warning?.Print(LogClass.Gpu, $"Invalid conditional render condition \"{condState.Condition}\".");

            return ConditionalRenderEnabled.True;
        }

        /// <summary>
        /// Checks if the counter value at a given GPU memory address is non-zero.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address of the counter value</param>
        /// <returns>True if the value is not zero, false otherwise. Returns host if handling with host conditional rendering</returns>
        private ConditionalRenderEnabled CounterNonZero(ulong gpuVa)
        {
            ICounterEvent evt = _counterCache.FindEvent(gpuVa);

            if (evt == null)
            {
                return ConditionalRenderEnabled.False;
            }

            if (_context.Renderer.Pipeline.TryHostConditionalRendering(evt, 0L, false))
            {
                return ConditionalRenderEnabled.Host;
            }
            else
            {
                evt.Flush();
                return (_context.MemoryManager.Read<ulong>(gpuVa) != 0) ? ConditionalRenderEnabled.True : ConditionalRenderEnabled.False;
            }
        }

        /// <summary>
        /// Checks if the counter at a given GPU memory address passes a specified equality comparison.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address</param>
        /// <param name="isEqual">True to check if the values are equal, false to check if they are not equal</param>
        /// <returns>True if the condition is met, false otherwise. Returns host if handling with host conditional rendering</returns>
        private ConditionalRenderEnabled CounterCompare(ulong gpuVa, bool isEqual)
        {
            ICounterEvent evt = FindEvent(gpuVa);
            ICounterEvent evt2 = FindEvent(gpuVa + 16);

            bool useHost;

            if (evt != null && evt2 == null)
            {
                useHost = _context.Renderer.Pipeline.TryHostConditionalRendering(evt, _context.MemoryManager.Read<ulong>(gpuVa + 16), isEqual);
            }
            else if (evt == null && evt2 != null)
            {
                useHost = _context.Renderer.Pipeline.TryHostConditionalRendering(evt2, _context.MemoryManager.Read<ulong>(gpuVa), isEqual);
            }
            else if (evt != null && evt2 != null)
            {
                useHost = _context.Renderer.Pipeline.TryHostConditionalRendering(evt, evt2, isEqual);
            }
            else
            {
                useHost = false;
            }

            if (useHost)
            {
                return ConditionalRenderEnabled.Host;
            }
            else
            {
                evt?.Flush();
                evt2?.Flush();

                ulong x = _context.MemoryManager.Read<ulong>(gpuVa);
                ulong y = _context.MemoryManager.Read<ulong>(gpuVa + 16);

                return (isEqual ? x == y : x != y) ? ConditionalRenderEnabled.True : ConditionalRenderEnabled.False;
            }
        }

        /// <summary>
        /// Tries to find a counter that is supposed to be written at the specified address,
        /// returning the related event.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the counter is supposed to be written</param>
        /// <returns>The counter event, or null if not present</returns>
        private ICounterEvent FindEvent(ulong gpuVa)
        {
            return _counterCache.FindEvent(gpuVa);
        }
    }
}
