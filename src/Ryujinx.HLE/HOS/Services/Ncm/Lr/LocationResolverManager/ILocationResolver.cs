using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.HLE.FileSystem;
using System.Text;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Ncm.Lr.LocationResolverManager
{
    class ILocationResolver : IpcService
    {
        private readonly StorageId _storageId;

        public ILocationResolver(StorageId storageId)
        {
            _storageId = storageId;
        }

        [CommandCmif(0)]
        // ResolveProgramPath(u64 titleId)
        public ResultCode ResolveProgramPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            if (ResolvePath(context, titleId, NcaContentType.Program))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.ProgramLocationEntryNotFound;
            }
        }

        [CommandCmif(1)]
        // RedirectProgramPath(u64 titleId)
        public ResultCode RedirectProgramPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            RedirectPath(context, titleId, 0, NcaContentType.Program);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // ResolveApplicationControlPath(u64 titleId)
        public ResultCode ResolveApplicationControlPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            if (ResolvePath(context, titleId, NcaContentType.Control))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandCmif(3)]
        // ResolveApplicationHtmlDocumentPath(u64 titleId)
        public ResultCode ResolveApplicationHtmlDocumentPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            if (ResolvePath(context, titleId, NcaContentType.Manual))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandCmif(4)]
        // ResolveDataPath(u64 titleId)
        public ResultCode ResolveDataPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            if (ResolvePath(context, titleId, NcaContentType.Data) || ResolvePath(context, titleId, NcaContentType.PublicData))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandCmif(5)]
        // RedirectApplicationControlPath(u64 titleId)
        public ResultCode RedirectApplicationControlPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Control);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // RedirectApplicationHtmlDocumentPath(u64 titleId)
        public ResultCode RedirectApplicationHtmlDocumentPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Manual);

            return ResultCode.Success;
        }

        [CommandCmif(7)]
        // ResolveApplicationLegalInformationPath(u64 titleId)
        public ResultCode ResolveApplicationLegalInformationPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            if (ResolvePath(context, titleId, NcaContentType.Manual))
            {
                return ResultCode.Success;
            }
            else
            {
                return ResultCode.AccessDenied;
            }
        }

        [CommandCmif(8)]
        // RedirectApplicationLegalInformationPath(u64 titleId)
        public ResultCode RedirectApplicationLegalInformationPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Manual);

            return ResultCode.Success;
        }

        [CommandCmif(9)]
        // Refresh()
        public ResultCode Refresh(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // SetProgramNcaPath2(u64 titleId)
        public ResultCode SetProgramNcaPath2(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            RedirectPath(context, titleId, 1, NcaContentType.Program);

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // ClearLocationResolver2()
        public ResultCode ClearLocationResolver2(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // DeleteProgramNcaPath(u64 titleId)
        public ResultCode DeleteProgramNcaPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            DeleteContentPath(context, titleId, NcaContentType.Program);

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // DeleteControlNcaPath(u64 titleId)
        public ResultCode DeleteControlNcaPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            DeleteContentPath(context, titleId, NcaContentType.Control);

            return ResultCode.Success;
        }

        [CommandCmif(14)]
        // DeleteDocHtmlNcaPath(u64 titleId)
        public ResultCode DeleteDocHtmlNcaPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            DeleteContentPath(context, titleId, NcaContentType.Manual);

            return ResultCode.Success;
        }

        [CommandCmif(15)]
        // DeleteInfoHtmlNcaPath(u64 titleId)
        public ResultCode DeleteInfoHtmlNcaPath(ServiceCtx context)
        {
            ulong titleId = context.RequestData.ReadUInt64();

            DeleteContentPath(context, titleId, NcaContentType.Manual);

            return ResultCode.Success;
        }

        private void RedirectPath(ServiceCtx context, ulong titleId, int flag, NcaContentType contentType)
        {
            string contentPath = ReadUtf8String(context);
            LocationEntry newLocation = new(contentPath, flag, titleId, contentType);

            context.Device.System.ContentManager.RedirectLocation(newLocation, _storageId);
        }

        private bool ResolvePath(ServiceCtx context, ulong titleId, NcaContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string contentPath = contentManager.GetInstalledContentPath(titleId, _storageId, NcaContentType.Program);

            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                ulong position = context.Request.RecvListBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
                ulong size = context.Request.RecvListBuff[0].Size;
#pragma warning restore IDE0059

                byte[] contentPathBuffer = Encoding.UTF8.GetBytes(contentPath);

                context.Memory.Write(position, contentPathBuffer);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void DeleteContentPath(ServiceCtx context, ulong titleId, NcaContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            string contentPath = contentManager.GetInstalledContentPath(titleId, _storageId, NcaContentType.Manual);
#pragma warning restore IDE0059

            contentManager.ClearEntry(titleId, NcaContentType.Manual, _storageId);
        }
    }
}
