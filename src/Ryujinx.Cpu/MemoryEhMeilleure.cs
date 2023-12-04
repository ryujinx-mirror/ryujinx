using ARMeilleure.Signal;
using Ryujinx.Memory;
using Ryujinx.Memory.Tracking;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Cpu
{
    public class MemoryEhMeilleure : IDisposable
    {
        private delegate bool TrackingEventDelegate(ulong address, ulong size, bool write);

        private readonly TrackingEventDelegate _trackingEvent;

        private readonly ulong _baseAddress;
        private readonly ulong _mirrorAddress;

        public MemoryEhMeilleure(MemoryBlock addressSpace, MemoryBlock addressSpaceMirror, MemoryTracking tracking)
        {
            _baseAddress = (ulong)addressSpace.Pointer;
            ulong endAddress = _baseAddress + addressSpace.Size;

            _trackingEvent = tracking.VirtualMemoryEvent;
            bool added = NativeSignalHandler.AddTrackedRegion((nuint)_baseAddress, (nuint)endAddress, Marshal.GetFunctionPointerForDelegate(_trackingEvent));

            if (!added)
            {
                throw new InvalidOperationException("Number of allowed tracked regions exceeded.");
            }

            if (OperatingSystem.IsWindows())
            {
                // Add a tracking event with no signal handler for the mirror on Windows.
                // The native handler has its own code to check for the partial overlap race when regions are protected by accident,
                // and when there is no signal handler present.

                _mirrorAddress = (ulong)addressSpaceMirror.Pointer;
                ulong endAddressMirror = _mirrorAddress + addressSpace.Size;

                bool addedMirror = NativeSignalHandler.AddTrackedRegion((nuint)_mirrorAddress, (nuint)endAddressMirror, IntPtr.Zero);

                if (!addedMirror)
                {
                    throw new InvalidOperationException("Number of allowed tracked regions exceeded.");
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            NativeSignalHandler.RemoveTrackedRegion((nuint)_baseAddress);

            if (_mirrorAddress != 0)
            {
                NativeSignalHandler.RemoveTrackedRegion((nuint)_mirrorAddress);
            }
        }
    }
}
