using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.FspSrv
{
    class IFileSystemProxy : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IFileSystemProxy()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 1,    SetCurrentProcess                    },
                { 18,   OpenSdCardFileSystem                 },
                { 22,   CreateSaveDataFileSystem             },
                { 51,   OpenSaveDataFileSystem               },
                { 200,  OpenDataStorageByCurrentProcess      },
                { 203,  OpenPatchDataStorageByCurrentProcess },
                { 1005, GetGlobalAccessLogMode               }
            };
        }

        public long SetCurrentProcess(ServiceCtx Context)
        {
            return 0;
        }

        public long OpenSdCardFileSystem(ServiceCtx Context)
        {
            MakeObject(Context, new IFileSystem(Context.Device.FileSystem.GetSdCardPath()));

            return 0;
        }

        public long CreateSaveDataFileSystem(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceFs, "Stubbed.");

            return 0;
        }

        public long OpenSaveDataFileSystem(ServiceCtx Context)
        {
            MakeObject(Context, new IFileSystem(Context.Device.FileSystem.GetGameSavesPath()));

            return 0;
        }

        public long OpenDataStorageByCurrentProcess(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(Context.Device.FileSystem.RomFs));

            return 0;
        }

        public long OpenPatchDataStorageByCurrentProcess(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(Context.Device.FileSystem.RomFs));

            return 0;
        }

        public long GetGlobalAccessLogMode(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }
    }
}