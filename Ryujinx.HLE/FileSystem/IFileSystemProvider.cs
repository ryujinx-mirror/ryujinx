using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.FspSrv;

namespace Ryujinx.HLE.FileSystem
{
    interface IFileSystemProvider
    {
        long CreateFile(string name, long size);

        long CreateDirectory(string name);

        long RenameFile(string oldName, string newName);

        long RenameDirectory(string oldName, string newName);

        DirectoryEntry[] GetEntries(string path);

        DirectoryEntry[] GetDirectories(string path);

        DirectoryEntry[] GetFiles(string path);

        long DeleteFile(string name);

        long DeleteDirectory(string name, bool recursive);

        bool FileExists(string name);

        bool DirectoryExists(string name);

        long OpenFile(string name, out IFile fileInterface);

        long OpenDirectory(string name, int filterFlags, out IDirectory directoryInterface);

        string GetFullPath(string name);

        long GetFreeSpace(ServiceCtx context);

        long GetTotalSpace(ServiceCtx context);
    }
}
