using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.FsCreator;
using LibHac.FsSystem;

namespace Ryujinx.HLE.FileSystem
{
    public class EncryptedFileSystemCreator : IEncryptedFileSystemCreator
    {
        public EncryptedFileSystemCreator() { }

        public Result Create(out ReferenceCountedDisposable<IFileSystem> encryptedFileSystem, ReferenceCountedDisposable<IFileSystem> baseFileSystem,
            EncryptedFsKeyId keyId, in EncryptionSeed encryptionSeed)
        {
            UnsafeHelpers.SkipParamInit(out encryptedFileSystem);

            if (keyId < EncryptedFsKeyId.Save || keyId > EncryptedFsKeyId.CustomStorage)
            {
                return ResultFs.InvalidArgument.Log();
            }

            // Force all-zero keys for now since people can open the emulator with different keys or sd seeds sometimes
            var fs = new AesXtsFileSystem(baseFileSystem, new byte[0x32], 0x4000);
            var aesFileSystem = new ReferenceCountedDisposable<IFileSystem>(fs);

            // This wrapper will handle deleting files that were created with different keys
            var wrappedFs = new ChangedEncryptionHandlingFileSystem(aesFileSystem);
            encryptedFileSystem = new ReferenceCountedDisposable<IFileSystem>(wrappedFs);

            return Result.Success;
        }
    }

    public class ChangedEncryptionHandlingFileSystem : ForwardingFileSystem
    {
        public ChangedEncryptionHandlingFileSystem(ReferenceCountedDisposable<IFileSystem> baseFileSystem) : base(baseFileSystem) { }

        protected override Result DoOpenFile(out IFile file, U8Span path, OpenMode mode)
        {
            UnsafeHelpers.SkipParamInit(out file);

            try
            {
                return base.DoOpenFile(out file, path, mode);
            }
            catch (HorizonResultException ex)
            {
                if (ResultFs.AesXtsFileHeaderInvalidKeys.Includes(ex.ResultValue))
                {
                    Result rc = DeleteFile(path);
                    if (rc.IsFailure()) return rc;

                    return base.DoOpenFile(out file, path, mode);
                }

                throw;
            }
        }
    }
}
