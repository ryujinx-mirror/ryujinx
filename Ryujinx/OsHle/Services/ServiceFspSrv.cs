using Ryujinx.OsHle.Objects.FspSrv;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Services
{
    static partial class Service
    {
        public static long FspSrvInitialize(ServiceCtx Context)
        {
            return 0;
        }

        public static long FspSrvMountSdCard(ServiceCtx Context)
        {
            MakeObject(Context, new IFileSystem(Context.Ns.VFs.GetSdCardPath()));

            return 0;
        }

        public static long FspSrvMountSaveData(ServiceCtx Context)
        {
            MakeObject(Context, new IFileSystem(Context.Ns.VFs.GetGameSavesPath()));

            return 0;
        }

        public static long FspSrvOpenDataStorageByCurrentProcess(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(Context.Ns.VFs.RomFs));

            return 0;
        }

        public static long FspSrvOpenRomStorage(ServiceCtx Context)
        {
            MakeObject(Context, new IStorage(Context.Ns.VFs.RomFs));

            return 0;
        }

        public static long FspSrvGetGlobalAccessLogMode(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }        
    }
}