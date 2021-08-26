using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.GPFifo;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Macro High-level emulation.
    /// </summary>
    class MacroHLE : IMacroEE
    {
        private readonly GPFifoProcessor _processor;
        private readonly MacroHLEFunctionName _functionName;

        /// <summary>
        /// Arguments FIFO.
        /// </summary>
        public Queue<FifoWord> Fifo { get; }

        /// <summary>
        /// Creates a new instance of the HLE macro handler.
        /// </summary>
        /// <param name="context">GPU context the macro is being executed on</param>
        /// <param name="memoryManager">GPU memory manager</param>
        /// <param name="engine">3D engine where this macro is being called</param>
        /// <param name="functionName">Name of the HLE macro function to be called</param>
        public MacroHLE(GPFifoProcessor processor, MacroHLEFunctionName functionName)
        {
            _processor = processor;
            _functionName = functionName;

            Fifo = new Queue<FifoWord>();
        }

        /// <summary>
        /// Executes a macro program until it exits.
        /// </summary>
        /// <param name="code">Code of the program to execute</param>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">Optional argument passed to the program, 0 if not used</param>
        public void Execute(ReadOnlySpan<int> code, IDeviceState state, int arg0)
        {
            switch (_functionName)
            {
                case MacroHLEFunctionName.MultiDrawElementsIndirectCount:
                    MultiDrawElementsIndirectCount(state, arg0);
                    break;
                default:
                    throw new NotImplementedException(_functionName.ToString());
            }
        }

        /// <summary>
        /// Performs a indirect multi-draw, with parameters from a GPU buffer.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void MultiDrawElementsIndirectCount(IDeviceState state, int arg0)
        {
            int arg1 = FetchParam().Word;
            int arg2 = FetchParam().Word;
            int arg3 = FetchParam().Word;

            int startOffset = arg0;
            int endOffset = arg1;
            var topology = (PrimitiveTopology)arg2;
            int paddingWords = arg3;
            int maxDrawCount = endOffset - startOffset;
            int stride = paddingWords * 4 + 0x14;
            int indirectBufferSize = maxDrawCount * stride;

            ulong parameterBufferGpuVa = FetchParam().GpuVa;
            ulong indirectBufferGpuVa = 0;

            int indexCount = 0;

            for (int i = 0; i < maxDrawCount; i++)
            {
                var count = FetchParam();
                var instanceCount = FetchParam();
                var firstIndex = FetchParam();
                var baseVertex = FetchParam();
                var baseInstance = FetchParam();

                if (i == 0)
                {
                    indirectBufferGpuVa = count.GpuVa;
                }

                indexCount = Math.Max(indexCount, count.Word + firstIndex.Word);

                if (i != maxDrawCount - 1)
                {
                    for (int j = 0; j < paddingWords; j++)
                    {
                        FetchParam();
                    }
                }
            }

            // It should be empty at this point, but clear it just to be safe.
            Fifo.Clear();

            var parameterBuffer = _processor.MemoryManager.Physical.BufferCache.GetGpuBufferRange(_processor.MemoryManager, parameterBufferGpuVa, 4);
            var indirectBuffer = _processor.MemoryManager.Physical.BufferCache.GetGpuBufferRange(_processor.MemoryManager, indirectBufferGpuVa, (ulong)indirectBufferSize);

            _processor.ThreedClass.MultiDrawIndirectCount(indexCount, topology, indirectBuffer, parameterBuffer, maxDrawCount, stride);
        }

        /// <summary>
        /// Fetches a arguments from the arguments FIFO.
        /// </summary>
        /// <returns>The call argument, or a 0 value with null address if the FIFO is empty</returns>
        private FifoWord FetchParam()
        {
            if (!Fifo.TryDequeue(out var value))
            {
                Logger.Warning?.Print(LogClass.Gpu, "Macro attempted to fetch an inexistent argument.");

                return new FifoWord(0UL, 0);
            }

            return value;
        }

        /// <summary>
        /// Performs a GPU method call.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="methAddr">Address, in words, of the method</param>
        /// <param name="value">Call argument</param>
        private static void Send(IDeviceState state, int methAddr, int value)
        {
            state.Write(methAddr * 4, value);
        }
    }
}
