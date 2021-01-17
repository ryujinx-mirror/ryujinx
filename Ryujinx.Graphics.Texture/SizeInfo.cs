using System;

namespace Ryujinx.Graphics.Texture
{
    public struct SizeInfo
    {
        private readonly int[] _mipOffsets;
        private readonly int[] _allOffsets;

        private readonly int _levels;

        public int LayerSize { get; }
        public int TotalSize { get; }

        public SizeInfo(int size)
        {
            _mipOffsets = new int[] { 0 };
            _allOffsets = new int[] { 0 };
            _levels     = 1;
            LayerSize   = size;
            TotalSize   = size;
        }

        internal SizeInfo(
            int[] mipOffsets,
            int[] allOffsets,
            int   levels,
            int   layerSize,
            int   totalSize)
        {
            _mipOffsets = mipOffsets;
            _allOffsets = allOffsets;
            _levels     = levels;
            LayerSize   = layerSize;
            TotalSize   = totalSize;
        }

        public int GetMipOffset(int level)
        {
            if ((uint)level >= _mipOffsets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            return _mipOffsets[level];
        }

        public bool FindView(int offset, out int firstLayer, out int firstLevel)
        {
            int index = Array.BinarySearch(_allOffsets, offset);

            if (index < 0)
            {
                firstLayer = 0;
                firstLevel = 0;

                return false;
            }

            firstLayer = index / _levels;
            firstLevel = index - (firstLayer * _levels);

            return true;
        }
    }
}