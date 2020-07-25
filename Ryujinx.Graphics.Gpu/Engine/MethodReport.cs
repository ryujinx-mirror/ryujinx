using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private const int NsToTicksFractionNumerator   = 384;
        private const int NsToTicksFractionDenominator = 625;

        private readonly CounterCache _counterCache = new CounterCache();

        /// <summary>
        /// Writes a GPU counter to guest memory.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void Report(GpuState state, int argument)
        {
            SemaphoreOperation op = (SemaphoreOperation)(argument & 3);
            ReportCounterType type = (ReportCounterType)((argument >> 23) & 0x1f);

            switch (op)
            {
                case SemaphoreOperation.Release: ReleaseSemaphore(state);    break;
                case SemaphoreOperation.Counter: ReportCounter(state, type); break;
            }
        }

        /// <summary>
        /// Writes (or Releases) a GPU semaphore value to guest memory.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void ReleaseSemaphore(GpuState state)
        {
            var rs = state.Get<SemaphoreState>(MethodOffset.ReportState);

            _context.MemoryManager.Write(rs.Address.Pack(), rs.Payload);

            _context.AdvanceSequence();
        }

        /// <summary>
        /// Packed GPU counter data (including GPU timestamp) in memory.
        /// </summary>
        private struct CounterData
        {
            public ulong Counter;
            public ulong Timestamp;
        }

        /// <summary>
        /// Writes a GPU counter to guest memory.
        /// This also writes the current timestamp value.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="type">Counter to be written to memory</param>
        private void ReportCounter(GpuState state, ReportCounterType type)
        {
            var rs = state.Get<SemaphoreState>(MethodOffset.ReportState);

            ulong gpuVa = rs.Address.Pack();

            ulong ticks = ConvertNanosecondsToTicks((ulong)PerformanceCounter.ElapsedNanoseconds);

            if (GraphicsConfig.FastGpuTime)
            {
                // Divide by some amount to report time as if operations were performed faster than they really are.
                // This can prevent some games from switching to a lower resolution because rendering is too slow.
                ticks /= 256;
            }

            ICounterEvent counter = null;

            EventHandler<ulong> resultHandler = (object evt, ulong result) =>
            {
                CounterData counterData = new CounterData();

                counterData.Counter = result;
                counterData.Timestamp = ticks;

                if (counter?.Invalid != true)
                {
                    _context.MemoryManager.Write(gpuVa, counterData);
                }
            };

            switch (type)
            {
                case ReportCounterType.Zero:
                    resultHandler(null, 0);
                    break;
                case ReportCounterType.SamplesPassed:
                    counter = _context.Renderer.ReportCounter(CounterType.SamplesPassed, resultHandler);
                    break;
                case ReportCounterType.PrimitivesGenerated:
                    counter = _context.Renderer.ReportCounter(CounterType.PrimitivesGenerated, resultHandler);
                    break;
                case ReportCounterType.TransformFeedbackPrimitivesWritten:
                    counter = _context.Renderer.ReportCounter(CounterType.TransformFeedbackPrimitivesWritten, resultHandler);
                    break;
            }

            _counterCache.AddOrUpdate(gpuVa, counter);
        }

        /// <summary>
        /// Converts a nanoseconds timestamp value to Maxwell time ticks.
        /// </summary>
        /// <remarks>
        /// The frequency is 614400000 Hz.
        /// </remarks>
        /// <param name="nanoseconds">Timestamp in nanoseconds</param>
        /// <returns>Maxwell ticks</returns>
        private static ulong ConvertNanosecondsToTicks(ulong nanoseconds)
        {
            // We need to divide first to avoid overflows.
            // We fix up the result later by calculating the difference and adding
            // that to the result.
            ulong divided = nanoseconds / NsToTicksFractionDenominator;

            ulong rounded = divided * NsToTicksFractionDenominator;

            ulong errorBias = (nanoseconds - rounded) * NsToTicksFractionNumerator / NsToTicksFractionDenominator;

            return divided * NsToTicksFractionNumerator + errorBias;
        }
    }
}