using System;

namespace Ryujinx.Graphics.Gal.Texture
{
    class SwizzleAddr
    {
        private int Width;

        private int XB;
        private int YB;

        public SwizzleAddr(int Width, int Height, int Pad)
        {
            int W = Pow2RoundUp(Width);
            int H = Pow2RoundUp(Height);

            XB = CountZeros(W);
            YB = CountZeros(H);

            int HH = H >> 1;

            if (!IsPow2(Height) && Height <= HH + HH / 3 && YB > 3)
            {
                YB--;
            }

            this.Width = RoundSize(Width, Pad);
        }

        private static int Pow2RoundUp(int Value)
        {
            Value--;

            Value |= (Value >>  1);
            Value |= (Value >>  2);
            Value |= (Value >>  4);
            Value |= (Value >>  8);
            Value |= (Value >> 16);

            return ++Value;
        }

        private static bool IsPow2(int Value)
        {
            return Value != 0 && (Value & (Value - 1)) == 0;
        }

        private static int CountZeros(int Value)
        {
            int Count = 0;

            for (int i = 0; i < 32; i++)
            {
                if ((Value & (1 << i)) != 0)
                {
                    break;
                }

                Count++;
            }

            return Count;
        }

        private static int RoundSize(int Size, int Pad)
        {
            int Mask = Pad - 1;

            if ((Size & Mask) != 0)
            {
                Size &= ~Mask;
                Size +=  Pad;
            }

            return Size;
        }

        public int GetSwizzledAddress8(int X, int Y)
        {
            return GetSwizzledAddress(X, Y, 4);
        }

        public int GetSwizzledAddress16(int X, int Y)
        {
            return GetSwizzledAddress(X, Y, 3);
        }

        public int GetSwizzledAddress32(int X, int Y)
        {
            return GetSwizzledAddress(X, Y, 2);
        }

        public int GetSwizzledAddress64(int X, int Y)
        {
            return GetSwizzledAddress(X, Y, 1);
        }

        public int GetSwizzledAddress128(int X, int Y)
        {
            return GetSwizzledAddress(X, Y, 0);
        }

        private int GetSwizzledAddress(int X, int Y, int XBase)
        {
            /*
             * Examples of patterns:
             *                     x x y x y y x y 0 0 0 0 64   x 64   dxt5
             *         x x x x x y y y y x y y x y 0 0 0 0 512  x 512  dxt5
             *     y x x x x x x y y y y x y y x y 0 0 0 0 1024 x 1024 dxt5
             *   y y x x x x x x y y y y x y y x y x 0 0 0 2048 x 2048 dxt1
             * y y y x x x x x x y y y y x y y x y x x 0 0 1024 x 1024 rgba8888
             *
             * Read from right to left, LSB first.
             */
            int XCnt    = XBase;
            int YCnt    = 1;
            int XUsed   = 0;
            int YUsed   = 0;
            int Address = 0;

            while (XUsed < XBase + 2 && XUsed + XCnt < XB)
            {
                int XMask = (1 << XCnt) - 1;
                int YMask = (1 << YCnt) - 1;

                Address |= (X & XMask) << XUsed + YUsed;
                Address |= (Y & YMask) << XUsed + YUsed + XCnt;

                X >>= XCnt;
                Y >>= YCnt;

                XUsed += XCnt;
                YUsed += YCnt;

                XCnt = Math.Min(XB - XUsed, 1);
                YCnt = Math.Min(YB - YUsed, YCnt << 1);
            }

            Address |= (X + Y * (Width >> XUsed)) << (XUsed + YUsed);

            return Address;
        }
    }
}
