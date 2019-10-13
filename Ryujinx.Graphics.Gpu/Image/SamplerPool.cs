using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Image
{
    class SamplerPool : Pool<Sampler>
    {
        public SamplerPool(GpuContext context, ulong address, int maximumId) : base(context, address, maximumId) { }

        public override Sampler Get(int id)
        {
            if ((uint)id >= Items.Length)
            {
                return null;
            }

            SynchronizeMemory();

            Sampler sampler = Items[id];

            if (sampler == null)
            {
                ulong address = Address + (ulong)(uint)id * DescriptorSize;

                Span<byte> data = Context.PhysicalMemory.Read(address, DescriptorSize);

                SamplerDescriptor descriptor = MemoryMarshal.Cast<byte, SamplerDescriptor>(data)[0];

                sampler = new Sampler(Context, descriptor);

                Items[id] = sampler;
            }

            return sampler;
        }

        protected override void InvalidateRangeImpl(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            for (; address < endAddress; address += DescriptorSize)
            {
                int id = (int)((address - Address) / DescriptorSize);

                Sampler sampler = Items[id];

                if (sampler != null)
                {
                    sampler.Dispose();

                    Items[id] = null;
                }
            }
        }

        protected override void Delete(Sampler item)
        {
            item?.Dispose();
        }
    }
}