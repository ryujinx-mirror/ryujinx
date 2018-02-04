using ChocolArm64.Memory;
using System.IO;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Objects
{
    class FspSrvIFileSystem
    {
        public string FilePath { get; private set; }

        public FspSrvIFileSystem(string Path)
        {
            this.FilePath = Path;
        }

        public static long GetEntryType(ServiceCtx Context)
        {
            FspSrvIFileSystem FileSystem = Context.GetObject<FspSrvIFileSystem>();

            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(FileSystem.FilePath, Name);

            if (FileName == null)
            {
                //TODO: Correct error code.
                return -1;
            }

            bool IsFile = File.Exists(FileName);

            Context.ResponseData.Write(IsFile ? 1 : 0);

            return 0;
        }

        public static long OpenFile(ServiceCtx Context)
        {
            FspSrvIFileSystem FileSystem = Context.GetObject<FspSrvIFileSystem>();

            long Position = Context.Request.PtrBuff[0].Position;

            int FilterFlags = Context.RequestData.ReadInt32();

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(FileSystem.FilePath, Name);

            if (FileName == null)
            {
                //TODO: Correct error code.
                return -1;
            }

            FileStream Stream = new FileStream(FileName, FileMode.OpenOrCreate);

            MakeObject(Context, new FspSrvIFile(Stream));

            return 0;
        }

        public static long Commit(ServiceCtx Context)
        {
            return 0;
        }
    }
}