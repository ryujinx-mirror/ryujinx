using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Represents a pool of GPU resources, such as samplers or textures.
    /// </summary>
    /// <typeparam name="T">GPU resource type</typeparam>
    abstract class Pool<T> : IDisposable
    {
        protected const int DescriptorSize = 0x20;

        protected GpuContext Context;

        protected T[] Items;

        /// <summary>
        /// The maximum ID value of resources on the pool (inclusive).
        /// </summary>
        /// <remarks>
        /// The maximum amount of resources on the pool is equal to this value plus one.
        /// </remarks>
        public int MaximumId { get; }

        /// <summary>
        /// The address of the pool in guest memory.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// The size of the pool in bytes.
        /// </summary>
        public ulong Size { get; }

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

        /// <summary>
        /// Gets the GPU resource with the given ID.
        /// </summary>
        /// <param name="id">ID of the resource. This is effectively a zero-based index</param>
        /// <returns>The GPU resource with the given ID</returns>
        public abstract T Get(int id);

        /// <summary>
        /// Synchronizes host memory with guest memory.
        /// This causes a invalidation of pool entries,
        /// if a modification of entries by the CPU is detected.
        /// </summary>
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

        /// <summary>
        /// Invalidates a range of memory of the GPU resource pool.
        /// Entries that falls inside the speicified range will be invalidated,
        /// causing all the data to be reloaded from guest memory.
        /// </summary>
        /// <param name="address">The start address of the range to invalidate</param>
        /// <param name="size">The size of the range to invalidate</param>
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

            size = endAddress - address;

            InvalidateRangeImpl(address, size);
        }

        protected abstract void InvalidateRangeImpl(ulong address, ulong size);

        protected abstract void Delete(T item);

        /// <summary>
        /// Performs the disposal of all resources stored on the pool.
        /// It's an error to try using the pool after disposal.
        /// </summary>
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