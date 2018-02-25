using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

using static Ryujinx.Core.OsHle.IpcServices.ObjHelper;

namespace Ryujinx.Core.OsHle.IpcServices.FspSrv
{
    class ServiceFspSrv : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceFspSrv()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {    1, Initialize                      },
                {   18, MountSdCard                     },
                {   51, MountSaveData                   },
                {  200, OpenDataStorageByCurrentProcess },
                {  203, OpenRomStorage                  },
                { 1005, GetGlobalAccessLogMode          }
            };
        }

        public long Initialize(ServiceCtx Context)
        {
            return 0;
        }

        public long MountSdCard(ServiceCtx Context)
        {
            MakeObject(Context, new IFileSystem(Context.Ns.VFs.GetSdCardPath()));

            return 0;
        }

        public long MountSaveData(ServiceCtx Context)
        {
            MakeObject(Context, new IFileSystem(Context.Ns.VFs.GetGameSavesPath()));

            return 0;
        }

        public long OpenDataStorageByCurrentProcess(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(Context.Ns.VFs.RomFs));

            return 0;
        }

        public long OpenRomStorage(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(Context.Ns.VFs.RomFs));

            return 0;
        }

        public long GetGlobalAccessLogMode(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }        
    }
}