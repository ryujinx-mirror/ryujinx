using ARMeilleure.Common;
using System;
using System.Runtime.CompilerServices;

namespace ARMeilleure
{
    static class Allocators
    {
        [ThreadStatic] private static ArenaAllocator _default;
        [ThreadStatic] private static ArenaAllocator _operands;
        [ThreadStatic] private static ArenaAllocator _operations;
        [ThreadStatic] private static ArenaAllocator _references;
        [ThreadStatic] private static ArenaAllocator _liveRanges;
        [ThreadStatic] private static ArenaAllocator _liveIntervals;

        public static ArenaAllocator Default => GetAllocator(ref _default, 256 * 1024, 4);
        public static ArenaAllocator Operands => GetAllocator(ref _operands, 64 * 1024, 8);
        public static ArenaAllocator Operations => GetAllocator(ref _operations, 64 * 1024, 8);
        public static ArenaAllocator References => GetAllocator(ref _references, 64 * 1024, 8);
        public static ArenaAllocator LiveRanges => GetAllocator(ref _liveRanges, 64 * 1024, 8);
        public static ArenaAllocator LiveIntervals => GetAllocator(ref _liveIntervals, 64 * 1024, 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ArenaAllocator GetAllocator(ref ArenaAllocator alloc, uint pageSize, uint pageCount)
        {
            alloc ??= new ArenaAllocator(pageSize, pageCount);

            return alloc;
        }

        public static void ResetAll()
        {
            Default.Reset();
            Operands.Reset();
            Operations.Reset();
            References.Reset();
        }
    }
}
