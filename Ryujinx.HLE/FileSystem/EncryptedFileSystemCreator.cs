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
            encryptedFileSystem = new ReferenceCountedDisposable<IFileSystem>(fs);

            return Result.Success;
        }
    }
}
