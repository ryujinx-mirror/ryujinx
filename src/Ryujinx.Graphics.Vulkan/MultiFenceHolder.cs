using Silk.NET.Vulkan;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Vulkan
{
    /// <summary>
    /// Holder for multiple host GPU fences.
    /// </summary>
    class MultiFenceHolder
    {
        private static int BufferUsageTrackingGranularity = 4096;

        private readonly Dictionary<FenceHolder, int> _fences;
        private BufferUsageBitmap _bufferUsageBitmap;

        /// <summary>
        /// Creates a new instance of the multiple fence holder.
        /// </summary>
        public MultiFenceHolder()
        {
            _fences = new Dictionary<FenceHolder, int>();
        }

        /// <summary>
        /// Creates a new instance of the multiple fence holder, with a given buffer size in mind.
        /// </summary>
        /// <param name="size">Size of the buffer</param>
        public MultiFenceHolder(int size)
        {
            _fences = new Dictionary<FenceHolder, int>();
            _bufferUsageBitmap = new BufferUsageBitmap(size, BufferUsageTrackingGranularity);
        }

        /// <summary>
        /// Adds buffer usage information to the uses list.
        /// </summary>
        /// <param name="cbIndex">Index of the command buffer where the buffer is used</param>
        /// <param name="offset">Offset of the buffer being used</param>
        /// <param name="size">Size of the buffer region being used, in bytes</param>
        public void AddBufferUse(int cbIndex, int offset, int size)
        {
            _bufferUsageBitmap.Add(cbIndex, offset, size);
        }

        /// <summary>
        /// Removes all buffer usage information for a given command buffer.
        /// </summary>
        /// <param name="cbIndex">Index of the command buffer where the buffer is used</param>
        public void RemoveBufferUses(int cbIndex)
        {
            _bufferUsageBitmap?.Clear(cbIndex);
        }

        /// <summary>
        /// Checks if a given range of a buffer is being used by a command buffer still being processed by the GPU.
        /// </summary>
        /// <param name="cbIndex">Index of the command buffer where the buffer is used</param>
        /// <param name="offset">Offset of the buffer being used</param>
        /// <param name="size">Size of the buffer region being used, in bytes</param>
        /// <returns>True if in use, false otherwise</returns>
        public bool IsBufferRangeInUse(int cbIndex, int offset, int size)
        {
            return _bufferUsageBitmap.OverlapsWith(cbIndex, offset, size);
        }

        /// <summary>
        /// Checks if a given range of a buffer is being used by any command buffer still being processed by the GPU.
        /// </summary>
        /// <param name="offset">Offset of the buffer being used</param>
        /// <param name="size">Size of the buffer region being used, in bytes</param>
        /// <returns>True if in use, false otherwise</returns>
        public bool IsBufferRangeInUse(int offset, int size)
        {
            return _bufferUsageBitmap.OverlapsWith(offset, size);
        }

        /// <summary>
        /// Adds a fence to the holder.
        /// </summary>
        /// <param name="cbIndex">Command buffer index of the command buffer that owns the fence</param>
        /// <param name="fence">Fence to be added</param>
        public void AddFence(int cbIndex, FenceHolder fence)
        {
            lock (_fences)
            {
                _fences.TryAdd(fence, cbIndex);
            }
        }

        /// <summary>
        /// Removes a fence from the holder.
        /// </summary>
        /// <param name="cbIndex">Command buffer index of the command buffer that owns the fence</param>
        /// <param name="fence">Fence to be removed</param>
        public void RemoveFence(int cbIndex, FenceHolder fence)
        {
            lock (_fences)
            {
                _fences.Remove(fence);
            }
        }

        /// <summary>
        /// Wait until all the fences on the holder are signaled.
        /// </summary>
        /// <param name="api">Vulkan API instance</param>
        /// <param name="device">GPU device that the fences belongs to</param>
        public void WaitForFences(Vk api, Device device)
        {
            WaitForFencesImpl(api, device, 0, 0, false, 0UL);
        }

        /// <summary>
        /// Wait until all the fences on the holder with buffer uses overlapping the specified range are signaled.
        /// </summary>
        /// <param name="api">Vulkan API instance</param>
        /// <param name="device">GPU device that the fences belongs to</param>
        /// <param name="offset">Start offset of the buffer range</param>
        /// <param name="size">Size of the buffer range in bytes</param>
        public void WaitForFences(Vk api, Device device, int offset, int size)
        {
            WaitForFencesImpl(api, device, offset, size, false, 0UL);
        }

        /// <summary>
        /// Wait until all the fences on the holder are signaled, or the timeout expires.
        /// </summary>
        /// <param name="api">Vulkan API instance</param>
        /// <param name="device">GPU device that the fences belongs to</param>
        /// <param name="timeout">Timeout in nanoseconds</param>
        /// <returns>True if all fences were signaled, false otherwise</returns>
        public bool WaitForFences(Vk api, Device device, ulong timeout)
        {
            return WaitForFencesImpl(api, device, 0, 0, true, timeout);
        }

        /// <summary>
        /// Wait until all the fences on the holder with buffer uses overlapping the specified range are signaled.
        /// </summary>
        /// <param name="api">Vulkan API instance</param>
        /// <param name="device">GPU device that the fences belongs to</param>
        /// <param name="offset">Start offset of the buffer range</param>
        /// <param name="size">Size of the buffer range in bytes</param>
        /// <param name="hasTimeout">Indicates if <paramref name="timeout"/> should be used</param>
        /// <param name="timeout">Timeout in nanoseconds</param>
        /// <returns>True if all fences were signaled before the timeout expired, false otherwise</returns>
        private bool WaitForFencesImpl(Vk api, Device device, int offset, int size, bool hasTimeout, ulong timeout)
        {
            FenceHolder[] fenceHolders;
            Fence[] fences;

            lock (_fences)
            {
                fenceHolders = size != 0 ? GetOverlappingFences(offset, size) : _fences.Keys.ToArray();
                fences = new Fence[fenceHolders.Length];

                for (int i = 0; i < fenceHolders.Length; i++)
                {
                    fences[i] = fenceHolders[i].Get();
                }
            }

            if (fences.Length == 0)
            {
                return true;
            }

            bool signaled = true;

            if (hasTimeout)
            {
                signaled = FenceHelper.AllSignaled(api, device, fences, timeout);
            }
            else
            {
                FenceHelper.WaitAllIndefinitely(api, device, fences);
            }

            for (int i = 0; i < fenceHolders.Length; i++)
            {
                fenceHolders[i].Put();
            }

            return signaled;
        }

        /// <summary>
        /// Gets fences to wait for use of a given buffer region.
        /// </summary>
        /// <param name="offset">Offset of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        /// <returns>Fences for the specified region</returns>
        private FenceHolder[] GetOverlappingFences(int offset, int size)
        {
            List<FenceHolder> overlapping = new List<FenceHolder>();

            foreach (var kv in _fences)
            {
                var fence = kv.Key;
                var ownerCbIndex = kv.Value;

                if (_bufferUsageBitmap.OverlapsWith(ownerCbIndex, offset, size))
                {
                    overlapping.Add(fence);
                }
            }

            return overlapping.ToArray();
        }
    }
}
