using Ryujinx.HLE.HOS.Kernel.Memory;
using System;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    static class KSystemControl
    {
        private const ulong Kb = 1024;
        private const ulong Mb = 1024 * Kb;
        private const ulong Gb = 1024 * Mb;

        private const ulong PageSize = 4 * Kb;

        private const ulong RequiredNonSecureSystemPoolSizeVi = 0x2238 * PageSize;
        private const ulong RequiredNonSecureSystemPoolSizeNvservices = 0x710 * PageSize;
        private const ulong RequiredNonSecureSystemPoolSizeOther = 0x80 * PageSize;

        private const ulong RequiredNonSecureSystemPoolSize =
            RequiredNonSecureSystemPoolSizeVi +
            RequiredNonSecureSystemPoolSizeNvservices +
            RequiredNonSecureSystemPoolSizeOther;

        public static ulong GetApplicationPoolSize(MemoryArrange arrange)
        {
            return arrange switch
            {
                MemoryArrange.MemoryArrange4GB or
                MemoryArrange.MemoryArrange4GBSystemDev or
                MemoryArrange.MemoryArrange6GBAppletDev => 3285 * Mb,
                MemoryArrange.MemoryArrange4GBAppletDev => 2048 * Mb,
                MemoryArrange.MemoryArrange6GB or
                MemoryArrange.MemoryArrange8GB => 4916 * Mb,
                _ => throw new ArgumentException($"Invalid memory arrange \"{arrange}\".")
            };
        }

        public static ulong GetAppletPoolSize(MemoryArrange arrange)
        {
            return arrange switch
            {
                MemoryArrange.MemoryArrange4GB => 507 * Mb,
                MemoryArrange.MemoryArrange4GBAppletDev => 1554 * Mb,
                MemoryArrange.MemoryArrange4GBSystemDev => 448 * Mb,
                MemoryArrange.MemoryArrange6GB => 562 * Mb,
                MemoryArrange.MemoryArrange6GBAppletDev or
                MemoryArrange.MemoryArrange8GB => 2193 * Mb,
                _ => throw new ArgumentException($"Invalid memory arrange \"{arrange}\".")
            };
        }

        public static ulong GetMinimumNonSecureSystemPoolSize()
        {
            return RequiredNonSecureSystemPoolSize;
        }

        public static ulong GetDramEndAddress(MemorySize size)
        {
            return DramMemoryMap.DramBase + GetDramSize(size);
        }

        public static ulong GenerateRandom()
        {
            // TODO
            return 0;
        }

        public static ulong GetDramSize(MemorySize size)
        {
            return size switch
            {
                MemorySize.MemorySize4GB => 4 * Gb,
                MemorySize.MemorySize6GB => 6 * Gb,
                MemorySize.MemorySize8GB => 8 * Gb,
                _ => throw new ArgumentException($"Invalid memory size \"{size}\".")
            };
        }
    }
}