using System;

namespace LibRyujinx.Jni
{
    public enum JReferenceType : Int32
    {
        InvalidRefType = 0,
        LocalRefType = 1,
        GlobalRefType = 2,
        WeakGlobalRefType = 3
    }
}
