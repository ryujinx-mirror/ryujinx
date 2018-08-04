using Ryujinx.HLE.Font;
using Ryujinx.HLE.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Services.Pl
{
    class ISharedFontManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ISharedFontManager()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, RequestLoad                    },
                { 1, GetLoadState                   },
                { 2, GetFontSize                    },
                { 3, GetSharedMemoryAddressOffset   },
                { 4, GetSharedMemoryNativeHandle    },
                { 5, GetSharedFontInOrderOfPriority }
            };
        }

        public long RequestLoad(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            Context.Ns.Font.Load(FontType);

            return 0;
        }

        public long GetLoadState(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            Context.ResponseData.Write(Context.Ns.Font.GetLoadState(FontType));

            return 0;
        }

        public long GetFontSize(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            Context.ResponseData.Write(Context.Ns.Font.GetFontSize(FontType));

            return 0;
        }

        public long GetSharedMemoryAddressOffset(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            Context.ResponseData.Write(Context.Ns.Font.GetSharedMemoryAddressOffset(FontType));

            return 0;
        }

        public long GetSharedMemoryNativeHandle(ServiceCtx Context)
        {
            int Handle = Context.Process.HandleTable.OpenHandle(Context.Ns.Os.FontSharedMem);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        private uint AddFontToOrderOfPriorityList(ServiceCtx Context, SharedFontType FontType, uint BufferPos, out uint LoadState)
        {
            long TypesPosition          = Context.Request.ReceiveBuff[0].Position;
            long TypesSize              = Context.Request.ReceiveBuff[0].Size;

            long OffsetsPosition        = Context.Request.ReceiveBuff[1].Position;
            long OffsetsSize            = Context.Request.ReceiveBuff[1].Size;

            long FontSizeBufferPosition = Context.Request.ReceiveBuff[2].Position;
            long FontSizeBufferSize     = Context.Request.ReceiveBuff[2].Size;

            LoadState                   = Context.Ns.Font.GetLoadState(FontType);

            if (BufferPos >= TypesSize || BufferPos >= OffsetsSize || BufferPos >= FontSizeBufferSize)
            {
                return 0;
            }

            Context.Memory.WriteUInt32(TypesPosition + BufferPos, (uint)FontType);
            Context.Memory.WriteUInt32(OffsetsPosition + BufferPos, Context.Ns.Font.GetSharedMemoryAddressOffset(FontType));
            Context.Memory.WriteUInt32(FontSizeBufferPosition + BufferPos, Context.Ns.Font.GetFontSize(FontType));

            BufferPos += 4;

            return BufferPos;
        }

        public long GetSharedFontInOrderOfPriority(ServiceCtx Context)
        {
            ulong LanguageCode = Context.RequestData.ReadUInt64();
            uint  LoadedCount  = 0;
            uint  BufferPos    = 0;
            uint  Loaded       = 0;

            for (int Type = 0; Type < Context.Ns.Font.Count; Type++)
            {
                BufferPos   = AddFontToOrderOfPriorityList(Context, (SharedFontType)Type, BufferPos, out Loaded);
                LoadedCount += Loaded;
            }

            Context.ResponseData.Write(LoadedCount);
            Context.ResponseData.Write(Context.Ns.Font.Count);

            return 0;
        }
    }
}