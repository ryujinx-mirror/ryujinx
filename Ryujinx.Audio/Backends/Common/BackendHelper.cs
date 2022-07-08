using Ryujinx.Audio.Common;
using System;

namespace Ryujinx.Audio.Backends.Common
{
    public static class BackendHelper
    {
        public static int GetSampleSize(SampleFormat format)
        {
            return format switch
            {
                SampleFormat.PcmInt8 => sizeof(byte),
                SampleFormat.PcmInt16 => sizeof(ushort),
                SampleFormat.PcmInt24 => 3,
                SampleFormat.PcmInt32 => sizeof(int),
                SampleFormat.PcmFloat => sizeof(float),
                _ => throw new ArgumentException($"{format}"),
            };
        }

        public static int GetSampleCount(SampleFormat format, int channelCount, int bufferSize)
        {
            return bufferSize / GetSampleSize(format) / channelCount;
        }
    }
}
