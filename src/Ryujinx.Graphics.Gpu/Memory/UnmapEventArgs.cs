using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Memory
{
    public class UnmapEventArgs
    {
        public ulong Address { get; }
        public ulong Size { get; }
        public List<Action> RemapActions { get; private set; }

        public UnmapEventArgs(ulong address, ulong size)
        {
            Address = address;
            Size = size;
        }

        public void AddRemapAction(Action action)
        {
            RemapActions ??= new List<Action>();
            RemapActions.Add(action);
        }
    }
}
