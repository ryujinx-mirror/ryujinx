using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Ava.Ui.Vulkan
{
    public class VulkanOptions
    {
        /// <summary>
        /// Sets the application name of the Vulkan instance
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Specifies the Vulkan API version to use
        /// </summary>
        public Version VulkanVersion { get; set; } = new Version(1, 1, 0);

        /// <summary>
        /// Specifies additional extensions to enable if available on the instance
        /// </summary>
        public IEnumerable<string> InstanceExtensions { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Specifies layers to enable if available on the instance
        /// </summary>
        public IEnumerable<string> EnabledLayers { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Enables the debug layer
        /// </summary>
        public bool UseDebug { get; set; }

        /// <summary>
        /// Selects the first suitable discrete GPU available
        /// </summary>
        public bool PreferDiscreteGpu { get; set; }

        /// <summary>
        /// Sets the device to use if available and suitable.
        /// </summary>
        public string PreferredDevice { get; set; }

        /// <summary>
        /// Max number of device queues to request
        /// </summary>
        public uint MaxQueueCount { get; set; }
    }
}
