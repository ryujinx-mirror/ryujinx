using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.Services.Time.Clock.Types;
using Ryujinx.HLE.HOS.Services.Time.Types;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class TimeSharedMemory
    {
        private Switch _device;
        private KSharedMemory _sharedMemory;
        private SharedMemoryStorage _timeSharedMemoryStorage;
#pragma warning disable IDE0052 // Remove unread private member
        private int _timeSharedMemorySize;
#pragma warning restore IDE0052

        private const uint SteadyClockContextOffset = 0x00;
        private const uint LocalSystemClockContextOffset = 0x38;
        private const uint NetworkSystemClockContextOffset = 0x80;
        private const uint AutomaticCorrectionEnabledOffset = 0xC8;
        private const uint ContinuousAdjustmentTimePointOffset = 0xD0;

        public void Initialize(Switch device, KSharedMemory sharedMemory, SharedMemoryStorage timeSharedMemoryStorage, int timeSharedMemorySize)
        {
            _device = device;
            _sharedMemory = sharedMemory;
            _timeSharedMemoryStorage = timeSharedMemoryStorage;
            _timeSharedMemorySize = timeSharedMemorySize;

            // Clean the shared memory
            timeSharedMemoryStorage.ZeroFill();
        }

        public KSharedMemory GetSharedMemory()
        {
            return _sharedMemory;
        }

        public void SetupStandardSteadyClock(ITickSource tickSource, UInt128 clockSourceId, TimeSpanType currentTimePoint)
        {
            UpdateSteadyClock(tickSource, clockSourceId, currentTimePoint);
        }

        public void SetAutomaticCorrectionEnabled(bool isAutomaticCorrectionEnabled)
        {
            // We convert the bool to byte here as a bool in C# takes 4 bytes...
            WriteObjectToSharedMemory(AutomaticCorrectionEnabledOffset, 0, Convert.ToByte(isAutomaticCorrectionEnabled));
        }

        public void SetSteadyClockRawTimePoint(ITickSource tickSource, TimeSpanType currentTimePoint)
        {
            SteadyClockContext context = ReadObjectFromSharedMemory<SteadyClockContext>(SteadyClockContextOffset, 4);

            UpdateSteadyClock(tickSource, context.ClockSourceId, currentTimePoint);
        }

        private void UpdateSteadyClock(ITickSource tickSource, UInt128 clockSourceId, TimeSpanType currentTimePoint)
        {
            TimeSpanType ticksTimeSpan = TimeSpanType.FromTicks(tickSource.Counter, tickSource.Frequency);

            ContinuousAdjustmentTimePoint adjustmentTimePoint = new()
            {
                ClockOffset = (ulong)ticksTimeSpan.NanoSeconds,
                Multiplier = 1,
                DivisorLog2 = 0,
                Context = new SystemClockContext
                {
                    Offset = 0,
                    SteadyTimePoint = new SteadyClockTimePoint
                    {
                        ClockSourceId = clockSourceId,
                        TimePoint = 0,
                    },
                },
            };

            WriteObjectToSharedMemory(ContinuousAdjustmentTimePointOffset, 4, adjustmentTimePoint);

            SteadyClockContext context = new()
            {
                InternalOffset = (ulong)(currentTimePoint.NanoSeconds - ticksTimeSpan.NanoSeconds),
                ClockSourceId = clockSourceId,
            };

            WriteObjectToSharedMemory(SteadyClockContextOffset, 4, context);
        }

        public void UpdateLocalSystemClockContext(SystemClockContext context)
        {
            WriteObjectToSharedMemory(LocalSystemClockContextOffset, 4, context);
        }

        public void UpdateNetworkSystemClockContext(SystemClockContext context)
        {
            WriteObjectToSharedMemory(NetworkSystemClockContextOffset, 4, context);
        }

        private T ReadObjectFromSharedMemory<T>(ulong offset, ulong padding) where T : unmanaged
        {
            T result;
            uint index;
            uint possiblyNewIndex;

            do
            {
                index = _timeSharedMemoryStorage.GetRef<uint>(offset);

                ulong objectOffset = offset + 4 + padding + (ulong)((index & 1) * Unsafe.SizeOf<T>());

                result = _timeSharedMemoryStorage.GetRef<T>(objectOffset);

                Thread.MemoryBarrier();

                possiblyNewIndex = _device.Memory.Read<uint>(offset);
            } while (index != possiblyNewIndex);

            return result;
        }

        private void WriteObjectToSharedMemory<T>(ulong offset, ulong padding, T value) where T : unmanaged
        {
            uint newIndex = _timeSharedMemoryStorage.GetRef<uint>(offset) + 1;

            ulong objectOffset = offset + 4 + padding + (ulong)((newIndex & 1) * Unsafe.SizeOf<T>());

            _timeSharedMemoryStorage.GetRef<T>(objectOffset) = value;

            Thread.MemoryBarrier();

            _timeSharedMemoryStorage.GetRef<uint>(offset) = newIndex;
        }
    }
}
