using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using System.Runtime.CompilerServices;
using VkBuffer = Silk.NET.Vulkan.Buffer;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class BufferHolder : IDisposable
    {
        private const int MaxUpdateBufferSize = 0x10000;

        public const AccessFlags DefaultAccessFlags =
            AccessFlags.AccessShaderReadBit |
            AccessFlags.AccessShaderWriteBit |
            AccessFlags.AccessTransferReadBit |
            AccessFlags.AccessTransferWriteBit |
            AccessFlags.AccessUniformReadBit |
            AccessFlags.AccessShaderReadBit |
            AccessFlags.AccessShaderWriteBit;

        private readonly VulkanRenderer _gd;
        private readonly Device _device;
        private readonly MemoryAllocation _allocation;
        private readonly Auto<DisposableBuffer> _buffer;
        private readonly Auto<MemoryAllocation> _allocationAuto;
        private readonly ulong _bufferHandle;

        private CacheByRange<BufferHolder> _cachedConvertedBuffers;

        public int Size { get; }

        private IntPtr _map;

        private readonly MultiFenceHolder _waitable;

        private bool _lastAccessIsWrite;

        public BufferHolder(VulkanRenderer gd, Device device, VkBuffer buffer, MemoryAllocation allocation, int size)
        {
            _gd = gd;
            _device = device;
            _allocation = allocation;
            _allocationAuto = new Auto<MemoryAllocation>(allocation);
            _waitable = new MultiFenceHolder(size);
            _buffer = new Auto<DisposableBuffer>(new DisposableBuffer(gd.Api, device, buffer), _waitable, _allocationAuto);
            _bufferHandle = buffer.Handle;
            Size = size;
            _map = allocation.HostPointer;
        }

        public unsafe Auto<DisposableBufferView> CreateView(VkFormat format, int offset, int size)
        {
            var bufferViewCreateInfo = new BufferViewCreateInfo()
            {
                SType = StructureType.BufferViewCreateInfo,
                Buffer = new VkBuffer(_bufferHandle),
                Format = format,
                Offset = (uint)offset,
                Range = (uint)size
            };

            _gd.Api.CreateBufferView(_device, bufferViewCreateInfo, null, out var bufferView).ThrowOnError();

            return new Auto<DisposableBufferView>(new DisposableBufferView(_gd.Api, _device, bufferView), _waitable, _buffer);
        }

        public unsafe void InsertBarrier(CommandBuffer commandBuffer, bool isWrite)
        {
            // If the last access is write, we always need a barrier to be sure we will read or modify
            // the correct data.
            // If the last access is read, and current one is a write, we need to wait until the
            // read finishes to avoid overwriting data still in use.
            // Otherwise, if the last access is a read and the current one too, we don't need barriers.
            bool needsBarrier = isWrite || _lastAccessIsWrite;

            _lastAccessIsWrite = isWrite;

            if (needsBarrier)
            {
                MemoryBarrier memoryBarrier = new MemoryBarrier()
                {
                    SType = StructureType.MemoryBarrier,
                    SrcAccessMask = DefaultAccessFlags,
                    DstAccessMask = DefaultAccessFlags
                };

                _gd.Api.CmdPipelineBarrier(
                    commandBuffer,
                    PipelineStageFlags.PipelineStageAllCommandsBit,
                    PipelineStageFlags.PipelineStageAllCommandsBit,
                    DependencyFlags.DependencyDeviceGroupBit,
                    1,
                    memoryBarrier,
                    0,
                    null,
                    0,
                    null);
            }
        }

        public Auto<DisposableBuffer> GetBuffer()
        {
            return _buffer;
        }

        public Auto<DisposableBuffer> GetBuffer(CommandBuffer commandBuffer, bool isWrite = false)
        {
            if (isWrite)
            {
                _cachedConvertedBuffers.Clear();
            }

            return _buffer;
        }

        public BufferHandle GetHandle()
        {
            var handle = _bufferHandle;
            return Unsafe.As<ulong, BufferHandle>(ref handle);
        }

        public unsafe IntPtr Map(int offset, int mappingSize)
        {
            return _map;
        }

        public unsafe ReadOnlySpan<byte> GetData(int offset, int size)
        {
            if (_map != IntPtr.Zero)
            {
                return GetDataStorage(offset, size);
            }
            else
            {
                BackgroundResource resource = _gd.BackgroundResources.Get();

                if (_gd.CommandBufferPool.OwnedByCurrentThread)
                {
                    _gd.FlushAllCommands();

                    return resource.GetFlushBuffer().GetBufferData(_gd.CommandBufferPool, this, offset, size);
                }
                else
                {
                    return resource.GetFlushBuffer().GetBufferData(resource.GetPool(), this, offset, size);
                }
            }
        }

        public unsafe Span<byte> GetDataStorage(int offset, int size)
        {
            int mappingSize = Math.Min(size, Size - offset);

            if (_map != IntPtr.Zero)
            {
                return new Span<byte>((void*)(_map + offset), mappingSize);
            }

            throw new InvalidOperationException("The buffer is not host mapped.");
        }

        public unsafe void SetData(int offset, ReadOnlySpan<byte> data, CommandBufferScoped? cbs = null, Action endRenderPass = null)
        {
            int dataSize = Math.Min(data.Length, Size - offset);
            if (dataSize == 0)
            {
                return;
            }

            if (_map != IntPtr.Zero)
            {
                // If persistently mapped, set the data directly if the buffer is not currently in use.
                bool isRented = _buffer.HasRentedCommandBufferDependency(_gd.CommandBufferPool);

                // If the buffer is rented, take a little more time and check if the use overlaps this handle.
                bool needsFlush = isRented && _waitable.IsBufferRangeInUse(offset, dataSize);

                if (!needsFlush)
                {
                    WaitForFences(offset, dataSize);

                    data.Slice(0, dataSize).CopyTo(new Span<byte>((void*)(_map + offset), dataSize));

                    return;
                }
            }

            if (cbs != null && !(_buffer.HasCommandBufferDependency(cbs.Value) && _waitable.IsBufferRangeInUse(cbs.Value.CommandBufferIndex, offset, dataSize)))
            {
                // If the buffer hasn't been used on the command buffer yet, try to preload the data.
                // This avoids ending and beginning render passes on each buffer data upload.

                cbs = _gd.PipelineInternal.GetPreloadCommandBuffer();
                endRenderPass = null;
            }

            if (cbs == null ||
                !VulkanConfiguration.UseFastBufferUpdates ||
                data.Length > MaxUpdateBufferSize ||
                !TryPushData(cbs.Value, endRenderPass, offset, data))
            {
                _gd.BufferManager.StagingBuffer.PushData(_gd.CommandBufferPool, cbs, endRenderPass, this, offset, data);
            }
        }

        public unsafe void SetDataUnchecked(int offset, ReadOnlySpan<byte> data)
        {
            int dataSize = Math.Min(data.Length, Size - offset);
            if (dataSize == 0)
            {
                return;
            }

            if (_map != IntPtr.Zero)
            {
                data.Slice(0, dataSize).CopyTo(new Span<byte>((void*)(_map + offset), dataSize));
            }
            else
            {
                _gd.BufferManager.StagingBuffer.PushData(_gd.CommandBufferPool, null, null, this, offset, data);
            }
        }

        public void SetDataInline(CommandBufferScoped cbs, Action endRenderPass, int dstOffset, ReadOnlySpan<byte> data)
        {
            if (!TryPushData(cbs, endRenderPass, dstOffset, data))
            {
                throw new ArgumentException($"Invalid offset 0x{dstOffset:X} or data size 0x{data.Length:X}.");
            }
        }

        private unsafe bool TryPushData(CommandBufferScoped cbs, Action endRenderPass, int dstOffset, ReadOnlySpan<byte> data)
        {
            if ((dstOffset & 3) != 0 || (data.Length & 3) != 0)
            {
                return false;
            }

            endRenderPass?.Invoke();

            var dstBuffer = GetBuffer(cbs.CommandBuffer, true).Get(cbs, dstOffset, data.Length).Value;

            InsertBufferBarrier(
                _gd,
                cbs.CommandBuffer,
                dstBuffer,
                BufferHolder.DefaultAccessFlags,
                AccessFlags.AccessTransferWriteBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                PipelineStageFlags.PipelineStageTransferBit,
                dstOffset,
                data.Length);

            fixed (byte* pData = data)
            {
                for (ulong offset = 0; offset < (ulong)data.Length;)
                {
                    ulong size = Math.Min(MaxUpdateBufferSize, (ulong)data.Length - offset);
                    _gd.Api.CmdUpdateBuffer(cbs.CommandBuffer, dstBuffer, (ulong)dstOffset + offset, size, pData + offset);
                    offset += size;
                }
            }

            InsertBufferBarrier(
                _gd,
                cbs.CommandBuffer,
                dstBuffer,
                AccessFlags.AccessTransferWriteBit,
                BufferHolder.DefaultAccessFlags,
                PipelineStageFlags.PipelineStageTransferBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                dstOffset,
                data.Length);

            return true;
        }

        public static unsafe void Copy(
            VulkanRenderer gd,
            CommandBufferScoped cbs,
            Auto<DisposableBuffer> src,
            Auto<DisposableBuffer> dst,
            int srcOffset,
            int dstOffset,
            int size)
        {
            var srcBuffer = src.Get(cbs, srcOffset, size).Value;
            var dstBuffer = dst.Get(cbs, dstOffset, size).Value;

            InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                BufferHolder.DefaultAccessFlags,
                AccessFlags.AccessTransferWriteBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                PipelineStageFlags.PipelineStageTransferBit,
                dstOffset,
                size);

            var region = new BufferCopy((ulong)srcOffset, (ulong)dstOffset, (ulong)size);

            gd.Api.CmdCopyBuffer(cbs.CommandBuffer, srcBuffer, dstBuffer, 1, &region);

            InsertBufferBarrier(
                gd,
                cbs.CommandBuffer,
                dstBuffer,
                AccessFlags.AccessTransferWriteBit,
                BufferHolder.DefaultAccessFlags,
                PipelineStageFlags.PipelineStageTransferBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                dstOffset,
                size);
        }

        public static unsafe void InsertBufferBarrier(
            VulkanRenderer gd,
            CommandBuffer commandBuffer,
            VkBuffer buffer,
            AccessFlags srcAccessMask,
            AccessFlags dstAccessMask,
            PipelineStageFlags srcStageMask,
            PipelineStageFlags dstStageMask,
            int offset,
            int size)
        {
            BufferMemoryBarrier memoryBarrier = new BufferMemoryBarrier()
            {
                SType = StructureType.BufferMemoryBarrier,
                SrcAccessMask = srcAccessMask,
                DstAccessMask = dstAccessMask,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Buffer = buffer,
                Offset = (ulong)offset,
                Size = (ulong)size
            };

            gd.Api.CmdPipelineBarrier(
                commandBuffer,
                srcStageMask,
                dstStageMask,
                0,
                0,
                null,
                1,
                memoryBarrier,
                0,
                null);
        }

        public void WaitForFences()
        {
            _waitable.WaitForFences(_gd.Api, _device);
        }

        public void WaitForFences(int offset, int size)
        {
            _waitable.WaitForFences(_gd.Api, _device, offset, size);
        }

        public Auto<DisposableBuffer> GetBufferI8ToI16(CommandBufferScoped cbs, int offset, int size)
        {
            var key = new I8ToI16CacheKey();

            if (!_cachedConvertedBuffers.TryGetValue(offset, size, key, out var holder))
            {
                holder = _gd.BufferManager.Create(_gd, (size * 2 + 3) & ~3);

                _gd.PipelineInternal.EndRenderPass();
                _gd.HelperShader.ConvertI8ToI16(_gd, cbs, this, holder, offset, size);

                _cachedConvertedBuffers.Add(offset, size, key, holder);
            }

            return holder.GetBuffer();
        }

        public Auto<DisposableBuffer> GetAlignedVertexBuffer(CommandBufferScoped cbs, int offset, int size, int stride, int alignment)
        {
            var key = new AlignedVertexBufferCacheKey(_gd, stride, alignment);

            if (!_cachedConvertedBuffers.TryGetValue(offset, size, key, out var holder))
            {
                int alignedStride = (stride + (alignment - 1)) & -alignment;

                holder = _gd.BufferManager.Create(_gd, (size / stride) * alignedStride);

                _gd.PipelineInternal.EndRenderPass();
                _gd.HelperShader.ChangeStride(_gd, cbs, this, holder, offset, size, stride, alignedStride);

                key.SetBuffer(holder.GetBuffer());

                _cachedConvertedBuffers.Add(offset, size, key, holder);
            }

            return holder.GetBuffer();
        }

        public Auto<DisposableBuffer> GetBufferTopologyConversion(CommandBufferScoped cbs, int offset, int size, IndexBufferPattern pattern, int indexSize)
        {
            var key = new TopologyConversionCacheKey(_gd, pattern, indexSize);

            if (!_cachedConvertedBuffers.TryGetValue(offset, size, key, out var holder))
            {
                // The destination index size is always I32.

                int indexCount = size / indexSize;

                int convertedCount = pattern.GetConvertedCount(indexCount);

                holder = _gd.BufferManager.Create(_gd, convertedCount * 4);

                _gd.PipelineInternal.EndRenderPass();
                _gd.HelperShader.ConvertIndexBuffer(_gd, cbs, this, holder, pattern, indexSize, offset, indexCount);

                _cachedConvertedBuffers.Add(offset, size, key, holder);
            }

            return holder.GetBuffer();
        }

        public void Dispose()
        {
            _gd.PipelineInternal?.FlushCommandsIfWeightExceeding(_buffer, (ulong)Size);

            _buffer.Dispose();
            _allocationAuto.Dispose();
            _cachedConvertedBuffers.Dispose();
        }
    }
}
