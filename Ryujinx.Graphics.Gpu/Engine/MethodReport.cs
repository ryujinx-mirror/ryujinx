using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private ulong _runningCounter;

        private void Report(GpuState state, int argument)
        {
            ReportMode mode = (ReportMode)(argument & 3);

            ReportCounterType type = (ReportCounterType)((argument >> 23) & 0x1f);

            switch (mode)
            {
                case ReportMode.Semaphore: ReportSemaphore(state);     break;
                case ReportMode.Counter:   ReportCounter(state, type); break;
            }
        }

        private void ReportSemaphore(GpuState state)
        {
            var rs = state.Get<ReportState>(MethodOffset.ReportState);

            _context.MemoryAccessor.Write(rs.Address.Pack(), rs.Payload);

            _context.AdvanceSequence();
        }

        private struct CounterData
        {
            public ulong Counter;
            public ulong Timestamp;
        }

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

        private static ulong ConvertNanosecondsToTicks(ulong nanoseconds)
        {
            // We need to divide first to avoid overflows.
            // We fix up the result later by calculating the difference and adding
            // that to the result.
            ulong divided = nanoseconds / 625;

            ulong rounded = divided * 625;

            ulong errorBias = ((nanoseconds - rounded) * 384) / 625;

            return divided * 384 + errorBias;
        }
    }
}