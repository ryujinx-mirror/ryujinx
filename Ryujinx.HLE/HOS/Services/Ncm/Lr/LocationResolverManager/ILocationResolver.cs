using LibHac.FsSystem.NcaUtils;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using System.Text;

using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Ncm.Lr.LocationResolverManager
{
    class ILocationResolver : IpcService
    {
        private StorageId _storageId;

        public ILocationResolver(StorageId storageId)
        {
            _storageId = storageId;
        }

        [CommandHipc(0)]
        // ResolveProgramPath()
        public ResultCode ResolveProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, NcaContentType.Program))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.ProgramLocationEntryNotFound;
            }
        }

        [CommandHipc(1)]
        // RedirectProgramPath()
        public ResultCode RedirectProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 0, NcaContentType.Program);

            return ResultCode.Success;
        }

        [CommandHipc(2)]
        // ResolveApplicationControlPath()
        public ResultCode ResolveApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, NcaContentType.Control))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandHipc(3)]
        // ResolveApplicationHtmlDocumentPath()
        public ResultCode ResolveApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, NcaContentType.Manual))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandHipc(4)]
        // ResolveDataPath()
        public ResultCode ResolveDataPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, NcaContentType.Data) || ResolvePath(context, titleId, NcaContentType.PublicData))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandHipc(5)]
        // RedirectApplicationControlPath()
        public ResultCode RedirectApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Control);

            return ResultCode.Success;
        }

        [CommandHipc(6)]
        // RedirectApplicationHtmlDocumentPath()
        public ResultCode RedirectApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Manual);

            return ResultCode.Success;
        }

        [CommandHipc(7)]
        // ResolveApplicationLegalInformationPath()
        public ResultCode ResolveApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, NcaContentType.Manual))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandHipc(8)]
        // RedirectApplicationLegalInformationPath()
        public ResultCode RedirectApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Manual);

            return ResultCode.Success;
        }

        [CommandHipc(9)]
        // Refresh()
        public ResultCode Refresh(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return ResultCode.Success;
        }

        [CommandHipc(10)]
        // SetProgramNcaPath2()
        public ResultCode SetProgramNcaPath2(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Program);

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // ClearLocationResolver2()
        public ResultCode ClearLocationResolver2(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return ResultCode.Success;
        }

        [CommandHipc(12)]
        // DeleteProgramNcaPath()
        public ResultCode DeleteProgramNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, NcaContentType.Program);

            return ResultCode.Success;
        }

        [CommandHipc(13)]
        // DeleteControlNcaPath()
        public ResultCode DeleteControlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, NcaContentType.Control);

            return ResultCode.Success;
        }

        [CommandHipc(14)]
        // DeleteDocHtmlNcaPath()
        public ResultCode DeleteDocHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, NcaContentType.Manual);

            return ResultCode.Success;
        }

        [CommandHipc(15)]
        // DeleteInfoHtmlNcaPath()
        public ResultCode DeleteInfoHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, NcaContentType.Manual);

            return ResultCode.Success;
        }

        private void RedirectPath(ServiceCtx context, long titleId, int flag, NcaContentType contentType)
        {
            string        contentPath = ReadUtf8String(context);
            LocationEntry newLocation = new LocationEntry(contentPath, flag, titleId, contentType);

            context.Device.System.ContentManager.RedirectLocation(newLocation, _storageId);
        }

        private bool ResolvePath(ServiceCtx context, long titleId, NcaContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string         contentPath    = contentManager.GetInstalledContentPath(titleId, _storageId, NcaContentType.Program);

            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                long position = context.Request.RecvListBuff[0].Position;
                long size     = context.Request.RecvListBuff[0].Size;

                byte[] contentPathBuffer = Encoding.UTF8.GetBytes(contentPath);

                context.Memory.Write((ulong)position, contentPathBuffer);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void DeleteContentPath(ServiceCtx context, long titleId, NcaContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string         contentPath    = contentManager.GetInstalledContentPath(titleId, _storageId, NcaContentType.Manual);

            contentManager.ClearEntry(titleId, NcaContentType.Manual, _storageId);
        }
    }
}
