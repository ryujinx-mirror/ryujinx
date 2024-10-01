using System;

namespace ARMeilleure.Translation
{
    delegate void DispatcherFunction(IntPtr nativeContext, ulong startAddress);
    delegate ulong WrapperFunction(IntPtr nativeContext, ulong startAddress);
}
