using ChocolArm64.Memory;
using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.IO;

using static Ryujinx.Core.OsHle.Objects.ErrorCode;
using static Ryujinx.Core.OsHle.Objects.ObjHelper;

namespace Ryujinx.Core.OsHle.Objects.FspSrv
{
    class IFileSystem : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private HashSet<string> OpenPaths;

        private string Path;

        public IFileSystem(string Path)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {  0, CreateFile                 },
                {  1, DeleteFile                 },
                {  2, CreateDirectory            },
                {  3, DeleteDirectory            },
                {  4, DeleteDirectoryRecursively },
                {  5, RenameFile                 },
                {  6, RenameDirectory            },
                {  7, GetEntryType               },
                {  8, OpenFile                   },
                {  9, OpenDirectory              },
                { 10, Commit                     },
                { 11, GetFreeSpaceSize           },
                { 12, GetTotalSpaceSize          },
                //{ 13, CleanDirectoryRecursively  },
                //{ 14, GetFileTimeStampRaw        }
            };

            OpenPaths = new HashSet<string>();

            this.Path = Path;
        }

        public long CreateFile(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            long Mode = Context.RequestData.ReadInt64();
            int  Size = Context.RequestData.ReadInt32();

            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (FileName == null)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (File.Exists(FileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(FileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            using (FileStream NewFile = File.Create(FileName))
            {
                NewFile.SetLength(Size);
            }

            return 0;
        }

        public long DeleteFile(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (!File.Exists(FileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(FileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            File.Delete(FileName);

            return 0;
        }

        public long CreateDirectory(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string DirName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (DirName == null)
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (Directory.Exists(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            Directory.CreateDirectory(DirName);

            return 0;
        }

        public long DeleteDirectory(ServiceCtx Context)
        {
            return DeleteDirectory(Context, false);
        }

        public long DeleteDirectoryRecursively(ServiceCtx Context)
        {
            return DeleteDirectory(Context, true);
        }

        private long DeleteDirectory(ServiceCtx Context, bool Recursive)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string DirName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (!Directory.Exists(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            Directory.Delete(DirName, Recursive);

            return 0;
        }

        public long RenameFile(ServiceCtx Context)
        {
            long OldPosition = Context.Request.PtrBuff[0].Position;
            long NewPosition = Context.Request.PtrBuff[0].Position;

            string OldName = AMemoryHelper.ReadAsciiString(Context.Memory, OldPosition);
            string NewName = AMemoryHelper.ReadAsciiString(Context.Memory, NewPosition);

            string OldFileName = Context.Ns.VFs.GetFullPath(Path, OldName);
            string NewFileName = Context.Ns.VFs.GetFullPath(Path, NewName);

            if (!File.Exists(OldFileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (File.Exists(NewFileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(OldFileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            File.Move(OldFileName, NewFileName);

            return 0;
        }

        public long RenameDirectory(ServiceCtx Context)
        {
            long OldPosition = Context.Request.PtrBuff[0].Position;
            long NewPosition = Context.Request.PtrBuff[0].Position;

            string OldName = AMemoryHelper.ReadAsciiString(Context.Memory, OldPosition);
            string NewName = AMemoryHelper.ReadAsciiString(Context.Memory, NewPosition);

            string OldDirName = Context.Ns.VFs.GetFullPath(Path, OldName);
            string NewDirName = Context.Ns.VFs.GetFullPath(Path, NewName);

            if (!Directory.Exists(OldDirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (Directory.Exists(NewDirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyExists);
            }

            if (IsPathAlreadyInUse(OldDirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            Directory.Move(OldDirName, NewDirName);

            return 0;
        }

        public long GetEntryType(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (File.Exists(FileName))
            {
                Context.ResponseData.Write(1);
            }
            else if (Directory.Exists(FileName))
            {
                Context.ResponseData.Write(0);
            }
            else
            {
                Context.ResponseData.Write(0);

                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            return 0;
        }

        public long OpenFile(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            int FilterFlags = Context.RequestData.ReadInt32();

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string FileName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (!File.Exists(FileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(FileName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            FileStream Stream = new FileStream(FileName, FileMode.Open);

            MakeObject(Context, new IFile(Stream, FileName));

            return 0;
        }

        public long OpenDirectory(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            int FilterFlags = Context.RequestData.ReadInt32();

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            string DirName = Context.Ns.VFs.GetFullPath(Path, Name);

            if (!Directory.Exists(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathDoesNotExist);
            }

            if (IsPathAlreadyInUse(DirName))
            {
                return MakeError(ErrorModule.Fs, FsErr.PathAlreadyInUse);
            }

            IDirectory DirInterface = new IDirectory(DirName, FilterFlags);

            DirInterface.Disposed += RemoveDirectoryInUse;

            lock (OpenPaths)
            {
                OpenPaths.Add(DirName);
            }

            MakeObject(Context, DirInterface);

            return 0;
        }

        public long Commit(ServiceCtx Context)
        {
            return 0;
        }

        public long GetFreeSpaceSize(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            Context.ResponseData.Write(Context.Ns.VFs.GetDrive().AvailableFreeSpace);

            return 0;
        }

        public long GetTotalSpaceSize(ServiceCtx Context)
        {
            long Position = Context.Request.PtrBuff[0].Position;

            string Name = AMemoryHelper.ReadAsciiString(Context.Memory, Position);

            Context.ResponseData.Write(Context.Ns.VFs.GetDrive().TotalSize);

            return 0;
        }

        private bool IsPathAlreadyInUse(string Path)
        {
            lock (OpenPaths)
            {
                return OpenPaths.Contains(Path);
            }
        }

        private void RemoveFileInUse(object sender, EventArgs e)
        {
            IFile FileInterface = (IFile)sender;

            lock (OpenPaths)
            {
                FileInterface.Disposed -= RemoveDirectoryInUse;

                OpenPaths.Remove(FileInterface.HostPath);
            }
        }

        private void RemoveDirectoryInUse(object sender, EventArgs e)
        {
            IDirectory DirInterface = (IDirectory)sender;

            lock (OpenPaths)
            {
                DirInterface.Disposed -= RemoveDirectoryInUse;

                OpenPaths.Remove(DirInterface.HostPath);
            }
        }
    }
}