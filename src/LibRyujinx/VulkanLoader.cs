using Ryujinx.Common.Logging;
using Silk.NET.Core.Contexts;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibRyujinx
{
    public class VulkanLoader : IDisposable
    {
        private delegate IntPtr GetInstanceProcAddress(IntPtr instance, IntPtr name);
        private delegate IntPtr GetDeviceProcAddress(IntPtr device, IntPtr name);

        private IntPtr _loadedLibrary = IntPtr.Zero;
        private GetInstanceProcAddress _getInstanceProcAddr;
        private GetDeviceProcAddress _getDeviceProcAddr;

        public void Dispose()
        {
            if (_loadedLibrary != IntPtr.Zero)
            {
                NativeLibrary.Free(_loadedLibrary);
                _loadedLibrary = IntPtr.Zero;
            }
        }

        public VulkanLoader(IntPtr driver)
        {
            _loadedLibrary = driver;

            if (_loadedLibrary != IntPtr.Zero)
            {
                var instanceGetProc = NativeLibrary.GetExport(_loadedLibrary, "vkGetInstanceProcAddr");
                var deviceProc = NativeLibrary.GetExport(_loadedLibrary, "vkGetDeviceProcAddr");

                _getInstanceProcAddr = Marshal.GetDelegateForFunctionPointer<GetInstanceProcAddress>(instanceGetProc);
                _getDeviceProcAddr = Marshal.GetDelegateForFunctionPointer<GetDeviceProcAddress>(deviceProc);
            }
        }

        public unsafe Vk GetApi()
        {

            if (_loadedLibrary == IntPtr.Zero)
            {
                return Vk.GetApi();
            }
            var ctx = new MultiNativeContext(new INativeContext[1]);
            var ret = new Vk(ctx);
            ctx.Contexts[0] = new LamdaNativeContext
            (
                x =>
                {
                    var xPtr = Marshal.StringToHGlobalAnsi(x);
                    byte* xp = (byte*)xPtr;
                    try
                    {
                        nint ptr = default;
                        ptr = _getInstanceProcAddr(ret.CurrentInstance.GetValueOrDefault().Handle, xPtr);

                        if (ptr == default)
                        {
                            ptr = _getInstanceProcAddr(IntPtr.Zero, xPtr);

                            if (ptr == default)
                            {
                                var currentDevice = ret.CurrentDevice.GetValueOrDefault().Handle;
                                if (currentDevice != IntPtr.Zero)
                                {
                                    ptr = _getDeviceProcAddr(currentDevice, xPtr);
                                }

                                if (ptr == default)
                                {
                                    Logger.Warning?.Print(LogClass.Gpu, $"Failed to get function pointer: {x}");
                                }

                            }
                        }

                        return ptr;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(xPtr);
                    }
                }
            );
            return ret;
        }
    }
}
