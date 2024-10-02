using System;
using System.Diagnostics.CodeAnalysis;

namespace LibRyujinx.Jni.Values
{
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This struct is created only by binary operations.")]
    public readonly struct JInvokeInterface
    {
#pragma warning disable 0169
        private readonly IntPtr _reserved0;
        private readonly IntPtr _reserved1;
        private readonly IntPtr _reserved2;
#pragma warning restore 0169
        internal IntPtr DestroyJavaVMPointer { get; init; }
        internal IntPtr AttachCurrentThreadPointer { get; init; }
        internal IntPtr DetachCurrentThreadPointer { get; init; }
        internal IntPtr GetEnvPointer { get; init; }
        internal IntPtr AttachCurrentThreadAsDaemonPointer { get; init; }
    }
}
