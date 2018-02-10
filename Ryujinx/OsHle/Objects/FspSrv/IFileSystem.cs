using ChocolArm64.Memory;
using Ryujinx.OsHle.Ipc;
using System.Collections.Generic;
using System.IO;

using static Ryujinx.OsHle.Objects.ObjHelper;

namespace Ryujinx.OsHle.Objects.FspSrv
{
    class IFileSystem : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private string Path;

        public IFileSystem(string Path)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  7, GetEntryType },
                {  8, OpenFile     },
                { 10, Commit       }
            };

            this.Path = Path;
        }

        public long GetEntryType(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName == null)
            {
                //TODO: Correct error code.
                return -1;
            }

            bool IsFile = File.Exists(FileName);

            Context.ResponseData.Write(IsFile ? 1 : 0);

            return 0;
        }

        public long OpenFile(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            int FilterFlags = Context.RequestData.ReadInt32();

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName == null)
            {
                //TODO: Correct error code.
                return -1;
            }

            FileStream Stream = new FileStream(FileName, FileMode.OpenOrCreate);

            MakeObject(Context, new IFile(Stream));

            return 0;
        }

        public long Commit(ServiceCtx Context)
        {
            return 0;
        }
    }
}