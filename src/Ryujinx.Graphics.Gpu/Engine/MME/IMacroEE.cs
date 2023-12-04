using Ryujinx.Graphics.Device;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// FIFO word.
    /// </summary>
    readonly struct FifoWord
    {
        /// <summary>
        /// GPU virtual address where the word is located in memory.
        /// </summary>
        public ulong GpuVa { get; }

        /// <summary>
        /// Word value.
        /// </summary>
        public int Word { get; }

        /// <summary>
        /// Creates a new FIFO word.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the word is located in memory</param>
        /// <param name="word">Word value</param>
        public FifoWord(ulong gpuVa, int word)
        {
            GpuVa = gpuVa;
            Word = word;
        }
    }

    /// <summary>
    /// Macro Execution Engine interface.
    /// </summary>
    interface IMacroEE
    {
        /// <summary>
        /// Arguments FIFO.
        /// </summary>
        Queue<FifoWord> Fifo { get; }

        /// <summary>
        /// Should execute the GPU Macro code being passed.
        /// </summary>
        /// <param name="code">Code to be executed</param>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument to be passed to the GPU Macro</param>
        void Execute(ReadOnlySpan<int> code, IDeviceState state, int arg0);
    }
}
