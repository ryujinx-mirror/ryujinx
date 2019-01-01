using System;

namespace Ryujinx.Graphics
{
    static class QuadHelper
    {
        public static int ConvertSizeQuadsToTris(int Size)
        {
            return Size <= 0 ? 0 : (Size / 4) * 6;
        }

        public static int ConvertSizeQuadStripToTris(int Size)
        {
            return Size <= 1 ? 0 : ((Size - 2) / 2) * 6;
        }

        public static byte[] ConvertQuadsToTris(byte[] Data, int EntrySize, int Count)
        {
            int PrimitivesCount = Count / 4;

            int QuadPrimSize = 4 * EntrySize;
            int TrisPrimSize = 6 * EntrySize;

            byte[] Output = new byte[PrimitivesCount * 6 * EntrySize];

            for (int Prim = 0; Prim < PrimitivesCount; Prim++)
            {
                void AssignIndex(int Src, int Dst, int CopyCount = 1)
                {
                    Src = Prim * QuadPrimSize + Src * EntrySize;
                    Dst = Prim * TrisPrimSize + Dst * EntrySize;

                    Buffer.BlockCopy(Data, Src, Output, Dst, CopyCount * EntrySize);
                }

                //0 1 2 -> 0 1 2.
                AssignIndex(0, 0, 3);

                //2 3 -> 3 4.
                AssignIndex(2, 3, 2);

                //0 -> 5.
                AssignIndex(0, 5);
            }

            return Output;
        }

        public static byte[] ConvertQuadStripToTris(byte[] Data, int EntrySize, int Count)
        {
            int PrimitivesCount = (Count - 2) / 2;

            int QuadPrimSize = 2 * EntrySize;
            int TrisPrimSize = 6 * EntrySize;

            byte[] Output = new byte[PrimitivesCount * 6 * EntrySize];

            for (int Prim = 0; Prim < PrimitivesCount; Prim++)
            {
                void AssignIndex(int Src, int Dst, int CopyCount = 1)
                {
                    Src = Prim * QuadPrimSize + Src * EntrySize + 2 * EntrySize;
                    Dst = Prim * TrisPrimSize + Dst * EntrySize;

                    Buffer.BlockCopy(Data, Src, Output, Dst, CopyCount * EntrySize);
                }

                //-2 -1 0 -> 0 1 2.
                AssignIndex(-2, 0, 3);

                //0 1 -> 3 4.
                AssignIndex(0, 3, 2);

                //-2 -> 5.
                AssignIndex(-2, 5);
            }

            return Output;
        }
    }
}