using System;

namespace Ryujinx.Graphics.VDec
{
    static class DecoderHelper
    {
        public static byte[] Combine(byte[] arr0, byte[] arr1)
        {
            byte[] output = new byte[arr0.Length + arr1.Length];

            Buffer.BlockCopy(arr0, 0, output, 0, arr0.Length);
            Buffer.BlockCopy(arr1, 0, output, arr0.Length, arr1.Length);

            return output;
        }
    }
}