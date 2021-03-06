using Ryujinx.Common;
using Ryujinx.Cpu.Tracking;
using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Represents a pool of GPU resources, such as samplers or textures.
    /// </summary>
    /// <typeparam name="T1">Type of the GPU resource</typeparam>
    /// <typeparam name="T2">Type of the descriptor</typeparam>
    abstract class Pool<T1, T2> : IDisposable where T2 : unmanaged
    {
        protected const int DescriptorSize = 0x20;

        protected GpuContext Context;

        protected T1[] Items;
        protected T2[] DescriptorCache;

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

        private readonly CpuMultiRegionHandle _memoryTracking;
        private readonly Action<ulong, ulong> _modifiedDelegate;

        public Pool(GpuContext context, ulong address, int maximumId)
        {
            Context   = context;
            MaximumId = maximumId;

            int count = maximumId + 1;

            ulong size = (ulong)(uint)count * DescriptorSize;

            Items = new T1[count];
            DescriptorCache = new T2[count];

            Address = address;
            Size    = size;

            _memoryTracking = context.PhysicalMemory.BeginGranularTracking(address, size);
            _modifiedDelegate = RegionModified;
        }


        /// <summary>
        /// Gets the descriptor for a given ID.
        /// </summary>
        /// <param name="id">ID of the descriptor. This is effectively a zero-based index</param>
        /// <returns>The descriptor</returns>
        public T2 GetDescriptor(int id)
        {
            return Context.PhysicalMemory.Read<T2>(Address + (ulong)id * DescriptorSize);
        }

        /// <summary>
        /// Gets the GPU resource with the given ID.
        /// </summary>
        /// <param name="id">ID of the resource. This is effectively a zero-based index</param>
        /// <returns>The GPU resource with the given ID</returns>
        public abstract T1 Get(int id);

        /// <summary>
        /// Synchronizes host memory with guest memory.
        /// This causes invalidation of pool entries,
        /// if a modification of entries by the CPU is detected.
        /// </summary>
        public void SynchronizeMemory()
        {
            _memoryTracking.QueryModified(_modifiedDelegate);
        }

        /// <summary>
        /// Indicate that a region of the pool was modified, and must be loaded from memory.
        /// </summary>
        /// <param name="mAddress">Start address of the modified region</param>
        /// <param name="mSize">Size of the modified region</param>
        private void RegionModified(ulong mAddress, ulong mSize)
        {
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

        protected abstract void InvalidateRangeImpl(ulong address, ulong size);

        protected abstract void Delete(T1 item);

        /// <summary>
        /// Performs the disposal of all resources stored on the pool.
        /// It's an error to try using the pool after disposal.
        /// </summary>
        public virtual void Dispose()
        {
            if (Items != null)
            {
                for (int index = 0; index < Items.Length; index++)
                {
                    Delete(Items[index]);
                }

                Items = null;
            }
            _memoryTracking.Dispose();
        }
    }
}