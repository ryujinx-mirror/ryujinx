using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp
{
    public static class FixedPointHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(long value, int qBits)
        {
            return (int)(value >> qBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToFloat(long value, int qBits)
        {
            return (float)value / (1 << qBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertFloat(float value, int qBits)
        {
            return value / (1 << qBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToFixed(float value, int qBits)
        {
            return (int)(value * (1 << qBits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundUpAndToInt(long value, int qBits)
        {
            int half = 1 << (qBits - 1);

            return ToInt(value + half, qBits);
        }
    }
}
