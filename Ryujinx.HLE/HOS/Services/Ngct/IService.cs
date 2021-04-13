using System.Text;

namespace Ryujinx.HLE.HOS.Services.Ngct
{
    [Service("ngct:u")] // 9.0.0+
    class IService : IpcService
    {
        public IService(ServiceCtx context) { }

        [CommandHipc(0)]
        // Match(buffer<string, 9>) -> b8
        public ResultCode Match(ServiceCtx context)
        {
            return NgctServer.Match(context);
        }

        [CommandHipc(1)]
        // Filter(buffer<string, 9>) -> buffer<filtered_string, 10>
        public ResultCode Filter(ServiceCtx context)
        {
            return NgctServer.Filter(context);
        }
    }
}