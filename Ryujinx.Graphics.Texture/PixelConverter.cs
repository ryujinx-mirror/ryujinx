using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Graphics.Texture
{
    public static class PixelConverter
    {
        public unsafe static byte[] ConvertR4G4ToR4G4B4A4(ReadOnlySpan<byte> data)
        {
            byte[] output = new byte[data.Length * 2];
            int start = 0;

            if (Sse41.IsSupported)
            {
                int sizeTrunc = data.Length & ~7;
                start = sizeTrunc;

                fixed (byte* inputPtr = data, outputPtr = output)
                {
                    for (ulong offset = 0; offset < (ulong)sizeTrunc; offset += 8)
                    {
                        Sse2.Store(outputPtr + offset * 2, Sse41.ConvertToVector128Int16(inputPtr + offset).AsByte());
                    }
                }
            }

            Span<ushort> outputSpan = MemoryMarshal.Cast<byte, ushort>(output);

            for (int i = start; i < data.Length; i++)
            {
                outputSpan[i] = (ushort)data[i];
            }

            return output;
        }
    }
}
