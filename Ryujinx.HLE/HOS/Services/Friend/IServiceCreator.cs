using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    class IServiceCreator : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IServiceCreator()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CreateFriendService }
            };
        }

        public static long CreateFriendService(ServiceCtx context)
        {
            MakeObject(context, new IFriendService());

            return 0;
        }
    }
}