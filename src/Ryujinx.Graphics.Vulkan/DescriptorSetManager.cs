using Silk.NET.Vulkan;
using System;
using System.Diagnostics;

namespace Ryujinx.Graphics.Vulkan
{
    class DescriptorSetManager : IDisposable
    {
        private const uint DescriptorPoolMultiplier = 16;

        public class DescriptorPoolHolder : IDisposable
        {
            public Vk Api { get; }
            public Device Device { get; }

            private readonly DescriptorPool _pool;
            private readonly uint _capacity;
            private int _totalSets;
            private int _setsInUse;
            private bool _done;

            public unsafe DescriptorPoolHolder(Vk api, Device device)
            {
                Api = api;
                Device = device;

                var poolSizes = new[]
                {
                    new DescriptorPoolSize(DescriptorType.UniformBuffer, (1 + Constants.MaxUniformBufferBindings) * DescriptorPoolMultiplier),
                    new DescriptorPoolSize(DescriptorType.StorageBuffer, Constants.MaxStorageBufferBindings * DescriptorPoolMultiplier),
                    new DescriptorPoolSize(DescriptorType.CombinedImageSampler, Constants.MaxTextureBindings * DescriptorPoolMultiplier),
                    new DescriptorPoolSize(DescriptorType.StorageImage, Constants.MaxImageBindings * DescriptorPoolMultiplier),
                    new DescriptorPoolSize(DescriptorType.UniformTexelBuffer, Constants.MaxTextureBindings * DescriptorPoolMultiplier),
                    new DescriptorPoolSize(DescriptorType.StorageTexelBuffer, Constants.MaxImageBindings * DescriptorPoolMultiplier),
                };

                uint maxSets = (uint)poolSizes.Length * DescriptorPoolMultiplier;

                _capacity = maxSets;

                fixed (DescriptorPoolSize* pPoolsSize = poolSizes)
                {
                    var descriptorPoolCreateInfo = new DescriptorPoolCreateInfo
                    {
                        SType = StructureType.DescriptorPoolCreateInfo,
                        MaxSets = maxSets,
                        PoolSizeCount = (uint)poolSizes.Length,
                        PPoolSizes = pPoolsSize,
                    };

                    Api.CreateDescriptorPool(device, descriptorPoolCreateInfo, null, out _pool).ThrowOnError();
                }
            }

            public DescriptorSetCollection AllocateDescriptorSets(ReadOnlySpan<DescriptorSetLayout> layouts)
            {
                TryAllocateDescriptorSets(layouts, isTry: false, out var dsc);
                return dsc;
            }

            public bool TryAllocateDescriptorSets(ReadOnlySpan<DescriptorSetLayout> layouts, out DescriptorSetCollection dsc)
            {
                return TryAllocateDescriptorSets(layouts, isTry: true, out dsc);
            }

            private unsafe bool TryAllocateDescriptorSets(ReadOnlySpan<DescriptorSetLayout> layouts, bool isTry, out DescriptorSetCollection dsc)
            {
                Debug.Assert(!_done);

                DescriptorSet[] descriptorSets = new DescriptorSet[layouts.Length];

                fixed (DescriptorSet* pDescriptorSets = descriptorSets)
                {
                    fixed (DescriptorSetLayout* pLayouts = layouts)
                    {
                        var descriptorSetAllocateInfo = new DescriptorSetAllocateInfo
                        {
                            SType = StructureType.DescriptorSetAllocateInfo,
                            DescriptorPool = _pool,
                            DescriptorSetCount = (uint)layouts.Length,
                            PSetLayouts = pLayouts,
                        };

                        var result = Api.AllocateDescriptorSets(Device, &descriptorSetAllocateInfo, pDescriptorSets);
                        if (isTry && result == Result.ErrorOutOfPoolMemory)
                        {
                            _totalSets = (int)_capacity;
                            _done = true;
                            DestroyIfDone();
                            dsc = default;
                            return false;
                        }

                        result.ThrowOnError();
                    }
                }

                _totalSets += layouts.Length;
                _setsInUse += layouts.Length;

                dsc = new DescriptorSetCollection(this, descriptorSets);
                return true;
            }

            public void FreeDescriptorSets(DescriptorSetCollection dsc)
            {
                _setsInUse -= dsc.SetsCount;
                Debug.Assert(_setsInUse >= 0);
                DestroyIfDone();
            }

            public bool CanFit(int count)
            {
                if (_totalSets + count <= _capacity)
                {
                    return true;
                }

                _done = true;
                DestroyIfDone();
                return false;
            }

            private unsafe void DestroyIfDone()
            {
                if (_done && _setsInUse == 0)
                {
                    Api.DestroyDescriptorPool(Device, _pool, null);
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    unsafe
                    {
                        Api.DestroyDescriptorPool(Device, _pool, null);
                    }
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Dispose(true);
            }
        }

        private readonly Device _device;
        private DescriptorPoolHolder _currentPool;

        public DescriptorSetManager(Device device)
        {
            _device = device;
        }

        public Auto<DescriptorSetCollection> AllocateDescriptorSet(Vk api, DescriptorSetLayout layout)
        {
            Span<DescriptorSetLayout> layouts = stackalloc DescriptorSetLayout[1];
            layouts[0] = layout;
            return AllocateDescriptorSets(api, layouts);
        }

        public Auto<DescriptorSetCollection> AllocateDescriptorSets(Vk api, ReadOnlySpan<DescriptorSetLayout> layouts)
        {
            // If we fail the first time, just create a new pool and try again.
            if (!GetPool(api, layouts.Length).TryAllocateDescriptorSets(layouts, out var dsc))
            {
                dsc = GetPool(api, layouts.Length).AllocateDescriptorSets(layouts);
            }

            return new Auto<DescriptorSetCollection>(dsc);
        }

        private DescriptorPoolHolder GetPool(Vk api, int requiredCount)
        {
            if (_currentPool == null || !_currentPool.CanFit(requiredCount))
            {
                _currentPool = new DescriptorPoolHolder(api, _device);
            }

            return _currentPool;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentPool?.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}
