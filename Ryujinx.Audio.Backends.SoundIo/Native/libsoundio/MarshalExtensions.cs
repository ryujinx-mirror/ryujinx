using System;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public static class MarshalEx
    {
        public static double ReadDouble(IntPtr handle, int offset = 0)
        {
            return BitConverter.Int64BitsToDouble(Marshal.ReadInt64(handle, offset));
        }

        public static void WriteDouble(IntPtr handle, double value)
        {
            WriteDouble(handle, 0, value);
        }

        public static void WriteDouble(IntPtr handle, int offset, double value)
        {
            Marshal.WriteInt64(handle, offset, BitConverter.DoubleToInt64Bits(value));
        }

        public static float ReadFloat(IntPtr handle, int offset = 0)
        {
            return BitConverter.Int32BitsToSingle(Marshal.ReadInt32(handle, offset));
        }

        public static void WriteFloat(IntPtr handle, float value)
        {
            WriteFloat(handle, 0, value);
        }

        public static void WriteFloat(IntPtr handle, int offset, float value)
        {
            Marshal.WriteInt32(handle, offset, BitConverter.SingleToInt32Bits(value));
        }
    }
}