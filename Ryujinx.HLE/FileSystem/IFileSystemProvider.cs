using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.FspSrv;
using System;

namespace Ryujinx.HLE.FileSystem
{
    interface IFileSystemProvider
    {
        long CreateFile(string Name, long Size);

        long CreateDirectory(string Name);

        long RenameFile(string OldName, string NewName);

        long RenameDirectory(string OldName, string NewName);

        DirectoryEntry[] GetEntries(string Path);

        DirectoryEntry[] GetDirectories(string Path);

        DirectoryEntry[] GetFiles(string Path);

        long DeleteFile(string Name);

        long DeleteDirectory(string Name, bool Recursive);

        bool FileExists(string Name);

        bool DirectoryExists(string Name);

        long OpenFile(string Name, out IFile FileInterface);

        long OpenDirectory(string Name, int FilterFlags, out IDirectory DirectoryInterface);

        string GetFullPath(string Name);

        long GetFreeSpace(ServiceCtx Context);

        long GetTotalSpace(ServiceCtx Context);
    }
}
