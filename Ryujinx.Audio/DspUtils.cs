namespace Ryujinx.Audio.Adpcm
{
    public static class DspUtils
    {
        public static short Saturate(int Value)
        {
            if (Value > short.MaxValue)
                Value = short.MaxValue;

            if (Value < short.MinValue)
                Value = short.MinValue;

            return (short)Value;
        }
    }
}