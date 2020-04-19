using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private const int NsToTicksFractionNumerator   = 384;
        private const int NsToTicksFractionDenominator = 625;

        private ulong _runningCounter;

        /// <summary>
        /// Writes a GPU counter to guest memory.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void Report(GpuState state, int argument)
        {
            ReportMode mode = (ReportMode)(argument & 3);

            ReportCounterType type = (ReportCounterType)((argument >> 23) & 0x1f);

            switch (mode)
            {
                case ReportMode.Release: ReleaseSemaphore(state);    break;
                case ReportMode.Counter: ReportCounter(state, type); break;
            }
        }

        /// <summary>
        /// Writes (or Releases) a GPU semaphore value to guest memory.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void ReleaseSemaphore(GpuState state)
        {
            var rs = state.Get<ReportState>(MethodOffset.ReportState);

            _context.MemoryAccessor.Write(rs.Address.Pack(), rs.Payload);

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
            CounterData counterData = new CounterData();

            ulong counter = 0;

            switch (type)
            {
                case ReportCounterType.Zero:
                    counter = 0;
                    break;
                case ReportCounterType.SamplesPassed:
                    counter = _context.Renderer.GetCounter(CounterType.SamplesPassed);
                    break;
                case ReportCounterType.PrimitivesGenerated:
                    counter = _context.Renderer.GetCounter(CounterType.PrimitivesGenerated);
                    break;
                case ReportCounterType.TransformFeedbackPrimitivesWritten:
                    counter = _context.Renderer.GetCounter(CounterType.TransformFeedbackPrimitivesWritten);
                    break;
            }

            ulong ticks;

            if (GraphicsConfig.FastGpuTime)
            {
                ticks = _runningCounter++;
            }
            else
            {
                ticks = ConvertNanosecondsToTicks((ulong)PerformanceCounter.ElapsedNanoseconds);
            }

            counterData.Counter   = counter;
            counterData.Timestamp = ticks;

            Span<CounterData> counterDataSpan = MemoryMarshal.CreateSpan(ref counterData, 1);

            Span<byte> data = MemoryMarshal.Cast<CounterData, byte>(counterDataSpan);

            var rs = state.Get<ReportState>(MethodOffset.ReportState);

            _context.MemoryAccessor.Write(rs.Address.Pack(), data);
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