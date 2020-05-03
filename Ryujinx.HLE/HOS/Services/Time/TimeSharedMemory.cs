using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.Services.Time.Types;
using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class TimeSharedMemory
    {
        private Switch        _device;
        private KSharedMemory _sharedMemory;
        private ulong         _timeSharedMemoryAddress;
        private int           _timeSharedMemorySize;

        private const uint SteadyClockContextOffset         = 0x00;
        private const uint LocalSystemClockContextOffset    = 0x38;
        private const uint NetworkSystemClockContextOffset  = 0x80;
        private const uint AutomaticCorrectionEnabledOffset = 0xC8;

        public void Initialize(Switch device, KSharedMemory sharedMemory, ulong timeSharedMemoryAddress, int timeSharedMemorySize)
        {
            _device                  = device;
            _sharedMemory            = sharedMemory;
            _timeSharedMemoryAddress = timeSharedMemoryAddress;
            _timeSharedMemorySize    = timeSharedMemorySize;

            // Clean the shared memory
            _device.Memory.ZeroFill(_timeSharedMemoryAddress, (ulong)_timeSharedMemorySize);
        }

        public KSharedMemory GetSharedMemory()
        {
            return _sharedMemory;
        }

        public void SetupStandardSteadyClock(KThread thread, UInt128 clockSourceId, TimeSpanType currentTimePoint)
        {
            TimeSpanType ticksTimeSpan;

            // As this may be called before the guest code, we support passing a null thread to make this api usable.
            if (thread == null)
            {
                ticksTimeSpan = TimeSpanType.FromSeconds(0);
            }
            else
            {
                ticksTimeSpan = TimeSpanType.FromTicks(thread.Context.CntpctEl0, thread.Context.CntfrqEl0);
            }

            SteadyClockContext context = new SteadyClockContext
            {
                InternalOffset = (ulong)(currentTimePoint.NanoSeconds - ticksTimeSpan.NanoSeconds),
                ClockSourceId  = clockSourceId
            };

            WriteObjectToSharedMemory(SteadyClockContextOffset, 4, context);
        }

        public void SetAutomaticCorrectionEnabled(bool isAutomaticCorrectionEnabled)
        {
            // We convert the bool to byte here as a bool in C# takes 4 bytes...
            WriteObjectToSharedMemory(AutomaticCorrectionEnabledOffset, 0, Convert.ToByte(isAutomaticCorrectionEnabled));
        }

        public void SetSteadyClockRawTimePoint(KThread thread, TimeSpanType currentTimePoint)
        {
            SteadyClockContext context       = ReadObjectFromSharedMemory<SteadyClockContext>(SteadyClockContextOffset, 4);
            TimeSpanType       ticksTimeSpan = TimeSpanType.FromTicks(thread.Context.CntpctEl0, thread.Context.CntfrqEl0);

            context.InternalOffset = (ulong)(currentTimePoint.NanoSeconds - ticksTimeSpan.NanoSeconds);

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
            ulong indexOffset = _timeSharedMemoryAddress + offset;

            T    result;
            uint index;
            uint possiblyNewIndex;

            do
            {
                index = _device.Memory.Read<uint>(indexOffset);

                ulong objectOffset = indexOffset + 4 + padding + (ulong)((index & 1) * Unsafe.SizeOf<T>());

                result = _device.Memory.Read<T>(objectOffset);

                Thread.MemoryBarrier();

                possiblyNewIndex = _device.Memory.Read<uint>(indexOffset);
            } while (index != possiblyNewIndex);

            return result;
        }

        private void WriteObjectToSharedMemory<T>(ulong offset, ulong padding, T value) where T : unmanaged
        {
            ulong indexOffset  = _timeSharedMemoryAddress + offset;
            uint  newIndex     = _device.Memory.Read<uint>(indexOffset) + 1;
            ulong objectOffset = indexOffset + 4 + padding + (ulong)((newIndex & 1) * Unsafe.SizeOf<T>());

            _device.Memory.Write(objectOffset, value);

            Thread.MemoryBarrier();

            _device.Memory.Write(indexOffset, newIndex);
        }
    }
}
