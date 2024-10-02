using System;

namespace LibRyujinx.Jni
{
    public enum JResult : Int32
    {
        Ok = 0,
        Error = -1,
        DetachedThreadError = -2,
        VersionError = -3,
        MemoryError = -4,
        ExitingVMError = -5,
        InvalidArgumentsError = -6,
    }
}
