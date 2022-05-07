using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public struct VibrationValue
    {
        public float AmplitudeLow;
        public float FrequencyLow;
        public float AmplitudeHigh;
        public float FrequencyHigh;

        public override bool Equals(object obj)
        {
            return obj is VibrationValue value &&
                   AmplitudeLow == value.AmplitudeLow &&
                   AmplitudeHigh == value.AmplitudeHigh;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AmplitudeLow, AmplitudeHigh);
        }
    }
}