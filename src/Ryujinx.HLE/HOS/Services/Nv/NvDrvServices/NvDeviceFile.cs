using Ryujinx.Common.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices
{
    abstract class NvDeviceFile
    {
        public readonly ServiceCtx Context;
        public readonly ulong Owner;

        public string Path;

        public NvDeviceFile(ServiceCtx context, ulong owner)
        {
            Context = context;
            Owner = owner;
        }

        public virtual NvInternalResult QueryEvent(out int eventHandle, uint eventId)
        {
            eventHandle = 0;

            return NvInternalResult.NotImplemented;
        }

        public virtual NvInternalResult MapSharedMemory(int sharedMemoryHandle, uint argument)
        {
            // Close shared memory immediately as we don't use it.
            Context.Device.System.KernelContext.Syscall.CloseHandle(sharedMemoryHandle);

            return NvInternalResult.NotImplemented;
        }

        public virtual NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            return NvInternalResult.NotImplemented;
        }

        public virtual NvInternalResult Ioctl2(NvIoctl command, Span<byte> arguments, Span<byte> inlineInBuffer)
        {
            return NvInternalResult.NotImplemented;
        }

        public virtual NvInternalResult Ioctl3(NvIoctl command, Span<byte> arguments, Span<byte> inlineOutBuffer)
        {
            return NvInternalResult.NotImplemented;
        }

        protected delegate NvInternalResult IoctlProcessor<T>(ref T arguments);
        protected delegate NvInternalResult IoctlProcessorSpan<T>(Span<T> arguments);
        protected delegate NvInternalResult IoctlProcessorInline<T, T1>(ref T arguments, ref T1 inlineData);
        protected delegate NvInternalResult IoctlProcessorInlineSpan<T, T1>(ref T arguments, Span<T1> inlineData);

        private static NvInternalResult PrintResult(MethodInfo info, NvInternalResult result)
        {
            Logger.Debug?.Print(LogClass.ServiceNv, $"{info.Name} returned result {result}");

            return result;
        }

        protected static NvInternalResult CallIoctlMethod<T>(IoctlProcessor<T> callback, Span<byte> arguments) where T : struct
        {
            Debug.Assert(arguments.Length == Unsafe.SizeOf<T>());

            return PrintResult(callback.Method, callback(ref MemoryMarshal.Cast<byte, T>(arguments)[0]));
        }

        protected static NvInternalResult CallIoctlMethod<T, T1>(IoctlProcessorInline<T, T1> callback, Span<byte> arguments, Span<byte> inlineBuffer) where T : struct where T1 : struct
        {
            Debug.Assert(arguments.Length == Unsafe.SizeOf<T>());
            Debug.Assert(inlineBuffer.Length == Unsafe.SizeOf<T1>());

            return PrintResult(callback.Method, callback(ref MemoryMarshal.Cast<byte, T>(arguments)[0], ref MemoryMarshal.Cast<byte, T1>(inlineBuffer)[0]));
        }

        protected static NvInternalResult CallIoctlMethod<T>(IoctlProcessorSpan<T> callback, Span<byte> arguments) where T : struct
        {
            return PrintResult(callback.Method, callback(MemoryMarshal.Cast<byte, T>(arguments)));
        }

        protected static NvInternalResult CallIoctlMethod<T, T1>(IoctlProcessorInlineSpan<T, T1> callback, Span<byte> arguments, Span<byte> inlineBuffer) where T : struct where T1 : struct
        {
            Debug.Assert(arguments.Length == Unsafe.SizeOf<T>());

            return PrintResult(callback.Method, callback(ref MemoryMarshal.Cast<byte, T>(arguments)[0], MemoryMarshal.Cast<byte, T1>(inlineBuffer)));
        }

        public abstract void Close();
    }
}
