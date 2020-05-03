using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System.IO;

namespace Ryujinx.HLE.HOS
{
    class ServiceCtx
    {
        public Switch         Device       { get; }
        public KProcess       Process      { get; }
        public MemoryManager  Memory       { get; }
        public KThread        Thread       { get; }
        public KClientSession Session      { get; }
        public IpcMessage     Request      { get; }
        public IpcMessage     Response     { get; }
        public BinaryReader   RequestData  { get; }
        public BinaryWriter   ResponseData { get; }

        public ServiceCtx(
            Switch         device,
            KProcess       process,
            MemoryManager  memory,
            KThread        thread,
            KClientSession session,
            IpcMessage     request,
            IpcMessage     response,
            BinaryReader   requestData,
            BinaryWriter   responseData)
        {
            Device       = device;
            Process      = process;
            Memory       = memory;
            Thread       = thread;
            Session      = session;
            Request      = request;
            Response     = response;
            RequestData  = requestData;
            ResponseData = responseData;
        }
    }
}