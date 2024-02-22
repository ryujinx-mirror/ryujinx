using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    internal class BarrierBatch : IDisposable
    {
        private const int MaxBarriersPerCall = 16;

        private readonly VulkanRenderer _gd;

        private readonly NativeArray<MemoryBarrier> _memoryBarrierBatch = new(MaxBarriersPerCall);
        private readonly NativeArray<BufferMemoryBarrier> _bufferBarrierBatch = new(MaxBarriersPerCall);
        private readonly NativeArray<ImageMemoryBarrier> _imageBarrierBatch = new(MaxBarriersPerCall);

        private readonly List<BarrierWithStageFlags<MemoryBarrier>> _memoryBarriers = new();
        private readonly List<BarrierWithStageFlags<BufferMemoryBarrier>> _bufferBarriers = new();
        private readonly List<BarrierWithStageFlags<ImageMemoryBarrier>> _imageBarriers = new();
        private int _queuedBarrierCount;

        public BarrierBatch(VulkanRenderer gd)
        {
            _gd = gd;
        }

        private readonly record struct StageFlags : IEquatable<StageFlags>
        {
            public readonly PipelineStageFlags Source;
            public readonly PipelineStageFlags Dest;

            public StageFlags(PipelineStageFlags source, PipelineStageFlags dest)
            {
                Source = source;
                Dest = dest;
            }
        }

        private readonly struct BarrierWithStageFlags<T> where T : unmanaged
        {
            public readonly StageFlags Flags;
            public readonly T Barrier;

            public BarrierWithStageFlags(StageFlags flags, T barrier)
            {
                Flags = flags;
                Barrier = barrier;
            }

            public BarrierWithStageFlags(PipelineStageFlags srcStageFlags, PipelineStageFlags dstStageFlags, T barrier)
            {
                Flags = new StageFlags(srcStageFlags, dstStageFlags);
                Barrier = barrier;
            }
        }

        private void QueueBarrier<T>(List<BarrierWithStageFlags<T>> list, T barrier, PipelineStageFlags srcStageFlags, PipelineStageFlags dstStageFlags) where T : unmanaged
        {
            list.Add(new BarrierWithStageFlags<T>(srcStageFlags, dstStageFlags, barrier));
            _queuedBarrierCount++;
        }

        public void QueueBarrier(MemoryBarrier barrier, PipelineStageFlags srcStageFlags, PipelineStageFlags dstStageFlags)
        {
            QueueBarrier(_memoryBarriers, barrier, srcStageFlags, dstStageFlags);
        }

        public void QueueBarrier(BufferMemoryBarrier barrier, PipelineStageFlags srcStageFlags, PipelineStageFlags dstStageFlags)
        {
            QueueBarrier(_bufferBarriers, barrier, srcStageFlags, dstStageFlags);
        }

        public void QueueBarrier(ImageMemoryBarrier barrier, PipelineStageFlags srcStageFlags, PipelineStageFlags dstStageFlags)
        {
            QueueBarrier(_imageBarriers, barrier, srcStageFlags, dstStageFlags);
        }

        public unsafe void Flush(CommandBuffer cb, bool insideRenderPass, Action endRenderPass)
        {
            while (_queuedBarrierCount > 0)
            {
                int memoryCount = 0;
                int bufferCount = 0;
                int imageCount = 0;

                bool hasBarrier = false;
                StageFlags flags = default;

                static void AddBarriers<T>(
                    Span<T> target,
                    ref int queuedBarrierCount,
                    ref bool hasBarrier,
                    ref StageFlags flags,
                    ref int count,
                    List<BarrierWithStageFlags<T>> list) where T : unmanaged
                {
                    int firstMatch = -1;
                    int end = list.Count;

                    for (int i = 0; i < list.Count; i++)
                    {
                        BarrierWithStageFlags<T> barrier = list[i];

                        if (!hasBarrier)
                        {
                            flags = barrier.Flags;
                            hasBarrier = true;

                            target[count++] = barrier.Barrier;
                            queuedBarrierCount--;
                            firstMatch = i;

                            if (count >= target.Length)
                            {
                                end = i + 1;
                                break;
                            }
                        }
                        else
                        {
                            if (flags.Equals(barrier.Flags))
                            {
                                target[count++] = barrier.Barrier;
                                queuedBarrierCount--;

                                if (firstMatch == -1)
                                {
                                    firstMatch = i;
                                }

                                if (count >= target.Length)
                                {
                                    end = i + 1;
                                    break;
                                }
                            }
                            else
                            {
                                // Delete consumed barriers from the first match to the current non-match.
                                if (firstMatch != -1)
                                {
                                    int deleteCount = i - firstMatch;
                                    list.RemoveRange(firstMatch, deleteCount);
                                    i -= deleteCount;

                                    firstMatch = -1;
                                }
                            }
                        }
                    }

                    if (firstMatch == 0 && end == list.Count)
                    {
                        list.Clear();
                    }
                    else if (firstMatch != -1)
                    {
                        int deleteCount = end - firstMatch;

                        list.RemoveRange(firstMatch, deleteCount);
                    }
                }

                if (insideRenderPass)
                {
                    // Image barriers queued in the batch are meant to be globally scoped,
                    // but inside a render pass they're scoped to just the range of the render pass.

                    // On MoltenVK, we just break the rules and always use image barrier.
                    // On desktop GPUs, all barriers are globally scoped, so we just replace it with a generic memory barrier.
                    // TODO: On certain GPUs, we need to split render pass so the barrier scope is global. When this is done,
                    //       notify the resource that it should add a barrier as soon as a render pass ends to avoid this in future.

                    if (!_gd.IsMoltenVk)
                    {
                        foreach (var barrier in _imageBarriers)
                        {
                            _memoryBarriers.Add(new BarrierWithStageFlags<MemoryBarrier>(
                                barrier.Flags,
                                new MemoryBarrier()
                                {
                                    SType = StructureType.MemoryBarrier,
                                    SrcAccessMask = barrier.Barrier.SrcAccessMask,
                                    DstAccessMask = barrier.Barrier.DstAccessMask
                                }));
                        }

                        _imageBarriers.Clear();
                    }
                }

                AddBarriers(_memoryBarrierBatch.AsSpan(), ref _queuedBarrierCount, ref hasBarrier, ref flags, ref memoryCount, _memoryBarriers);
                AddBarriers(_bufferBarrierBatch.AsSpan(), ref _queuedBarrierCount, ref hasBarrier, ref flags, ref bufferCount, _bufferBarriers);
                AddBarriers(_imageBarrierBatch.AsSpan(), ref _queuedBarrierCount, ref hasBarrier, ref flags, ref imageCount, _imageBarriers);

                if (hasBarrier)
                {
                    PipelineStageFlags srcStageFlags = flags.Source;

                    if (insideRenderPass)
                    {
                        // Inside a render pass, barrier stages can only be from rasterization.
                        srcStageFlags &= ~PipelineStageFlags.ComputeShaderBit;
                    }

                    _gd.Api.CmdPipelineBarrier(
                        cb,
                        srcStageFlags,
                        flags.Dest,
                        0,
                        (uint)memoryCount,
                        _memoryBarrierBatch.Pointer,
                        (uint)bufferCount,
                        _bufferBarrierBatch.Pointer,
                        (uint)imageCount,
                        _imageBarrierBatch.Pointer);
                }
            }
        }

        public void Dispose()
        {
            _memoryBarrierBatch.Dispose();
            _bufferBarrierBatch.Dispose();
            _imageBarrierBatch.Dispose();
        }
    }
}
