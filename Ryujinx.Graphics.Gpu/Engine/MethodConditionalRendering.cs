using Ryujinx.Common.Logging;
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
        private bool GetRenderEnable(GpuState state)
        {
            ConditionState condState = state.Get<ConditionState>(MethodOffset.ConditionState);

            switch (condState.Condition)
            {
                case Condition.Always:
                    return true;
                case Condition.Never:
                    return false;
                case Condition.ResultNonZero:
                    return CounterNonZero(condState.Address.Pack());
                case Condition.Equal:
                    return CounterCompare(condState.Address.Pack(), true);
                case Condition.NotEqual:
                    return CounterCompare(condState.Address.Pack(), false);
            }

            Logger.PrintWarning(LogClass.Gpu, $"Invalid conditional render condition \"{condState.Condition}\".");

            return true;
        }

        /// <summary>
        /// Checks if the counter value at a given GPU memory address is non-zero.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address of the counter value</param>
        /// <returns>True if the value is not zero, false otherwise</returns>
        private bool CounterNonZero(ulong gpuVa)
        {
            if (!FindAndFlush(gpuVa))
            {
                return false;
            }

            return _context.MemoryAccessor.ReadUInt64(gpuVa) != 0;
        }

        /// <summary>
        /// Checks if the counter at a given GPU memory address passes a specified equality comparison.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address</param>
        /// <param name="isEqual">True to check if the values are equal, false to check if they are not equal</param>
        /// <returns>True if the condition is met, false otherwise</returns>
        private bool CounterCompare(ulong gpuVa, bool isEqual)
        {
            if (!FindAndFlush(gpuVa) && !FindAndFlush(gpuVa + 16))
            {
                return false;
            }

            ulong x = _context.MemoryAccessor.ReadUInt64(gpuVa);
            ulong y = _context.MemoryAccessor.ReadUInt64(gpuVa + 16);

            return isEqual ? x == y : x != y;
        }

        /// <summary>
        /// Tries to find a counter that is supposed to be written at the specified address,
        /// flushing if necessary.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the counter is supposed to be written</param>
        /// <returns>True if a counter value is found at the specified address, false otherwise</returns>
        private bool FindAndFlush(ulong gpuVa)
        {
            return _counterCache.Contains(gpuVa);
        }
    }
}
