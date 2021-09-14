using Ryujinx.HLE.HOS.Kernel.Common;
using System;

namespace Ryujinx.HLE
{
    public enum MemoryConfiguration
    {
        MemoryConfiguration4GB          = 0,
        MemoryConfiguration4GBAppletDev = 1,
        MemoryConfiguration4GBSystemDev = 2,
        MemoryConfiguration6GB          = 3,
        MemoryConfiguration6GBAppletDev = 4,
        MemoryConfiguration8GB          = 5
    }

    static class MemoryConfigurationExtensions
    {
        private const ulong Gb = 1024 * 1024 * 1024;

        public static MemoryArrange ToKernelMemoryArrange(this MemoryConfiguration configuration)
        {
            return configuration switch
            {
                MemoryConfiguration.MemoryConfiguration4GB          => MemoryArrange.MemoryArrange4GB,
                MemoryConfiguration.MemoryConfiguration4GBAppletDev => MemoryArrange.MemoryArrange4GBAppletDev,
                MemoryConfiguration.MemoryConfiguration4GBSystemDev => MemoryArrange.MemoryArrange4GBSystemDev,
                MemoryConfiguration.MemoryConfiguration6GB          => MemoryArrange.MemoryArrange6GB,
                MemoryConfiguration.MemoryConfiguration6GBAppletDev => MemoryArrange.MemoryArrange6GBAppletDev,
                MemoryConfiguration.MemoryConfiguration8GB          => MemoryArrange.MemoryArrange8GB,
                _ => throw new AggregateException($"Invalid memory configuration \"{configuration}\".")
            };
        }

        public static MemorySize ToKernelMemorySize(this MemoryConfiguration configuration)
        {
            return configuration switch
            {
                MemoryConfiguration.MemoryConfiguration4GB or
                MemoryConfiguration.MemoryConfiguration4GBAppletDev or
                MemoryConfiguration.MemoryConfiguration4GBSystemDev => MemorySize.MemorySize4GB,
                MemoryConfiguration.MemoryConfiguration6GB or
                MemoryConfiguration.MemoryConfiguration6GBAppletDev => MemorySize.MemorySize6GB,
                MemoryConfiguration.MemoryConfiguration8GB          => MemorySize.MemorySize8GB,
                _ => throw new AggregateException($"Invalid memory configuration \"{configuration}\".")
            };
        }

        public static ulong ToDramSize(this MemoryConfiguration configuration)
        {
            return configuration switch
            {
                MemoryConfiguration.MemoryConfiguration4GB or
                MemoryConfiguration.MemoryConfiguration4GBAppletDev or
                MemoryConfiguration.MemoryConfiguration4GBSystemDev => 4 * Gb,
                MemoryConfiguration.MemoryConfiguration6GB or
                MemoryConfiguration.MemoryConfiguration6GBAppletDev => 6 * Gb,
                MemoryConfiguration.MemoryConfiguration8GB          => 8 * Gb,
                _ => throw new AggregateException($"Invalid memory configuration \"{configuration}\".")
            };
        }
    }
}