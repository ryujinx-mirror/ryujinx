using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Pl
{
    class ISharedFontManager : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public ISharedFontManager()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, RequestLoad                    },
                { 1, GetLoadState                   },
                { 2, GetFontSize                    },
                { 3, GetSharedMemoryAddressOffset   },
                { 4, GetSharedMemoryNativeHandle    },
                { 5, GetSharedFontInOrderOfPriority }
            };
        }

        public long RequestLoad(ServiceCtx context)
        {
            SharedFontType fontType = (SharedFontType)context.RequestData.ReadInt32();

            //We don't need to do anything here because we do lazy initialization
            //on SharedFontManager (the font is loaded when necessary).
            return 0;
        }

        public long GetLoadState(ServiceCtx context)
        {
            SharedFontType fontType = (SharedFontType)context.RequestData.ReadInt32();

            //1 (true) indicates that the font is already loaded.
            //All fonts are already loaded.
            context.ResponseData.Write(1);

            return 0;
        }

        public long GetFontSize(ServiceCtx context)
        {
            SharedFontType fontType = (SharedFontType)context.RequestData.ReadInt32();

            context.ResponseData.Write(context.Device.System.Font.GetFontSize(fontType));

            return 0;
        }

        public long GetSharedMemoryAddressOffset(ServiceCtx context)
        {
            SharedFontType fontType = (SharedFontType)context.RequestData.ReadInt32();

            context.ResponseData.Write(context.Device.System.Font.GetSharedMemoryAddressOffset(fontType));

            return 0;
        }

        public long GetSharedMemoryNativeHandle(ServiceCtx context)
        {
            context.Device.System.Font.EnsureInitialized(context.Device.System.ContentManager);

            if (context.Process.HandleTable.GenerateHandle(context.Device.System.FontSharedMem, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        public long GetSharedFontInOrderOfPriority(ServiceCtx context)
        {
            long languageCode = context.RequestData.ReadInt64();
            int  loadedCount  = 0;

            for (SharedFontType type = 0; type < SharedFontType.Count; type++)
            {
                int offset = (int)type * 4;

                if (!AddFontToOrderOfPriorityList(context, type, offset))
                {
                    break;
                }

                loadedCount++;
            }

            context.ResponseData.Write(loadedCount);
            context.ResponseData.Write((int)SharedFontType.Count);

            return 0;
        }

        private bool AddFontToOrderOfPriorityList(ServiceCtx context, SharedFontType fontType, int offset)
        {
            long typesPosition = context.Request.ReceiveBuff[0].Position;
            long typesSize     = context.Request.ReceiveBuff[0].Size;

            long offsetsPosition = context.Request.ReceiveBuff[1].Position;
            long offsetsSize     = context.Request.ReceiveBuff[1].Size;

            long fontSizeBufferPosition = context.Request.ReceiveBuff[2].Position;
            long fontSizeBufferSize     = context.Request.ReceiveBuff[2].Size;

            if ((uint)offset + 4 > (uint)typesSize   ||
                (uint)offset + 4 > (uint)offsetsSize ||
                (uint)offset + 4 > (uint)fontSizeBufferSize)
            {
                return false;
            }

            context.Memory.WriteInt32(typesPosition + offset, (int)fontType);

            context.Memory.WriteInt32(offsetsPosition + offset, context.Device.System.Font.GetSharedMemoryAddressOffset(fontType));

            context.Memory.WriteInt32(fontSizeBufferPosition + offset, context.Device.System.Font.GetFontSize(fontType));

            return true;
        }
    }
}