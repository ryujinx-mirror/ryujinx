using Rxmxnx.PInvoke;
using System;

namespace LibRyujinx.Jni.Values;

public readonly struct JavaVMOption
{
	internal ReadOnlyValPtr<Byte> Name { get; init; }
	internal IntPtr ExtraInfo { get; init; }
}
