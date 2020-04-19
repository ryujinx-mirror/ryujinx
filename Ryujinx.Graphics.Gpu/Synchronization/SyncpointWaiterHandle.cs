using System;

namespace Ryujinx.Graphics.Gpu.Synchronization
{
    public class SyncpointWaiterHandle
    {
        internal uint   Threshold;
        internal Action Callback;
    }
}
