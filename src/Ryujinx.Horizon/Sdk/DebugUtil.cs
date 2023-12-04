using System.Diagnostics;

namespace Ryujinx.Horizon.Sdk
{
    static class DebugUtil
    {
        public static void Assert(bool condition)
        {
            Debug.Assert(condition);
        }
    }
}
