namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Sampler pool.
    /// </summary>
    class SamplerPool : Pool<Sampler, SamplerDescriptor>
    {
        private int _sequenceNumber;

        /// <summary>
        /// Constructs a new instance of the sampler pool.
        /// </summary>
        /// <param name="context">GPU context that the sampler pool belongs to</param>
        /// <param name="address">Address of the sampler pool in guest memory</param>
        /// <param name="maximumId">Maximum sampler ID of the sampler pool (equal to maximum samplers minus one)</param>
        public SamplerPool(GpuContext context, ulong address, int maximumId) : base(context, address, maximumId) { }

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

            if (_sequenceNumber != Context.SequenceNumber)
            {
                _sequenceNumber = Context.SequenceNumber;

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