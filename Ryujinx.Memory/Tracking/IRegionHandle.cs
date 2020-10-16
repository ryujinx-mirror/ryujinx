using System;

namespace Ryujinx.Memory.Tracking
{
    public interface IRegionHandle : IDisposable
    {
        bool Dirty { get; }

        ulong Address { get; }
        ulong Size { get; }
        ulong EndAddress { get; }

        void Reprotect();
        void RegisterAction(RegionSignal action);
    }
}
