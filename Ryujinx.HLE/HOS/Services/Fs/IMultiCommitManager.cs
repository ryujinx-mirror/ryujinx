using LibHac;
using Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class IMultiCommitManager : DisposableIpcService // 6.0.0+
    {
        private ReferenceCountedDisposable<LibHac.FsSrv.Sf.IMultiCommitManager> _baseCommitManager;

        public IMultiCommitManager(ReferenceCountedDisposable<LibHac.FsSrv.Sf.IMultiCommitManager> baseCommitManager)
        {
            _baseCommitManager = baseCommitManager;
        }

        [CommandHipc(1)] // 6.0.0+
        // Add(object<nn::fssrv::sf::IFileSystem>)
        public ResultCode Add(ServiceCtx context)
        {
            IFileSystem fileSystem = GetObject<IFileSystem>(context, 0);

            Result result = _baseCommitManager.Target.Add(fileSystem.GetBaseFileSystem());

            return (ResultCode)result.Value;
        }

        [CommandHipc(2)] // 6.0.0+
        // Commit()
        public ResultCode Commit(ServiceCtx context)
        {
            Result result = _baseCommitManager.Target.Commit();

            return (ResultCode)result.Value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _baseCommitManager?.Dispose();
            }
        }
    }
}
