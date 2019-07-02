using System;
using System.Diagnostics;

namespace Ryujinx.Graphics.Texture
{
    class AstcPixel
    {
        public short R { get; set; }
        public short G { get; set; }
        public short B { get; set; }
        public short A { get; set; }

        byte[] _bitDepth = new byte[4];

        public AstcPixel(short a, short r, short g, short b)
        {
            A = a;
            R = r;
            G = g;
            B = b;

            for (int i = 0; i < 4; i++)
                _bitDepth[i] = 8;
        }

        public void ClampByte()
        {
            R = Math.Min(Math.Max(R, (short)0), (short)255);
            G = Math.Min(Math.Max(G, (short)0), (short)255);
            B = Math.Min(Math.Max(B, (short)0), (short)255);
            A = Math.Min(Math.Max(A, (short)0), (short)255);
        }

        public short GetComponent(int index)
        {
            switch(index)
            {
                case 0: return A;
                case 1: return R;
                case 2: return G;
                case 3: return B;
            }

            return 0;
        }

        public void SetComponent(int index, int value)
        {
            switch (index)
            {
                case 0:
                    A = (short)value;
                    break;
                case 1:
                    R = (short)value;
                    break;
                case 2:
                    G = (short)value;
                    break;
                case 3:
                    B = (short)value;
                    break;
            }
        }

        public void ChangeBitDepth(byte[] depth)
        {
            for (int i = 0; i< 4; i++)
            {
                int value = ChangeBitDepth(GetComponent(i), _bitDepth[i], depth[i]);

                SetComponent(i, value);
                _bitDepth[i] = depth[i];
            }
        }

        short ChangeBitDepth(short value, byte oldDepth, byte newDepth)
        {
            Debug.Assert(newDepth <= 8);
            Debug.Assert(oldDepth <= 8);

            if (oldDepth == newDepth)
            {
                // Do nothing
                return value;
            }
            else if (oldDepth == 0 && newDepth != 0)
            {
                return (short)((1 << newDepth) - 1);
            }
            else if (newDepth > oldDepth)
            {
                return (short)BitArrayStream.Replicate(value, oldDepth, newDepth);
            }
            else
            {
                // oldDepth > newDepth
                if (newDepth == 0)
                {
                    return 0xFF;
                }
                else
                {
                    byte bitsWasted = (byte)(oldDepth - newDepth);
                    short tempValue = value;

                    tempValue = (short)((tempValue + (1 << (bitsWasted - 1))) >> bitsWasted);
                    tempValue = Math.Min(Math.Max((short)0, tempValue), (short)((1 << newDepth) - 1));

                    return (byte)(tempValue);
                }
            }
        }

        public int Pack()
        {
            AstcPixel newPixel   = new AstcPixel(A, R, G, B);
            byte[] eightBitDepth = { 8, 8, 8, 8 };

            newPixel.ChangeBitDepth(eightBitDepth);

            return (byte)newPixel.A << 24 |
                   (byte)newPixel.B << 16 |
                   (byte)newPixel.G << 8  |
                   (byte)newPixel.R << 0;
        }

        // Adds more precision to the blue channel as described
        // in C.2.14
        public static AstcPixel BlueContract(int a, int r, int g, int b)
        {
            return new AstcPixel((short)(a),
                                 (short)((r + b) >> 1),
                                 (short)((g + b) >> 1),
                                 (short)(b));
        }
    }
}
