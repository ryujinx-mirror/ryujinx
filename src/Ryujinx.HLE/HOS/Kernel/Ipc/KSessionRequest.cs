using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Ipc
{
    class KSessionRequest
    {
        public KBufferDescriptorTable BufferDescriptorTable { get; }

        public KThread ClientThread { get; }

        public KProcess ServerProcess { get; set; }

        public KWritableEvent AsyncEvent { get; }

        public ulong CustomCmdBuffAddr { get; }
        public ulong CustomCmdBuffSize { get; }

        public KSessionRequest(
            KThread clientThread,
            ulong customCmdBuffAddr,
            ulong customCmdBuffSize,
            KWritableEvent asyncEvent = null)
        {
            ClientThread = clientThread;
            CustomCmdBuffAddr = customCmdBuffAddr;
            CustomCmdBuffSize = customCmdBuffSize;
            AsyncEvent = asyncEvent;

            BufferDescriptorTable = new KBufferDescriptorTable();
        }
    }
}
