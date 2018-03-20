using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.NvServices
{
    class NvMapFb
    {
        private List<long> BufferOffs;

        public NvMapFb()
        {
            BufferOffs = new List<long>();
        }

        public void AddBufferOffset(long Offset)
        {
            BufferOffs.Add(Offset);
        }

        public bool HasBufferOffset(int Index)
        {
            if ((uint)Index >= BufferOffs.Count)
            {
                return false;
            }

            return true;
        }

        public long GetBufferOffset(int Index)
        {
            if ((uint)Index >= BufferOffs.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }

            return BufferOffs[Index];
        }
    }
}