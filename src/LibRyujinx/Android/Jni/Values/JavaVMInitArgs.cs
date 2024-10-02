using System;

namespace LibRyujinx.Jni.Values
{
    public readonly struct JavaVMInitArgs
    {
        internal Int32 Version { get; init; }
        internal Int32 OptionsLenght { get; init; }
        internal IntPtr Options { get; init; }
        internal Boolean IgnoreUnrecognized { get; init; }
    }
}
