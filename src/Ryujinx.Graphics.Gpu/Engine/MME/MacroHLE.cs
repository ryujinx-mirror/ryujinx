using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.GPFifo;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Engine.MME
{
    /// <summary>
    /// Macro High-level emulation.
    /// </summary>
    class MacroHLE : IMacroEE
    {
        private const int ColorLayerCountOffset = 0x818;
        private const int ColorStructSize = 0x40;
        private const int ZetaLayerCountOffset = 0x1230;

        private const int IndirectDataEntrySize = 0x10;
        private const int IndirectIndexedDataEntrySize = 0x14;

        private readonly GPFifoProcessor _processor;
        private readonly MacroHLEFunctionName _functionName;

        /// <summary>
        /// Arguments FIFO.
        /// </summary>
        public Queue<FifoWord> Fifo { get; }

        /// <summary>
        /// Creates a new instance of the HLE macro handler.
        /// </summary>
        /// <param name="processor">GPU GP FIFO command processor</param>
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
                case MacroHLEFunctionName.ClearColor:
                    ClearColor(state, arg0);
                    break;
                case MacroHLEFunctionName.ClearDepthStencil:
                    ClearDepthStencil(state, arg0);
                    break;
                case MacroHLEFunctionName.DrawArraysInstanced:
                    DrawArraysInstanced(state, arg0);
                    break;
                case MacroHLEFunctionName.DrawElementsInstanced:
                    DrawElementsInstanced(state, arg0);
                    break;
                case MacroHLEFunctionName.DrawElementsIndirect:
                    DrawElementsIndirect(state, arg0);
                    break;
                case MacroHLEFunctionName.MultiDrawElementsIndirectCount:
                    MultiDrawElementsIndirectCount(state, arg0);
                    break;
                default:
                    throw new NotImplementedException(_functionName.ToString());
            }

            // It should be empty at this point, but clear it just to be safe.
            Fifo.Clear();
        }

        /// <summary>
        /// Clears one bound color target.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void ClearColor(IDeviceState state, int arg0)
        {
            int index = (arg0 >> 6) & 0xf;
            int layerCount = state.Read(ColorLayerCountOffset + index * ColorStructSize);

            _processor.ThreedClass.Clear(arg0, layerCount);
        }

        /// <summary>
        /// Clears the current depth-stencil target.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void ClearDepthStencil(IDeviceState state, int arg0)
        {
            int layerCount = state.Read(ZetaLayerCountOffset);

            _processor.ThreedClass.Clear(arg0, layerCount);
        }

        /// <summary>
        /// Performs a draw.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void DrawArraysInstanced(IDeviceState state, int arg0)
        {
            var topology = (PrimitiveTopology)arg0;

            var count = FetchParam();
            var instanceCount = FetchParam();
            var firstVertex = FetchParam();
            var firstInstance = FetchParam();

            if (ShouldSkipDraw(state, instanceCount.Word))
            {
                return;
            }

            _processor.ThreedClass.Draw(
                topology,
                count.Word,
                instanceCount.Word,
                0,
                firstVertex.Word,
                firstInstance.Word,
                indexed: false);
        }

        /// <summary>
        /// Performs a indexed draw.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void DrawElementsInstanced(IDeviceState state, int arg0)
        {
            var topology = (PrimitiveTopology)arg0;

            var count = FetchParam();
            var instanceCount = FetchParam();
            var firstIndex = FetchParam();
            var firstVertex = FetchParam();
            var firstInstance = FetchParam();

            if (ShouldSkipDraw(state, instanceCount.Word))
            {
                return;
            }

            _processor.ThreedClass.Draw(
                topology,
                count.Word,
                instanceCount.Word,
                firstIndex.Word,
                firstVertex.Word,
                firstInstance.Word,
                indexed: true);
        }

        /// <summary>
        /// Performs a indirect indexed draw, with parameters from a GPU buffer.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void DrawElementsIndirect(IDeviceState state, int arg0)
        {
            var topology = (PrimitiveTopology)arg0;

            var count = FetchParam();
            var instanceCount = FetchParam();
            var firstIndex = FetchParam();
            var firstVertex = FetchParam();
            var firstInstance = FetchParam();

            ulong indirectBufferGpuVa = count.GpuVa;

            var bufferCache = _processor.MemoryManager.Physical.BufferCache;

            bool useBuffer = bufferCache.CheckModified(_processor.MemoryManager, indirectBufferGpuVa, IndirectIndexedDataEntrySize, out ulong indirectBufferAddress);

            if (useBuffer)
            {
                int indexCount = firstIndex.Word + count.Word;

                _processor.ThreedClass.DrawIndirect(
                    topology,
                    indirectBufferAddress,
                    0,
                    1,
                    IndirectIndexedDataEntrySize,
                    indexCount,
                    Threed.IndirectDrawType.DrawIndexedIndirect);
            }
            else
            {
                if (ShouldSkipDraw(state, instanceCount.Word))
                {
                    return;
                }

                _processor.ThreedClass.Draw(
                    topology,
                    count.Word,
                    instanceCount.Word,
                    firstIndex.Word,
                    firstVertex.Word,
                    firstInstance.Word,
                    indexed: true);
            }
        }

        /// <summary>
        /// Performs a indirect indexed multi-draw, with parameters from a GPU buffer.
        /// </summary>
        /// <param name="state">GPU state at the time of the call</param>
        /// <param name="arg0">First argument of the call</param>
        private void MultiDrawElementsIndirectCount(IDeviceState state, int arg0)
        {
            int arg1 = FetchParam().Word;
            int arg2 = FetchParam().Word;
            int arg3 = FetchParam().Word;

            int startDraw = arg0;
            int endDraw = arg1;
            var topology = (PrimitiveTopology)arg2;
            int paddingWords = arg3;
            int stride = paddingWords * 4 + 0x14;

            ulong parameterBufferGpuVa = FetchParam().GpuVa;

            int maxDrawCount = endDraw - startDraw;

            if (startDraw != 0)
            {
                int drawCount = _processor.MemoryManager.Read<int>(parameterBufferGpuVa, tracked: true);

                // Calculate maximum draw count based on the previous draw count and current draw count.
                if ((uint)drawCount <= (uint)startDraw)
                {
                    // The start draw is past our total draw count, so all draws were already performed.
                    maxDrawCount = 0;
                }
                else
                {
                    // Perform just the missing number of draws.
                    maxDrawCount = (int)Math.Min((uint)maxDrawCount, (uint)(drawCount - startDraw));
                }
            }

            if (maxDrawCount == 0)
            {
                Fifo.Clear();
                return;
            }

            ulong indirectBufferGpuVa = 0;
            int indexCount = 0;

            for (int i = 0; i < maxDrawCount; i++)
            {
                var count = FetchParam();
                var instanceCount = FetchParam();
                var firstIndex = FetchParam();
                var firstVertex = FetchParam();
                var firstInstance = FetchParam();

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

            var bufferCache = _processor.MemoryManager.Physical.BufferCache;

            ulong indirectBufferSize = (ulong)maxDrawCount * (ulong)stride;

            ulong indirectBufferAddress = bufferCache.TranslateAndCreateBuffer(_processor.MemoryManager, indirectBufferGpuVa, indirectBufferSize);
            ulong parameterBufferAddress = bufferCache.TranslateAndCreateBuffer(_processor.MemoryManager, parameterBufferGpuVa, 4);

            _processor.ThreedClass.DrawIndirect(
                topology,
                indirectBufferAddress,
                parameterBufferAddress,
                maxDrawCount,
                stride,
                indexCount,
                Threed.IndirectDrawType.DrawIndexedIndirectCount);
        }

        /// <summary>
        /// Checks if the draw should be skipped, because the masked instance count is zero.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="instanceCount">Draw instance count</param>
        /// <returns>True if the draw should be skipped, false otherwise</returns>
        private static bool ShouldSkipDraw(IDeviceState state, int instanceCount)
        {
            return (Read(state, 0xd1b) & instanceCount) == 0;
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
        /// Reads data from a GPU register.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="reg">Register offset to read</param>
        /// <returns>GPU register value</returns>
        private static int Read(IDeviceState state, int reg)
        {
            return state.Read(reg * 4);
        }
    }
}
