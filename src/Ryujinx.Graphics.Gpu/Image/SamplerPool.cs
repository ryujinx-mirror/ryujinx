using Ryujinx.Graphics.Gpu.Memory;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Sampler pool.
    /// </summary>
    class SamplerPool : Pool<Sampler, SamplerDescriptor>, IPool<SamplerPool>
    {
        private float _forcedAnisotropy;

        /// <summary>
        /// Linked list node used on the sampler pool cache.
        /// </summary>
        public LinkedListNode<SamplerPool> CacheNode { get; set; }

        /// <summary>
        /// Timestamp used by the sampler pool cache, updated on every use of this sampler pool.
        /// </summary>
        public ulong CacheTimestamp { get; set; }

        /// <summary>
        /// Creates a new instance of the sampler pool.
        /// </summary>
        /// <param name="context">GPU context that the sampler pool belongs to</param>
        /// <param name="physicalMemory">Physical memory where the sampler descriptors are mapped</param>
        /// <param name="address">Address of the sampler pool in guest memory</param>
        /// <param name="maximumId">Maximum sampler ID of the sampler pool (equal to maximum samplers minus one)</param>
        public SamplerPool(GpuContext context, PhysicalMemory physicalMemory, ulong address, int maximumId) : base(context, physicalMemory, address, maximumId)
        {
            _forcedAnisotropy = GraphicsConfig.MaxAnisotropy;
        }

        /// <summary>
        /// Gets the sampler with the given ID.
        /// </summary>
        /// <param name="id">ID of the sampler. This is effectively a zero-based index</param>
        /// <returns>The sampler with the given ID</returns>
        public override Sampler Get(int id)
        {
            if ((uint)id >= Items.Length)
            {
                return null;
            }

            if (SequenceNumber != Context.SequenceNumber)
            {
                if (_forcedAnisotropy != GraphicsConfig.MaxAnisotropy)
                {
                    _forcedAnisotropy = GraphicsConfig.MaxAnisotropy;

                    for (int i = 0; i < Items.Length; i++)
                    {
                        if (Items[i] != null)
                        {
                            Items[i].Dispose();

                            Items[i] = null;
                        }
                    }

                    UpdateModifiedSequence();
                }

                SequenceNumber = Context.SequenceNumber;

                SynchronizeMemory();
            }

            Sampler sampler = Items[id];

            if (sampler == null)
            {
                SamplerDescriptor descriptor = GetDescriptor(id);

                sampler = new Sampler(Context, descriptor);

                Items[id] = sampler;

                DescriptorCache[id] = descriptor;
            }

            return sampler;
        }

        /// <summary>
        /// Checks if the pool was modified, and returns the last sequence number where a modification was detected.
        /// </summary>
        /// <returns>A number that increments each time a modification is detected</returns>
        public int CheckModified()
        {
            if (SequenceNumber != Context.SequenceNumber)
            {
                SequenceNumber = Context.SequenceNumber;

                if (_forcedAnisotropy != GraphicsConfig.MaxAnisotropy)
                {
                    _forcedAnisotropy = GraphicsConfig.MaxAnisotropy;

                    for (int i = 0; i < Items.Length; i++)
                    {
                        if (Items[i] != null)
                        {
                            Items[i].Dispose();

                            Items[i] = null;
                        }
                    }

                    UpdateModifiedSequence();
                }

                SynchronizeMemory();
            }

            return ModifiedSequenceNumber;
        }

        /// <summary>
        /// Implementation of the sampler pool range invalidation.
        /// </summary>
        /// <param name="address">Start address of the range of the sampler pool</param>
        /// <param name="size">Size of the range being invalidated</param>
        protected override void InvalidateRangeImpl(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            for (; address < endAddress; address += DescriptorSize)
            {
                int id = (int)((address - Address) / DescriptorSize);

                Sampler sampler = Items[id];

                if (sampler != null)
                {
                    SamplerDescriptor descriptor = GetDescriptor(id);

                    // If the descriptors are the same, the sampler is still valid.
                    if (descriptor.Equals(ref DescriptorCache[id]))
                    {
                        continue;
                    }

                    sampler.Dispose();

                    Items[id] = null;
                }
            }
        }

        /// <summary>
        /// Deletes a given sampler pool entry.
        /// The host memory used by the sampler is released by the driver.
        /// </summary>
        /// <param name="item">The entry to be deleted</param>
        protected override void Delete(Sampler item)
        {
            item?.Dispose();
        }
    }
}
