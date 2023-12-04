using LibHac;
using LibHac.Common;
using Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class IMultiCommitManager : DisposableIpcService // 6.0.0+
    {
        private SharedRef<LibHac.FsSrv.Sf.IMultiCommitManager> _baseCommitManager;

        public IMultiCommitManager(ref SharedRef<LibHac.FsSrv.Sf.IMultiCommitManager> baseCommitManager)
        {
            _baseCommitManager = SharedRef<LibHac.FsSrv.Sf.IMultiCommitManager>.CreateMove(ref baseCommitManager);
        }

        [CommandCmif(1)] // 6.0.0+
        // Add(object<nn::fssrv::sf::IFileSystem>)
        public ResultCode Add(ServiceCtx context)
        {
            using SharedRef<LibHac.FsSrv.Sf.IFileSystem> fileSystem = GetObject<IFileSystem>(context, 0).GetBaseFileSystem();

            Result result = _baseCommitManager.Get.Add(ref fileSystem.Ref);

            return (ResultCode)result.Value;
        }

        [CommandCmif(2)] // 6.0.0+
        // Commit()
        public ResultCode Commit(ServiceCtx context)
        {
            Result result = _baseCommitManager.Get.Commit();

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseCommitManager.Destroy();
            }
        }
    }
}
