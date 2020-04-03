using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class TouchDevice : BaseDevice
    {
        public TouchDevice(Switch device, bool active) : base(device, active) { }

        public void Update(params TouchPoint[] points)
        {
            ref ShMemTouchScreen touchscreen = ref _device.Hid.SharedMemory.TouchScreen;

            int currentIndex = UpdateEntriesHeader(ref touchscreen.Header, out int previousIndex);

            if (!Active)
            {
                return;
            }

            ref TouchScreenState currentEntry = ref touchscreen.Entries[currentIndex];
            TouchScreenState previousEntry = touchscreen.Entries[previousIndex];

            currentEntry.SampleTimestamp = previousEntry.SampleTimestamp + 1;
            currentEntry.SampleTimestamp2 = previousEntry.SampleTimestamp2 + 1;

            currentEntry.NumTouches = (ulong)points.Length;

            int pointsLength = Math.Min(points.Length, currentEntry.Touches.Length);

            for (int i = 0; i < pointsLength; ++i)
            {
                TouchPoint pi = points[i];
                currentEntry.Touches[i] = new TouchScreenStateData
                {
                    SampleTimestamp = currentEntry.SampleTimestamp,
                    X = pi.X,
                    Y = pi.Y,
                    TouchIndex = (uint)i,
                    DiameterX = pi.DiameterX,
                    DiameterY = pi.DiameterY,
                    Angle = pi.Angle
                };
            }
        }
    }
}