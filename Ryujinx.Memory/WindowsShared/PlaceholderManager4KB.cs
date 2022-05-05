using System;
using System.Runtime.Versioning;

namespace Ryujinx.Memory.WindowsShared
{
    /// <summary>
    /// Windows 4KB memory placeholder manager.
    /// </summary>
    [SupportedOSPlatform("windows")]
    class PlaceholderManager4KB
    {
        private const int PageSize = MemoryManagementWindows.PageSize;

        private readonly IntervalTree<ulong, byte> _mappings;

        /// <summary>
        /// Creates a new instance of the Windows 4KB memory placeholder manager.
        /// </summary>
        public PlaceholderManager4KB()
        {
            _mappings = new IntervalTree<ulong, byte>();
        }

        /// <summary>
        /// Unmaps the specified range of memory and marks it as mapped internally.
        /// </summary>
        /// <remarks>
        /// Since this marks the range as mapped, the expectation is that the range will be mapped after calling this method.
        /// </remarks>
        /// <param name="location">Memory address to unmap and mark as mapped</param>
        /// <param name="size">Size of the range in bytes</param>
        public void UnmapAndMarkRangeAsMapped(IntPtr location, IntPtr size)
        {
            ulong startAddress = (ulong)location;
            ulong unmapSize = (ulong)size;
            ulong endAddress = startAddress + unmapSize;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, byte>>();
            int count = 0;

            lock (_mappings)
            {
                count = _mappings.Get(startAddress, endAddress, ref overlaps);
            }

            for (int index = 0; index < count; index++)
            {
                var overlap = overlaps[index];

                // Tree operations might modify the node start/end values, so save a copy before we modify the tree.
                ulong overlapStart = overlap.Start;
                ulong overlapEnd = overlap.End;
                ulong overlapValue = overlap.Value;

                _mappings.Remove(overlap);

                ulong unmapStart = Math.Max(overlapStart, startAddress);
                ulong unmapEnd = Math.Min(overlapEnd, endAddress);

                if (overlapStart < startAddress)
                {
                    startAddress = overlapStart;
                }

                if (overlapEnd > endAddress)
                {
                    endAddress = overlapEnd;
                }

                ulong currentAddress = unmapStart;
                while (currentAddress < unmapEnd)
                {
                    WindowsApi.UnmapViewOfFile2(WindowsApi.CurrentProcessHandle, (IntPtr)currentAddress, 2);
                    currentAddress += PageSize;
                }
            }

            _mappings.Add(startAddress, endAddress, 0);
        }

        /// <summary>
        /// Unmaps views at the specified memory range.
        /// </summary>
        /// <param name="location">Address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        public void UnmapView(IntPtr location, IntPtr size)
        {
            ulong startAddress = (ulong)location;
            ulong unmapSize = (ulong)size;
            ulong endAddress = startAddress + unmapSize;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, byte>>();
            int count = 0;

            lock (_mappings)
            {
                count = _mappings.Get(startAddress, endAddress, ref overlaps);
            }

            for (int index = 0; index < count; index++)
            {
                var overlap = overlaps[index];

                // Tree operations might modify the node start/end values, so save a copy before we modify the tree.
                ulong overlapStart = overlap.Start;
                ulong overlapEnd = overlap.End;

                _mappings.Remove(overlap);

                if (overlapStart < startAddress)
                {
                    _mappings.Add(overlapStart, startAddress, 0);
                }

                if (overlapEnd > endAddress)
                {
                    _mappings.Add(endAddress, overlapEnd, 0);
                }

                ulong unmapStart = Math.Max(overlapStart, startAddress);
                ulong unmapEnd = Math.Min(overlapEnd, endAddress);

                ulong currentAddress = unmapStart;
                while (currentAddress < unmapEnd)
                {
                    WindowsApi.UnmapViewOfFile2(WindowsApi.CurrentProcessHandle, (IntPtr)currentAddress, 2);
                    currentAddress += PageSize;
                }
            }
        }

        /// <summary>
        /// Unmaps mapped memory at a given range.
        /// </summary>
        /// <param name="location">Address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        public void UnmapRange(IntPtr location, IntPtr size)
        {
            ulong startAddress = (ulong)location;
            ulong unmapSize = (ulong)size;
            ulong endAddress = startAddress + unmapSize;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, byte>>();
            int count = 0;

            lock (_mappings)
            {
                count = _mappings.Get(startAddress, endAddress, ref overlaps);
            }

            for (int index = 0; index < count; index++)
            {
                var overlap = overlaps[index];

                // Tree operations might modify the node start/end values, so save a copy before we modify the tree.
                ulong unmapStart = Math.Max(overlap.Start, startAddress);
                ulong unmapEnd = Math.Min(overlap.End, endAddress);

                _mappings.Remove(overlap);

                ulong currentAddress = unmapStart;
                while (currentAddress < unmapEnd)
                {
                    WindowsApi.UnmapViewOfFile2(WindowsApi.CurrentProcessHandle, (IntPtr)currentAddress, 2);
                    currentAddress += PageSize;
                }
            }
        }
    }
}