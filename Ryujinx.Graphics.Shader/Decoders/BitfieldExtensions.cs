namespace Ryujinx.Graphics.Shader.Decoders
{
    static class BitfieldExtensions
    {
        public static bool Extract(this int value, int lsb)
        {
            return ((value >> lsb) & 1) != 0;
        }

        public static int Extract(this int value, int lsb, int length)
        {
            return (value >> lsb) & (int)(uint.MaxValue >> (32 - length));
        }

        public static bool Extract(this long value, int lsb)
        {
            return ((int)(value >> lsb) & 1) != 0;
        }

        public static int Extract(this long value, int lsb, int length)
        {
            return (int)(value >> lsb) & (int)(uint.MaxValue >> (32 - length));
        }
    }
}