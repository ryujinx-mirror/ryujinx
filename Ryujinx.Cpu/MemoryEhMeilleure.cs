using ARMeilleure.Signal;
using Ryujinx.Memory;
using Ryujinx.Memory.Tracking;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu
{
    class MemoryEhMeilleure : IDisposable
    {
        private delegate bool TrackingEventDelegate(ulong address, ulong size, bool write, bool precise = false);

        private readonly MemoryBlock _addressSpace;
        private readonly MemoryTracking _tracking;
        private readonly TrackingEventDelegate _trackingEvent;

        private readonly ulong _baseAddress;

        public MemoryEhMeilleure(MemoryBlock addressSpace, MemoryTracking tracking)
        {
            _addressSpace = addressSpace;
            _tracking = tracking;

            _baseAddress = (ulong)_addressSpace.Pointer;
            ulong endAddress = _baseAddress + addressSpace.Size;

            _trackingEvent = new TrackingEventDelegate(tracking.VirtualMemoryEventEh);
            bool added = NativeSignalHandler.AddTrackedRegion((nuint)_baseAddress, (nuint)endAddress, Marshal.GetFunctionPointerForDelegate(_trackingEvent));

            if (!added)
            {
                throw new InvalidOperationException("Number of allowed tracked regions exceeded.");
            }
        }

        public void Dispose()
        {
            NativeSignalHandler.RemoveTrackedRegion((nuint)_baseAddress);
        }
    }
}
