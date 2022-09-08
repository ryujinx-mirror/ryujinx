using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Vulkan.Queries;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    class PipelineFull : PipelineBase, IPipeline
    {
        private const ulong MinByteWeightForFlush = 256 * 1024 * 1024; // MB

        private bool _hasPendingQuery;

        private readonly List<QueryPool> _activeQueries;
        private CounterQueueEvent _activeConditionalRender;

        private readonly List<BufferedQuery> _pendingQueryCopies;
        private readonly List<BufferedQuery> _pendingQueryResets;

        private ulong _byteWeight;

        public PipelineFull(VulkanRenderer gd, Device device) : base(gd, device)
        {
            _activeQueries = new List<QueryPool>();
            _pendingQueryCopies = new();
            _pendingQueryResets = new List<BufferedQuery>();

            CommandBuffer = (Cbs = gd.CommandBufferPool.Rent()).CommandBuffer;
        }

        private void CopyPendingQuery()
        {
            foreach (var query in _pendingQueryCopies)
            {
                query.PoolCopy(Cbs);
            }

            lock (_pendingQueryResets)
            {
                foreach (var query in _pendingQueryResets)
                {
                    query.PoolReset(CommandBuffer);
                }

                _pendingQueryResets.Clear();
            }

            _pendingQueryCopies.Clear();
        }

        public void ClearRenderTargetColor(int index, int layer, int layerCount, uint componentMask, ColorF color)
        {
            if (FramebufferParams == null)
            {
                return;
            }

            if (componentMask != 0xf)
            {
                // We can't use CmdClearAttachments if not writing all components,
                // because on Vulkan, the pipeline state does not affect clears.
                var dstTexture = FramebufferParams.GetAttachment(index);
                if (dstTexture == null)
                {
                    return;
                }

                Span<float> clearColor = stackalloc float[4];
                clearColor[0] = color.Red;
                clearColor[1] = color.Green;
                clearColor[2] = color.Blue;
                clearColor[3] = color.Alpha;

                // TODO: Clear only the specified layer.
                Gd.HelperShader.Clear(
                    Gd,
                    dstTexture,
                    clearColor,
                    componentMask,
                    (int)FramebufferParams.Width,
                    (int)FramebufferParams.Height,
                    FramebufferParams.AttachmentFormats[index],
                    ClearScissor);
            }
            else
            {
                ClearRenderTargetColor(index, layer, layerCount, color);
            }
        }

        public void EndHostConditionalRendering()
        {
            if (Gd.Capabilities.SupportsConditionalRendering)
            {
                // Gd.ConditionalRenderingApi.CmdEndConditionalRendering(CommandBuffer);
            }
            else
            {
                // throw new NotSupportedException();
            }

            _activeConditionalRender?.ReleaseHostAccess();
            _activeConditionalRender = null;
        }

        public bool TryHostConditionalRendering(ICounterEvent value, ulong compare, bool isEqual)
        {
            // Compare an event and a constant value.
            if (value is CounterQueueEvent evt)
            {
                // Easy host conditional rendering when the check matches what GL can do:
                //  - Event is of type samples passed.
                //  - Result is not a combination of multiple queries.
                //  - Comparing against 0.
                //  - Event has not already been flushed.

                if (compare == 0 && evt.Type == CounterType.SamplesPassed && evt.ClearCounter)
                {
                    if (!value.ReserveForHostAccess())
                    {
                        // If the event has been flushed, then just use the values on the CPU.
                        // The query object may already be repurposed for another draw (eg. begin + end).
                        return false;
                    }

                    if (Gd.Capabilities.SupportsConditionalRendering)
                    {
                        var buffer = evt.GetBuffer().Get(Cbs, 0, sizeof(long)).Value;
                        var flags = isEqual ? ConditionalRenderingFlagsEXT.ConditionalRenderingInvertedBitExt : 0;

                        var conditionalRenderingBeginInfo = new ConditionalRenderingBeginInfoEXT()
                        {
                            SType = StructureType.ConditionalRenderingBeginInfoExt,
                            Buffer = buffer,
                            Flags = flags
                        };

                        // Gd.ConditionalRenderingApi.CmdBeginConditionalRendering(CommandBuffer, conditionalRenderingBeginInfo);
                    }

                    _activeConditionalRender = evt;
                    return true;
                }
            }

            // The GPU will flush the queries to CPU and evaluate the condition there instead.

            FlushPendingQuery(); // The thread will be stalled manually flushing the counter, so flush commands now.
            return false;
        }

        public bool TryHostConditionalRendering(ICounterEvent value, ICounterEvent compare, bool isEqual)
        {
            FlushPendingQuery(); // The thread will be stalled manually flushing the counter, so flush commands now.
            return false;
        }

        private void FlushPendingQuery()
        {
            if (_hasPendingQuery)
            {
                _hasPendingQuery = false;
                FlushCommandsImpl();
            }
        }

        public CommandBufferScoped GetPreloadCommandBuffer()
        {
            if (PreloadCbs == null)
            {
                PreloadCbs = Gd.CommandBufferPool.Rent();
            }

            return PreloadCbs.Value;
        }

        public void FlushCommandsIfWeightExceeding(IAuto disposedResource, ulong byteWeight)
        {
            bool usedByCurrentCb = disposedResource.HasCommandBufferDependency(Cbs);

            if (PreloadCbs != null && !usedByCurrentCb)
            {
                usedByCurrentCb = disposedResource.HasCommandBufferDependency(PreloadCbs.Value);
            }

            if (usedByCurrentCb)
            {
                // Since we can only free memory after the command buffer that uses a given resource was executed,
                // keeping the command buffer might cause a high amount of memory to be in use.
                // To prevent that, we force submit command buffers if the memory usage by resources
                // in use by the current command buffer is above a given limit, and those resources were disposed.
                _byteWeight += byteWeight;

                if (_byteWeight >= MinByteWeightForFlush)
                {
                    FlushCommandsImpl();
                }
            }
        }

        public void Restore()
        {
            if (Pipeline != null)
            {
                Gd.Api.CmdBindPipeline(CommandBuffer, Pbp, Pipeline.Get(Cbs).Value);
            }

            SignalCommandBufferChange();
        }

        public void FlushCommandsImpl()
        {
            EndRenderPass();

            foreach (var queryPool in _activeQueries)
            {
                Gd.Api.CmdEndQuery(CommandBuffer, queryPool, 0);
            }

            _byteWeight = 0;

            if (PreloadCbs != null)
            {
                PreloadCbs.Value.Dispose();
                PreloadCbs = null;
            }

            CommandBuffer = (Cbs = Gd.CommandBufferPool.ReturnAndRent(Cbs)).CommandBuffer;

            // Restore per-command buffer state.

            foreach (var queryPool in _activeQueries)
            {
                Gd.Api.CmdResetQueryPool(CommandBuffer, queryPool, 0, 1);
                Gd.Api.CmdBeginQuery(CommandBuffer, queryPool, 0, 0);
            }

            Restore();
        }

        public void BeginQuery(BufferedQuery query, QueryPool pool, bool needsReset)
        {
            if (needsReset)
            {
                EndRenderPass();

                Gd.Api.CmdResetQueryPool(CommandBuffer, pool, 0, 1);

                lock (_pendingQueryResets)
                {
                    _pendingQueryResets.Remove(query); // Might be present on here.
                }
            }

            Gd.Api.CmdBeginQuery(CommandBuffer, pool, 0, 0);

            _activeQueries.Add(pool);
        }

        public void EndQuery(QueryPool pool)
        {
            Gd.Api.CmdEndQuery(CommandBuffer, pool, 0);

            _activeQueries.Remove(pool);
        }

        public void ResetQuery(BufferedQuery query)
        {
            lock (_pendingQueryResets)
            {
                _pendingQueryResets.Add(query);
            }
        }

        public void CopyQueryResults(BufferedQuery query)
        {
            _pendingQueryCopies.Add(query);

            _hasPendingQuery = true;
        }

        protected override void SignalAttachmentChange()
        {
            FlushPendingQuery();
        }

        protected override void SignalRenderPassEnd()
        {
            CopyPendingQuery();
        }
    }
}
