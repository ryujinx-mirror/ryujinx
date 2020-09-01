using LibHac;
using Ryujinx.HLE.HOS.Services.Fs.FileSystemProxy;

namespace Ryujinx.HLE.HOS.Services.Fs
{
    class IMultiCommitManager : IpcService // 6.0.0+
    {
        private LibHac.FsSrv.IMultiCommitManager _baseCommitManager;

        public IMultiCommitManager(LibHac.FsSrv.IMultiCommitManager baseCommitManager)
        {
            _baseCommitManager = baseCommitManager;
        }

        [Command(1)] // 6.0.0+
        // Add(object<nn::fssrv::sf::IFileSystem>)
        public ResultCode Add(ServiceCtx context)
        {
            IFileSystem fileSystem = GetObject<IFileSystem>(context, 0);

            Result result = _baseCommitManager.Add(fileSystem.GetBaseFileSystem());

            return (ResultCode)result.Value;
        }

        [Command(2)] // 6.0.0+
        // Commit()
        public ResultCode Commit(ServiceCtx context)
        {
            Result result = _baseCommitManager.Commit();

            return (ResultCode)result.Value;
        }
    }
}
