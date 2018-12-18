using ChocolArm64.Memory;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Ipc;
using Ryujinx.HLE.HOS.Kernel.Process;
using System.IO;

namespace Ryujinx.HLE.HOS
{
    class ServiceCtx
    {
        public Switch        Device       { get; private set; }
        public KProcess      Process      { get; private set; }
        public MemoryManager Memory       { get; private set; }
        public KSession      Session      { get; private set; }
        public IpcMessage    Request      { get; private set; }
        public IpcMessage    Response     { get; private set; }
        public BinaryReader  RequestData  { get; private set; }
        public BinaryWriter  ResponseData { get; private set; }

        public ServiceCtx(
            Switch        device,
            KProcess      process,
            MemoryManager memory,
            KSession      session,
            IpcMessage    request,
            IpcMessage    response,
            BinaryReader  requestData,
            BinaryWriter  responseData)
        {
            Device       = device;
            Process      = process;
            Memory       = memory;
            Session      = session;
            Request      = request;
            Response     = response;
            RequestData  = requestData;
            ResponseData = responseData;
        }
    }
}