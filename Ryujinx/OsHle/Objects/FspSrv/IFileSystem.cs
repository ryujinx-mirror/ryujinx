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
            //TODO: implement.
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  0, CreateFile   },
                {  1, DeleteFile   },
                {  2, CreateDirectory },
                {  3, DeleteDirectory },
                {  4, DeleteDirectoryRecursively },
                {  5, RenameFile },
                {  6, RenameDirectory },
                {  7, GetEntryType },
                {  8, OpenFile     },
                {  9, OpenDirectory },
                { 10, Commit       },
                //{ 11, GetFreeSpaceSize },
                //{ 12, GetTotalSpaceSize },
                //{ 13, CleanDirectoryRecursively },
                //{ 14, GetFileTimeStampRaw }
            };

            this.Path = Path;
        }

        public long CreateFile(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;
            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);
            ulong Mode = Context.RequestData.ReadUInt64();
            uint Size = Context.RequestData.ReadUInt32();
            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName != null)
            {
                FileStream NewFile = File.Create(FileName);
                NewFile.SetLength(Size);
                NewFile.Close();
                return 0;
            }

            //TODO: Correct error code.
            return -1;
        }

        public long DeleteFile(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;
            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);
            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName != null)
            {
                File.Delete(FileName);
                return 0;
            }

            //TODO: Correct error code.
            return -1;
        }

        public long CreateDirectory(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;
            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);
            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName != null)
            {
                Directory.CreateDirectory(FileName);
                return 0;
            }

            //TODO: Correct error code.
            return -1;
        }

        public long DeleteDirectory(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;
            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);
            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName != null)
            {
                Directory.Delete(FileName);
                return 0;
            }

            // TODO: Correct error code.
            return -1;
        }

        public long DeleteDirectoryRecursively(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;
            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);
            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName != null)
            {
                Directory.Delete(FileName, true); // recursive = true
                return 0;
            }

            // TODO: Correct error code.
            return -1;
        }

        public long RenameFile(ServiceCtx Context)
        {
            long OldPosition = Context.Request.PtrBuff[0].Position;
            long NewPosition = Context.Request.PtrBuff[0].Position;
            string OldName = AMemoryHelper.ReadAsciiString(Context.Memory, OldPosition);
            string NewName = AMemoryHelper.ReadAsciiString(Context.Memory, NewPosition);
            string OldFileName = Context.Ns.VFs.GetFullPath(Path, OldName);
            string NewFileName = Context.Ns.VFs.GetFullPath(Path, NewName);

            if (OldFileName != null && NewFileName != null)
            {
                File.Move(OldFileName, NewFileName);
                return 0;
            }

            // TODO: Correct error code.
            return -1;
        }

        public long RenameDirectory(ServiceCtx Context)
        {
            long OldPosition = Context.Request.PtrBuff[0].Position;
            long NewPosition = Context.Request.PtrBuff[0].Position;
            string OldName = AMemoryHelper.ReadAsciiString(Context.Memory, OldPosition);
            string NewName = AMemoryHelper.ReadAsciiString(Context.Memory, NewPosition);
            string OldDirName = Context.Ns.VFs.GetFullPath(Path, OldName);
            string NewDirName = Context.Ns.VFs.GetFullPath(Path, NewName);

            if (OldDirName != null && NewDirName != null)
            {
                Directory.Move(OldDirName, NewDirName);
                return 0;
            }

            // TODO: Correct error code.
            return -1;
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

            if (File.Exists(FileName))
            {
                FileStream Stream = new FileStream(FileName, FileMode.OpenOrCreate);
                MakeObject(Context, new IFile(Stream));

                return 0;
            }

            //TODO: Correct error code.
            return -1;
        }

        public long OpenDirectory(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            int FilterFlags = Context.RequestData.ReadInt32();

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string DirName = Context.Ns.VFs.GetFullPath(Path, Name);

            if(DirName != null)
            {
                if (Directory.Exists(DirName))
                {
                    MakeObject(Context, new IDirectory(DirName, FilterFlags));
                    return 0;
                }
                else
                {
                    // TODO: correct error code.
                    return -1;
                }
            }

            // TODO: Correct error code.
            return -1;
        }

        public long Commit(ServiceCtx Context)
        {
            return 0;
        }
    }
}