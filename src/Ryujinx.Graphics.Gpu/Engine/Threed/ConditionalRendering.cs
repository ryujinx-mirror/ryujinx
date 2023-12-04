using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Memory;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Helper methods used for conditional rendering.
    /// </summary>
    static class ConditionalRendering
    {
        /// <summary>
        /// Checks if draws and clears should be performed, according
        /// to currently set conditional rendering conditions.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="memoryManager">Memory manager bound to the channel currently executing</param>
        /// <param name="address">Conditional rendering buffer address</param>
        /// <param name="condition">Conditional rendering condition</param>
        /// <returns>True if rendering is enabled, false otherwise</returns>
        public static ConditionalRenderEnabled GetRenderEnable(GpuContext context, MemoryManager memoryManager, GpuVa address, Condition condition)
        {
            switch (condition)
            {
                case Condition.Always:
                    return ConditionalRenderEnabled.True;
                case Condition.Never:
                    return ConditionalRenderEnabled.False;
                case Condition.ResultNonZero:
                    return CounterNonZero(context, memoryManager, address.Pack());
                case Condition.Equal:
                    return CounterCompare(context, memoryManager, address.Pack(), true);
                case Condition.NotEqual:
                    return CounterCompare(context, memoryManager, address.Pack(), false);
            }

            Logger.Warning?.Print(LogClass.Gpu, $"Invalid conditional render condition \"{condition}\".");

            return ConditionalRenderEnabled.True;
        }

        /// <summary>
        /// Checks if the counter value at a given GPU memory address is non-zero.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="memoryManager">Memory manager bound to the channel currently executing</param>
        /// <param name="gpuVa">GPU virtual address of the counter value</param>
        /// <returns>True if the value is not zero, false otherwise. Returns host if handling with host conditional rendering</returns>
        private static ConditionalRenderEnabled CounterNonZero(GpuContext context, MemoryManager memoryManager, ulong gpuVa)
        {
            ICounterEvent evt = memoryManager.CounterCache.FindEvent(gpuVa);

            if (evt == null)
            {
                return ConditionalRenderEnabled.False;
            }

            if (context.Renderer.Pipeline.TryHostConditionalRendering(evt, 0L, false))
            {
                return ConditionalRenderEnabled.Host;
            }
            else
            {
                evt.Flush();
                return (memoryManager.Read<ulong>(gpuVa, true) != 0) ? ConditionalRenderEnabled.True : ConditionalRenderEnabled.False;
            }
        }

        /// <summary>
        /// Checks if the counter at a given GPU memory address passes a specified equality comparison.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="memoryManager">Memory manager bound to the channel currently executing</param>
        /// <param name="gpuVa">GPU virtual address</param>
        /// <param name="isEqual">True to check if the values are equal, false to check if they are not equal</param>
        /// <returns>True if the condition is met, false otherwise. Returns host if handling with host conditional rendering</returns>
        private static ConditionalRenderEnabled CounterCompare(GpuContext context, MemoryManager memoryManager, ulong gpuVa, bool isEqual)
        {
            ICounterEvent evt = FindEvent(memoryManager.CounterCache, gpuVa);
            ICounterEvent evt2 = FindEvent(memoryManager.CounterCache, gpuVa + 16);

            bool useHost;

            if (evt != null && evt2 == null)
            {
                useHost = context.Renderer.Pipeline.TryHostConditionalRendering(evt, memoryManager.Read<ulong>(gpuVa + 16), isEqual);
            }
            else if (evt == null && evt2 != null)
            {
                useHost = context.Renderer.Pipeline.TryHostConditionalRendering(evt2, memoryManager.Read<ulong>(gpuVa), isEqual);
            }
            else if (evt != null && evt2 != null)
            {
                useHost = context.Renderer.Pipeline.TryHostConditionalRendering(evt, evt2, isEqual);
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

                ulong x = memoryManager.Read<ulong>(gpuVa, true);
                ulong y = memoryManager.Read<ulong>(gpuVa + 16, true);

                return (isEqual ? x == y : x != y) ? ConditionalRenderEnabled.True : ConditionalRenderEnabled.False;
            }
        }

        /// <summary>
        /// Tries to find a counter that is supposed to be written at the specified address,
        /// returning the related event.
        /// </summary>
        /// <param name="counterCache">GPU counter cache to search on</param>
        /// <param name="gpuVa">GPU virtual address where the counter is supposed to be written</param>
        /// <returns>The counter event, or null if not present</returns>
        private static ICounterEvent FindEvent(CounterCache counterCache, ulong gpuVa)
        {
            return counterCache.FindEvent(gpuVa);
        }
    }
}
