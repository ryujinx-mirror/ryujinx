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
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private StorageId StorageId;

        public ILocationResolver(StorageId StorageId)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
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

            this.StorageId = StorageId;
        }

        // DeleteInfoHtmlNcaPath()
        public long DeleteInfoHtmlNcaPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            DeleteContentPath(Context, TitleId, ContentType.Manual);

            return 0;
        }

        // DeleteDocHtmlNcaPath()
        public long DeleteDocHtmlNcaPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            DeleteContentPath(Context, TitleId, ContentType.Manual);

            return 0;
        }

        // DeleteControlNcaPath()
        public long DeleteControlNcaPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            DeleteContentPath(Context, TitleId, ContentType.Control);

            return 0;
        }

        // DeleteProgramNcaPath()
        public long DeleteProgramNcaPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            DeleteContentPath(Context, TitleId, ContentType.Program);

            return 0;
        }

        // ClearLocationResolver2()
        public long ClearLocationResolver2(ServiceCtx Context)
        {
            Context.Device.System.ContentManager.RefreshEntries(StorageId, 1);

            return 0;
        }

        // SetProgramNcaPath2()
        public long SetProgramNcaPath2(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            RedirectPath(Context, TitleId, 1, ContentType.Program);

            return 0;
        }

        // RedirectApplicationControlPath()
        public long RedirectApplicationControlPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            RedirectPath(Context, TitleId, 1, ContentType.Control);

            return 0;
        }

        // RedirectApplicationHtmlDocumentPath()
        public long RedirectApplicationHtmlDocumentPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            RedirectPath(Context, TitleId, 1, ContentType.Manual);

            return 0;
        }

        // RedirectApplicationLegalInformationPath()
        public long RedirectApplicationLegalInformationPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            RedirectPath(Context, TitleId, 1, ContentType.Manual);

            return 0;
        }

        // ResolveDataPath()
        public long ResolveDataPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            if (ResolvePath(Context, TitleId, ContentType.Data) || ResolvePath(Context, TitleId, ContentType.AocData))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        // ResolveApplicationHtmlDocumentPath()
        public long ResolveApplicationHtmlDocumentPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            if (ResolvePath(Context, TitleId, ContentType.Manual))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        // ResolveApplicationLegalInformationPath()
        public long ResolveApplicationLegalInformationPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            if (ResolvePath(Context, TitleId, ContentType.Manual))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        // ResolveApplicationControlPath()
        public long ResolveApplicationControlPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            if (ResolvePath(Context, TitleId, ContentType.Control))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.AccessDenied);
            }
        }

        // RedirectProgramPath()
        public long RedirectProgramPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            RedirectPath(Context, TitleId, 0, ContentType.Program);

            return 0;
        }

        // Refresh()
        public long Refresh(ServiceCtx Context)
        {
            Context.Device.System.ContentManager.RefreshEntries(StorageId, 1);

            return 0;
        }

        // ResolveProgramPath()
        public long ResolveProgramPath(ServiceCtx Context)
        {
            long TitleId = Context.RequestData.ReadInt64();

            if (ResolvePath(Context, TitleId, ContentType.Program))
            {
                return 0;
            }
            else
            {
                return MakeError(ErrorModule.Lr, LrErr.ProgramLocationEntryNotFound);
            }
        }

        private void RedirectPath(ServiceCtx Context, long TitleId, int Flag, ContentType ContentType)
        {
            string        ContentPath = ReadUtf8String(Context);
            LocationEntry NewLocation = new LocationEntry(ContentPath, Flag, TitleId, ContentType);

            Context.Device.System.ContentManager.RedirectLocation(NewLocation, StorageId);
        }

        private bool ResolvePath(ServiceCtx Context, long TitleId,ContentType ContentType)
        {
            ContentManager ContentManager = Context.Device.System.ContentManager;
            string         ContentPath    = ContentManager.GetInstalledContentPath(TitleId, StorageId, ContentType.Program);

            if (!string.IsNullOrWhiteSpace(ContentPath))
            {
                long Position = Context.Request.RecvListBuff[0].Position;
                long Size     = Context.Request.RecvListBuff[0].Size;

                byte[] ContentPathBuffer = Encoding.UTF8.GetBytes(ContentPath);

                Context.Memory.WriteBytes(Position, ContentPathBuffer);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void DeleteContentPath(ServiceCtx Context, long TitleId, ContentType ContentType)
        {
            ContentManager ContentManager = Context.Device.System.ContentManager;
            string         ContentPath    = ContentManager.GetInstalledContentPath(TitleId, StorageId, ContentType.Manual);

            ContentManager.ClearEntry(TitleId, ContentType.Manual, StorageId);
        }
    }
}
