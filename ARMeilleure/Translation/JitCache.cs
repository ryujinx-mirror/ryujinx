using ARMeilleure.CodeGen;
using ARMeilleure.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    static class JitCache
    {
        private const int PageSize = 4 * 1024;
        private const int PageMask = PageSize - 1;

        private const int CodeAlignment = 4; // Bytes
        private const int CacheSize = 2047 * 1024 * 1024;

        private static ReservedRegion _jitRegion;
        private static int _offset;
        private static readonly List<JitCacheEntry> _cacheEntries = new List<JitCacheEntry>();

        private static readonly object _lock = new object();
        private static bool _initialized;

        public static void Initialize(IJitMemoryAllocator allocator)
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                _jitRegion = new ReservedRegion(allocator, CacheSize);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _jitRegion.ExpandIfNeeded(PageSize);
                    JitUnwindWindows.InstallFunctionTableHandler(_jitRegion.Pointer, CacheSize);

                    // The first page is used for the table based SEH structs.
                    _offset = PageSize;
                }
                _initialized = true;
            }
        }

        public static IntPtr Map(CompiledFunction func)
        {
            byte[] code = func.Code;

            lock (_lock)
            {
                Debug.Assert(_initialized);

                int funcOffset = Allocate(code.Length);

                IntPtr funcPtr = _jitRegion.Pointer + funcOffset;

                Marshal.Copy(code, 0, funcPtr, code.Length);

                ReprotectRange(funcOffset, code.Length);

                Add(new JitCacheEntry(funcOffset, code.Length, func.UnwindInfo));

                return funcPtr;
            }
        }

        private static void ReprotectRange(int offset, int size)
        {
            // Map pages that are already full as RX.
            // Map pages that are not full yet as RWX.
            // On unix, the address must be page aligned.
            int endOffs = offset + size;

            int pageStart = offset  & ~PageMask;
            int pageEnd   = endOffs & ~PageMask;

            int fullPagesSize = pageEnd - pageStart;

            if (fullPagesSize != 0)
            {
                _jitRegion.Block.MapAsRx((ulong)pageStart, (ulong)fullPagesSize);
            }

            int remaining = endOffs - pageEnd;

            if (remaining != 0)
            {
                _jitRegion.Block.MapAsRwx((ulong)pageEnd, (ulong)remaining);
            }
        }

        private static int Allocate(int codeSize)
        {
            codeSize = checked(codeSize + (CodeAlignment - 1)) & ~(CodeAlignment - 1);

            int allocOffset = _offset;

            _offset += codeSize;

            _jitRegion.ExpandIfNeeded((ulong)_offset);

            if ((ulong)(uint)_offset > CacheSize)
            {
                throw new OutOfMemoryException();
            }

            return allocOffset;
        }

        private static void Add(JitCacheEntry entry)
        {
            _cacheEntries.Add(entry);
        }

        public static bool TryFind(int offset, out JitCacheEntry entry)
        {
            lock (_lock)
            {
                foreach (JitCacheEntry cacheEntry in _cacheEntries)
                {
                    int endOffset = cacheEntry.Offset + cacheEntry.Size;

                    if (offset >= cacheEntry.Offset && offset < endOffset)
                    {
                        entry = cacheEntry;

                        return true;
                    }
                }
            }

            entry = default;

            return false;
        }
    }
}