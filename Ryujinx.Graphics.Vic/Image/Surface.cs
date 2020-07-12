using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Vic.Image
{
    struct Surface : IDisposable
    {
        private readonly int _bufferIndex;

        private readonly BufferPool<Pixel> _pool;

        public Pixel[] Data { get; }

        public int Width { get; }
        public int Height { get; }

        public Surface(BufferPool<Pixel> pool, int width, int height)
        {
            _bufferIndex = pool.RentMinimum(width * height, out Pixel[] data);
            _pool = pool;
            Data = data;
            Width = width;
            Height = height;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetR(int x, int y) => Data[y * Width + x].R;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetG(int x, int y) => Data[y * Width + x].G;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetB(int x, int y) => Data[y * Width + x].B;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetA(int x, int y) => Data[y * Width + x].A;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetR(int x, int y, ushort value) => Data[y * Width + x].R = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetG(int x, int y, ushort value) => Data[y * Width + x].G = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetB(int x, int y, ushort value) => Data[y * Width + x].B = value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetA(int x, int y, ushort value) => Data[y * Width + x].A = value;

        public void Dispose() => _pool.Return(_bufferIndex);
    }
}
