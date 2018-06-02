using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Am
{
    class ILibraryAppletCreator : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ILibraryAppletCreator()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  CreateLibraryApplet },
                { 10, CreateStorage       }
            };
        }

        public long CreateLibraryApplet(ServiceCtx Context)
        {
            MakeObject(Context, new ILibraryAppletAccessor());

            return 0;
        }

        public long CreateStorage(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(StorageHelper.MakeLaunchParams()));

            return 0;
        }
    }
}