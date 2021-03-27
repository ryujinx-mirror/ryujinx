using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    internal class ProcessTamperInfo
    {
        public KProcess Process { get; }
        public IEnumerable<string> BuildIds { get; }
        public IEnumerable<ulong> CodeAddresses { get; }
        public ulong HeapAddress { get; }

        public ProcessTamperInfo(KProcess process, IEnumerable<string> buildIds, IEnumerable<ulong> codeAddresses, ulong heapAddress)
        {
            Process = process;
            BuildIds = buildIds;
            CodeAddresses = codeAddresses;
            HeapAddress = heapAddress;
        }
    }
}