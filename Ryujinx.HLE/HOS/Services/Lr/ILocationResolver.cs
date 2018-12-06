using LibHac;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.Utilities.StringUtils;

namespace Ryujinx.HLE.HOS.Services.Lr
{
    class ILocationResolver : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private StorageId _storageId;

        public ILocationResolver(StorageId storageId)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  ResolveProgramPath                      },
                { 1,  RedirectProgramPath                     },
                { 2,  ResolveApplicationControlPath           },
                { 3,  ResolveApplicationHtmlDocumentPath      },
                { 4,  ResolveDataPath                         },
                { 5,  RedirectApplicationControlPath          },
                { 6,  RedirectApplicationHtmlDocumentPath     },
                { 7,  ResolveApplicationLegalInformationPath  },
                { 8,  RedirectApplicationLegalInformationPath },
                { 9,  Refresh                                 },
                { 10, SetProgramNcaPath2                      },
                { 11, ClearLocationResolver2                  },
                { 12, DeleteProgramNcaPath                    },
                { 13, DeleteControlNcaPath                    },
                { 14, DeleteDocHtmlNcaPath                    },
                { 15, DeleteInfoHtmlNcaPath                   }
            };

            _storageId = storageId;
        }

        // DeleteInfoHtmlNcaPath()
        public long DeleteInfoHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Manual);

            return 0;
        }

        // DeleteDocHtmlNcaPath()
        public long DeleteDocHtmlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Manual);

            return 0;
        }

        // DeleteControlNcaPath()
        public long DeleteControlNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Control);

            return 0;
        }

        // DeleteProgramNcaPath()
        public long DeleteProgramNcaPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            DeleteContentPath(context, titleId, ContentType.Program);

            return 0;
        }

        // ClearLocationResolver2()
        public long ClearLocationResolver2(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return 0;
        }

        // SetProgramNcaPath2()
        public long SetProgramNcaPath2(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Program);

            return 0;
        }

        // RedirectApplicationControlPath()
        public long RedirectApplicationControlPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Control);

            return 0;
        }

        // RedirectApplicationHtmlDocumentPath()
        public long RedirectApplicationHtmlDocumentPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Manual);

            return 0;
        }

        // RedirectApplicationLegalInformationPath()
        public long RedirectApplicationLegalInformationPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 1, ContentType.Manual);

            return 0;
        }

        // ResolveDataPath()
        public long ResolveDataPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            if (ResolvePath(context, titleId, ContentType.Data) || ResolvePath(context, titleId, ContentType.AocData))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

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

        // RedirectProgramPath()
        public long RedirectProgramPath(ServiceCtx context)
        {
            long titleId = context.RequestData.ReadInt64();

            RedirectPath(context, titleId, 0, ContentType.Program);

            return 0;
        }

        // Refresh()
        public long Refresh(ServiceCtx context)
        {
            context.Device.System.ContentManager.RefreshEntries(_storageId, 1);

            return 0;
        }

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
