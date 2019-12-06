using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    abstract class Pool<T> : IDisposable
    {
        protected const int DescriptorSize = 0x20;

        protected GpuContext Context;

        protected T[] Items;

        public int MaximumId { get; }

        public ulong Address { get; }
        public ulong Size    { get; }

        public Pool(GpuContext context, ulong address, int maximumId)
        {
            Context   = context;
            MaximumId = maximumId;

            int count = maximumId + 1;

            ulong size = (ulong)(uint)count * DescriptorSize;;

            Items = new T[count];

            Address = address;
            Size    = size;
        }

        public abstract T Get(int id);

        public void SynchronizeMemory()
        {
            (ulong, ulong)[] modifiedRanges = Context.PhysicalMemory.GetModifiedRanges(Address, Size, ResourceName.TexturePool);

            for (int index = 0; index < modifiedRanges.Length; index++)
            {
                (ulong mAddress, ulong mSize) = modifiedRanges[index];

                if (mAddress < Address)
                {
                    mAddress = Address;
                }

                ulong maxSize = Address + Size - mAddress;

                if (mSize > maxSize)
                {
                    mSize = maxSize;
                }

                InvalidateRangeImpl(mAddress, mSize);
            }
        }

        public void InvalidateRange(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            ulong texturePoolEndAddress = Address + Size;

            // If the range being invalidated is not overlapping the texture pool range,
            // then we don't have anything to do, exit early.
            if (address >= texturePoolEndAddress || endAddress <= Address)
            {
                return;
            }

            if (address < Address)
            {
                address = Address;
            }

            if (endAddress > texturePoolEndAddress)
            {
                endAddress = texturePoolEndAddress;
            }

            InvalidateRangeImpl(address, size);
        }

        protected abstract void InvalidateRangeImpl(ulong address, ulong size);

        protected abstract void Delete(T item);

        public void Dispose()
        {
            if (Items != null)
            {
                for (int index = 0; index < Items.Length; index++)
                {
                    Delete(Items[index]);
                }

                Items = null;
            }
        }
    }
}