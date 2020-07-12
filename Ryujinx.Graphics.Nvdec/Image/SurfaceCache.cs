using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Video;
using System;
using System.Diagnostics;

namespace Ryujinx.Graphics.Nvdec.Image
{
    class SurfaceCache
    {
        // Must be equal to at least the maximum number of surfaces
        // that can be in use simultaneously (which is 17, since H264
        // can have up to 16 reference frames, and we need another one
        // for the current frame).
        // Realistically, most codecs won't ever use more than 4 simultaneously.
        private const int MaxItems = 17;

        private struct CacheItem
        {
            public int ReferenceCount;
            public uint LumaOffset;
            public uint ChromaOffset;
            public int Width;
            public int Height;
            public CodecId CodecId;
            public ISurface Surface;
        }

        private readonly CacheItem[] _pool = new CacheItem[MaxItems];

        private readonly MemoryManager _gmm;

        public SurfaceCache(MemoryManager gmm)
        {
            _gmm = gmm;
        }

        public ISurface Get(IDecoder decoder, CodecId codecId, uint lumaOffset, uint chromaOffset, int width, int height)
        {
            ISurface surface = null;

            // Try to find a compatible surface with same parameters, and same offsets.
            for (int i = 0; i < MaxItems; i++)
            {
                ref CacheItem item = ref _pool[i];

                if (item.LumaOffset == lumaOffset &&
                    item.ChromaOffset == chromaOffset &&
                    item.CodecId == codecId &&
                    item.Width == width &&
                    item.Height == height)
                {
                    item.ReferenceCount++;
                    surface = item.Surface;
                    MoveToFront(i);
                    break;
                }
            }

            // If we failed to find a perfect match, now ignore the offsets.
            // Search backwards to replace the oldest compatible surface,
            // this avoids thrashing frquently used surfaces.
            // Now we need to ensure that the surface is not in use, as we'll change the data.
            if (surface == null)
            {
                for (int i = MaxItems - 1; i >= 0; i--)
                {
                    ref CacheItem item = ref _pool[i];

                    if (item.ReferenceCount == 0 && item.CodecId == codecId && item.Width == width && item.Height == height)
                    {
                        item.ReferenceCount = 1;
                        item.LumaOffset = lumaOffset;
                        item.ChromaOffset = chromaOffset;
                        surface = item.Surface;

                        if ((lumaOffset | chromaOffset) != 0)
                        {
                            SurfaceReader.Read(_gmm, surface, lumaOffset, chromaOffset);
                        }

                        MoveToFront(i);
                        break;
                    }
                }
            }

            // If everything else failed, we try to create a new surface,
            // and insert it on the pool. We replace the oldest item on the
            // pool to avoid thrashing frequently used surfaces.
            // If even the oldest item is in use, that means that the entire pool
            // is in use, in that case we throw as there's no place to insert
            // the new surface.
            if (surface == null)
            {
                if (_pool[MaxItems - 1].ReferenceCount == 0)
                {
                    surface = decoder.CreateSurface(width, height);

                    if ((lumaOffset | chromaOffset) != 0)
                    {
                        SurfaceReader.Read(_gmm, surface, lumaOffset, chromaOffset);
                    }

                    MoveToFront(MaxItems - 1);
                    ref CacheItem item = ref _pool[0];
                    item.Surface?.Dispose();
                    item.ReferenceCount = 1;
                    item.LumaOffset = lumaOffset;
                    item.ChromaOffset = chromaOffset;
                    item.Width = width;
                    item.Height = height;
                    item.CodecId = codecId;
                    item.Surface = surface;
                }
                else
                {
                    throw new InvalidOperationException("No free slot on the surface pool.");
                }
            }

            return surface;
        }

        public void Put(ISurface surface)
        {
            for (int i = 0; i < MaxItems; i++)
            {
                ref CacheItem item = ref _pool[i];

                if (item.Surface == surface)
                {
                    item.ReferenceCount--;
                    Debug.Assert(item.ReferenceCount >= 0);
                    break;
                }
            }
        }

        private void MoveToFront(int index)
        {
            // If index is 0 we don't need to do anything,
            // as it's already on the front.
            if (index != 0)
            {
                CacheItem temp = _pool[index];
                Array.Copy(_pool, 0, _pool, 1, index);
                _pool[0] = temp;
            }
        }
    }
}
