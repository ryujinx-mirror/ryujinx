using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Values;

public readonly struct JNativeMethod
{
	internal ReadOnlyValPtr<Byte> Name { get; init; }
	internal ReadOnlyValPtr<Byte> Signature { get; init; }
	internal IntPtr Pointer { get; init; }
}
