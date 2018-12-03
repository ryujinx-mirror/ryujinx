using System;

namespace Ryujinx.Graphics.VDec
{
    static class DecoderHelper
    {
        public static byte[] Combine(byte[] Arr0, byte[] Arr1)
        {
            byte[] Output = new byte[Arr0.Length + Arr1.Length];

            Buffer.BlockCopy(Arr0, 0, Output, 0, Arr0.Length);
            Buffer.BlockCopy(Arr1, 0, Output, Arr0.Length, Arr1.Length);

            return Output;
        }
    }
}