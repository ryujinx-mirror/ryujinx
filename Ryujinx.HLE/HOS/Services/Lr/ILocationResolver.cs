using LibHac.Fs.NcaUtils;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Lr
{
    class ILocationResolver : IpcService
    {
        private StorageId _storageId;

        public ILocationResolver(StorageId storageId)
        {
            _storageId = storageId;
        }

        [Command(0)]
        // ResolveProgramPath()
        public long ResolveProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Program))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.ProgramLocationEntryNotFound);
            }
        }

        [Command(1)]
        // RedirectProgramPath()
        public long RedirectProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 0, ContentType.Program);

            return 0;
        }

        [Command(2)]
        // ResolveApplicationControlPath()
        public long ResolveApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Control))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        [Command(3)]
        // ResolveApplicationHtmlDocumentPath()
        public long ResolveApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Manual))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        [Command(4)]
        // ResolveDataPath()
        public long ResolveDataPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Data) || ResolvePath(context, titleId, ContentType.PublicData))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        [Command(5)]
        // RedirectApplicationControlPath()
        public long RedirectApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Control);

            return 0;
        }

        [Command(6)]
        // RedirectApplicationHtmlDocumentPath()
        public long RedirectApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Manual);

            return 0;
        }

        [Command(7)]
        // ResolveApplicationLegalInformationPath()
        public long ResolveApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Manual))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        [Command(8)]
        // RedirectApplicationLegalInformationPath()
        public long RedirectApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Manual);

            return 0;
        }

        [Command(9)]
        // Refresh()
        public long Refresh(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return 0;
        }

        [Command(10)]
        // SetProgramNcaPath2()
        public long SetProgramNcaPath2(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Program);

            return 0;
        }

        [Command(11)]
        // ClearLocationResolver2()
        public long ClearLocationResolver2(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return 0;
        }

        [Command(12)]
        // DeleteProgramNcaPath()
        public long DeleteProgramNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Program);

            return 0;
        }

        [Command(13)]
        // DeleteControlNcaPath()
        public long DeleteControlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Control);

            return 0;
        }

        [Command(14)]
        // DeleteDocHtmlNcaPath()
        public long DeleteDocHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Manual);

            return 0;
        }

        [Command(15)]
        // DeleteInfoHtmlNcaPath()
        public long DeleteInfoHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Manual);

            return 0;
        }

        private void RedirectPath(ServiceCtx context, long titleId, int flag, ContentType contentType)
        {
            string        contentPath = ReadUtf8String(context);
            LocationEntry newLocation = new LocationEntry(contentPath, flag, titleId, contentType);

            context.Device.System.ContentManager.RedirectLocation(newLocation, _storageId);
        }

        private bool ResolvePath(ServiceCtx context, long titleId,ContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string         contentPath    = contentManager.GetInstalledContentPath(titleId, _storageId, ContentType.Program);

            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                long position = context.Request.RecvListBuff[0].Position;
                long size     = context.Request.RecvListBuff[0].Size;

                byte[] contentPathBuffer = Encoding.UTF8.GetBytes(contentPath);

                context.Memory.WriteBytes(position, contentPathBuffer);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void DeleteContentPath(ServiceCtx context, long titleId, ContentType contentType)
        {
            ContentManager contentManager = context.Device.System.ContentManager;
            string         contentPath    = contentManager.GetInstalledContentPath(titleId, _storageId, ContentType.Manual);

            contentManager.ClearEntry(titleId, ContentType.Manual, _storageId);
        }
    }
}
