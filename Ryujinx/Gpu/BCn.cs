using System;
using System.Drawing;

namespace Ryujinx.Gpu
{
    static class BCn
    {
        public static byte[] DecodeBC1(NsGpuTexture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            SwizzleAddr Swizzle = new SwizzleAddr(W, H, 8);

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzledAddress64(X, Y) * 8;

                    byte[] Tile = BCnDecodeTile(Tex.Data, IOffs, true);

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = Tile[TOffset + 3];

                            TOffset += 4;
                        }
                    }
                }
            }

            return Output;
        }

        public static byte[] DecodeBC2(NsGpuTexture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            SwizzleAddr Swizzle = new SwizzleAddr(W, H, 4);

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzledAddress128(X, Y) * 16;

                    byte[] Tile = BCnDecodeTile(Tex.Data, IOffs + 8, false);

                    int AlphaLow  = Get32(Tex.Data, IOffs + 0);
                    int AlphaHigh = Get32(Tex.Data, IOffs + 4);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            ulong Alpha = (AlphaCh >> (TY * 16 + TX * 4)) & 0xf;

                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = (byte)(Alpha | (Alpha << 4));

                            TOffset += 4;
                        }
                    }
                }
            }

            return Output;
        }

        public static byte[] DecodeBC3(NsGpuTexture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            SwizzleAddr Swizzle = new SwizzleAddr(W, H, 4);

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Offset + Swizzle.GetSwizzledAddress128(X, Y) * 16;

                    byte[] Tile = BCnDecodeTile(Tex.Data, IOffs + 8, false);

                    byte[] Alpha = new byte[8];

                    Alpha[0] = Tex.Data[IOffs + 0];
                    Alpha[1] = Tex.Data[IOffs + 1];

                    CalculateBC3Alpha(Alpha);

                    int AlphaLow  = Get32(Tex.Data, IOffs + 2);
                    int AlphaHigh = Get16(Tex.Data, IOffs + 6);

                    ulong AlphaCh = (uint)AlphaLow | (ulong)AlphaHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            byte AlphaPx = Alpha[(AlphaCh >> (TY * 12 + TX * 3)) & 7];

                            Output[OOffset + 0] = Tile[TOffset + 0];
                            Output[OOffset + 1] = Tile[TOffset + 1];
                            Output[OOffset + 2] = Tile[TOffset + 2];
                            Output[OOffset + 3] = AlphaPx;

                            TOffset += 4;
                        }
                    }
                }
            }

            return Output;
        }

        public static byte[] DecodeBC4(NsGpuTexture Tex, int Offset)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            SwizzleAddr Swizzle = new SwizzleAddr(W, H, 8);

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Swizzle.GetSwizzledAddress64(X, Y) * 8;

                    byte[] Red = new byte[8];

                    Red[0] = Tex.Data[IOffs + 0];
                    Red[1] = Tex.Data[IOffs + 1];

                    CalculateBC3Alpha(Red);

                    int RedLow  = Get32(Tex.Data, IOffs + 2);
                    int RedHigh = Get16(Tex.Data, IOffs + 6);

                    ulong RedCh = (uint)RedLow | (ulong)RedHigh << 32;

                    int TOffset = 0;

                    for (int TY = 0; TY < 4; TY++)
                    {
                        for (int TX = 0; TX < 4; TX++)
                        {
                            int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                            byte RedPx = Red[(RedCh >> (TY * 12 + TX * 3)) & 7];

                            Output[OOffset + 0] = RedPx;
                            Output[OOffset + 1] = RedPx;
                            Output[OOffset + 2] = RedPx;
                            Output[OOffset + 3] = 0xff;

                            TOffset += 4;
                        }
                    }
                }
            }

            return Output;
        }

        public static byte[] DecodeBC5(NsGpuTexture Tex, int Offset, bool SNorm)
        {
            int W = (Tex.Width  + 3) / 4;
            int H = (Tex.Height + 3) / 4;

            byte[] Output = new byte[W * H * 64];

            SwizzleAddr Swizzle = new SwizzleAddr(W, H, 4);

            for (int Y = 0; Y < H; Y++)
            {
                for (int X = 0; X < W; X++)
                {
                    int IOffs = Swizzle.GetSwizzledAddress128(X, Y) * 16;

                    byte[] Red   = new byte[8];
                    byte[] Green = new byte[8];

                    Red[0]   = Tex.Data[IOffs + 0];
                    Red[1]   = Tex.Data[IOffs + 1];

                    Green[0] = Tex.Data[IOffs + 8];
                    Green[1] = Tex.Data[IOffs + 9];

                    if (SNorm)
                    {
                        CalculateBC3AlphaS(Red);
                        CalculateBC3AlphaS(Green);
                    }
                    else
                    {
                        CalculateBC3Alpha(Red);
                        CalculateBC3Alpha(Green);
                    }

                    int RedLow    = Get32(Tex.Data, IOffs + 2);
                    int RedHigh   = Get16(Tex.Data, IOffs + 6);

                    int GreenLow  = Get32(Tex.Data, IOffs + 10);
                    int GreenHigh = Get16(Tex.Data, IOffs + 14);

                    ulong RedCh   = (uint)RedLow   | (ulong)RedHigh   << 32;
                    ulong GreenCh = (uint)GreenLow | (ulong)GreenHigh << 32;

                    int TOffset = 0;

                    if (SNorm)
                    {
                        for (int TY = 0; TY < 4; TY++)
                        {
                            for (int TX = 0; TX < 4; TX++)
                            {
                                int Shift = TY * 12 + TX * 3;

                                int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                                byte RedPx   = Red  [(RedCh   >> Shift) & 7];
                                byte GreenPx = Green[(GreenCh >> Shift) & 7];

                                RedPx   += 0x80;
                                GreenPx += 0x80;

                                float NX = (RedPx   / 255f) * 2 - 1;
                                float NY = (GreenPx / 255f) * 2 - 1;

                                float NZ = (float)Math.Sqrt(1 - (NX * NX + NY * NY));

                                Output[OOffset + 0] = Clamp((NZ + 1) * 0.5f);
                                Output[OOffset + 1] = Clamp((NY + 1) * 0.5f);
                                Output[OOffset + 2] = Clamp((NX + 1) * 0.5f);
                                Output[OOffset + 3] = 0xff;

                                TOffset += 4;
                            }
                        }
                    }
                    else
                    {
                        for (int TY = 0; TY < 4; TY++)
                        {
                            for (int TX = 0; TX < 4; TX++)
                            {
                                int Shift = TY * 12 + TX * 3;

                                int OOffset = (X * 4 + TX + (Y * 4 + TY) * W * 4) * 4;

                                byte RedPx   = Red  [(RedCh   >> Shift) & 7];
                                byte GreenPx = Green[(GreenCh >> Shift) & 7];

                                Output[OOffset + 0] = RedPx;
                                Output[OOffset + 1] = RedPx;
                                Output[OOffset + 2] = RedPx;
                                Output[OOffset + 3] = GreenPx;

                                TOffset += 4;
                            }
                        }
                    }
                }
            }

            return Output;
        }

        private static byte Clamp(float Value)
        {
            if (Value > 1)
            {
                return 0xff;
            }
            else if (Value < 0)
            {
                return 0;
            }
            else
            {
                return (byte)(Value * 0xff);
            }
        }

        private static void CalculateBC3Alpha(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if (Alpha[0] > Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * Alpha[0] + (i - 1) * Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0xff;
                }
            }
        }

        private static void CalculateBC3AlphaS(byte[] Alpha)
        {
            for (int i = 2; i < 8; i++)
            {
                if ((sbyte)Alpha[0] > (sbyte)Alpha[1])
                {
                    Alpha[i] = (byte)(((8 - i) * (sbyte)Alpha[0] + (i - 1) * (sbyte)Alpha[1]) / 7);
                }
                else if (i < 6)
                {
                    Alpha[i] = (byte)(((6 - i) * (sbyte)Alpha[0] + (i - 1) * (sbyte)Alpha[1]) / 7);
                }
                else if (i == 6)
                {
                    Alpha[i] = 0x80;
                }
                else /* i == 7 */
                {
                    Alpha[i] = 0x7f;
                }
            }
        }

        private static byte[] BCnDecodeTile(
            byte[] Input,
            int    Offset,
            bool   IsBC1)
        {
            Color[] CLUT = new Color[4];

            int c0 = Get16(Input, Offset + 0);
            int c1 = Get16(Input, Offset + 2);

            CLUT[0] = DecodeRGB565(c0);
            CLUT[1] = DecodeRGB565(c1);
            CLUT[2] = CalculateCLUT2(CLUT[0], CLUT[1], c0, c1, IsBC1);
            CLUT[3] = CalculateCLUT3(CLUT[0], CLUT[1], c0, c1, IsBC1);

            int Indices = Get32(Input, Offset + 4);

            int IdxShift = 0;

            byte[] Output = new byte[4 * 4 * 4];

            int OOffset = 0;

            for (int TY = 0; TY < 4; TY++)
            {
                for (int TX = 0; TX < 4; TX++)
                {
                    int Idx = (Indices >> IdxShift) & 3;

                    IdxShift += 2;

                    Color Pixel = CLUT[Idx];

                    Output[OOffset + 0] = Pixel.R;
                    Output[OOffset + 1] = Pixel.G;
                    Output[OOffset + 2] = Pixel.B;
                    Output[OOffset + 3] = Pixel.A;

                    OOffset += 4;
                }
            }

            return Output;
        }

        private static Color CalculateCLUT2(Color C0, Color C1, int c0, int c1, bool IsBC1)
        {
            if (c0 > c1 || !IsBC1)
            {
                return Color.FromArgb(
                    (2 * C0.R + C1.R) / 3,
                    (2 * C0.G + C1.G) / 3,
                    (2 * C0.B + C1.B) / 3);
            }
            else
            {
                return Color.FromArgb(
                    (C0.R + C1.R) / 2,
                    (C0.G + C1.G) / 2,
                    (C0.B + C1.B) / 2);
            }
        }

        private static Color CalculateCLUT3(Color C0, Color C1, int c0, int c1, bool IsBC1)
        {
            if (c0 > c1 || !IsBC1)
            {
                return
                    Color.FromArgb(
                        (2 * C1.R + C0.R) / 3,
                        (2 * C1.G + C0.G) / 3,
                        (2 * C1.B + C0.B) / 3);
            }

            return Color.Transparent;
        }

        private static Color DecodeRGB565(int Value)
        {
            int B = ((Value >>  0) & 0x1f) << 3;
            int G = ((Value >>  5) & 0x3f) << 2;
            int R = ((Value >> 11) & 0x1f) << 3;

            return Color.FromArgb(
                R | (R >> 5),
                G | (G >> 6),
                B | (B >> 5));
        }

        private static int Get16(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8;
        }

        private static int Get32(byte[] Data, int Address)
        {
            return
                Data[Address + 0] << 0 |
                Data[Address + 1] << 8 |
                Data[Address + 2] << 16 |
                Data[Address + 3] << 24;
        }
    }
}