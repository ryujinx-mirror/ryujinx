using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Pl
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

            //We don't need to do anything here because we do lazy initialization
            //on SharedFontManager (the font is loaded when necessary).
            return 0;
        }

        public long GetLoadState(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            //1 (true) indicates that the font is already loaded.
            //All fonts are already loaded.
            Context.ResponseData.Write(1);

            return 0;
        }

        public long GetFontSize(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            Context.ResponseData.Write(Context.Device.System.Font.GetFontSize(FontType));

            return 0;
        }

        public long GetSharedMemoryAddressOffset(ServiceCtx Context)
        {
            SharedFontType FontType = (SharedFontType)Context.RequestData.ReadInt32();

            Context.ResponseData.Write(Context.Device.System.Font.GetSharedMemoryAddressOffset(FontType));

            return 0;
        }

        public long GetSharedMemoryNativeHandle(ServiceCtx Context)
        {
            Context.Device.System.Font.EnsureInitialized();

            int Handle = Context.Process.HandleTable.OpenHandle(Context.Device.System.FontSharedMem);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long GetSharedFontInOrderOfPriority(ServiceCtx Context)
        {
            long LanguageCode = Context.RequestData.ReadInt64();
            int  LoadedCount  = 0;

            for (SharedFontType Type = 0; Type < SharedFontType.Count; Type++)
            {
                int Offset = (int)Type * 4;

                if (!AddFontToOrderOfPriorityList(Context, (SharedFontType)Type, Offset))
                {
                    break;
                }

                LoadedCount++;
            }

            Context.ResponseData.Write(LoadedCount);
            Context.ResponseData.Write((int)SharedFontType.Count);

            return 0;
        }

        private bool AddFontToOrderOfPriorityList(ServiceCtx Context, SharedFontType FontType, int Offset)
        {
            long TypesPosition = Context.Request.ReceiveBuff[0].Position;
            long TypesSize     = Context.Request.ReceiveBuff[0].Size;

            long OffsetsPosition = Context.Request.ReceiveBuff[1].Position;
            long OffsetsSize     = Context.Request.ReceiveBuff[1].Size;

            long FontSizeBufferPosition = Context.Request.ReceiveBuff[2].Position;
            long FontSizeBufferSize     = Context.Request.ReceiveBuff[2].Size;

            if ((uint)Offset + 4 > (uint)TypesSize   ||
                (uint)Offset + 4 > (uint)OffsetsSize ||
                (uint)Offset + 4 > (uint)FontSizeBufferSize)
            {
                return false;
            }

            Context.Memory.WriteInt32(TypesPosition + Offset, (int)FontType);

            Context.Memory.WriteInt32(OffsetsPosition + Offset, Context.Device.System.Font.GetSharedMemoryAddressOffset(FontType));

            Context.Memory.WriteInt32(FontSizeBufferPosition + Offset, Context.Device.System.Font.GetFontSize(FontType));

            return true;
        }
    }
}