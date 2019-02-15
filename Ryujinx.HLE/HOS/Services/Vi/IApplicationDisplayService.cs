using ChocolArm64.Memory;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.HOS.Services.Android.Parcel;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class IApplicationDisplayService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private IdDictionary _displays;

        public IApplicationDisplayService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 100,  GetRelayService                      },
                { 101,  GetSystemDisplayService              },
                { 102,  GetManagerDisplayService             },
                { 103,  GetIndirectDisplayTransactionService },
                { 1000, ListDisplays                         },
                { 1010, OpenDisplay                          },
                { 1020, CloseDisplay                         },
                { 1102, GetDisplayResolution                 },
                { 2020, OpenLayer                            },
                { 2021, CloseLayer                           },
                { 2030, CreateStrayLayer                     },
                { 2031, DestroyStrayLayer                    },
                { 2101, SetLayerScalingMode                  },
                { 2102, ConvertScalingMode                   },
                { 5202, GetDisplayVSyncEvent                 }
            };

            _displays = new IdDictionary();
        }

        public long GetRelayService(ServiceCtx context)
        {
            MakeObject(context, new IhosBinderDriver(
                context.Device.System,
                context.Device.Gpu.Renderer));

            return 0;
        }

        public long GetSystemDisplayService(ServiceCtx context)
        {
            MakeObject(context, new ISystemDisplayService(this));

            return 0;
        }

        public long GetManagerDisplayService(ServiceCtx context)
        {
            MakeObject(context, new IManagerDisplayService());

            return 0;
        }

        public long GetIndirectDisplayTransactionService(ServiceCtx context)
        {
            MakeObject(context, new IhosBinderDriver(
                context.Device.System,
                context.Device.Gpu.Renderer));

            return 0;
        }

        public long ListDisplays(ServiceCtx context)
        {
            long recBuffPtr = context.Request.ReceiveBuff[0].Position;

            MemoryHelper.FillWithZeros(context.Memory, recBuffPtr, 0x60);

            //Add only the default display to buffer
            context.Memory.WriteBytes(recBuffPtr, Encoding.ASCII.GetBytes("Default"));
            context.Memory.WriteInt64(recBuffPtr + 0x40, 0x1L);
            context.Memory.WriteInt64(recBuffPtr + 0x48, 0x1L);
            context.Memory.WriteInt64(recBuffPtr + 0x50, 1920L);
            context.Memory.WriteInt64(recBuffPtr + 0x58, 1080L);

            context.ResponseData.Write(1L);

            return 0;
        }

        public long OpenDisplay(ServiceCtx context)
        {
            string name = GetDisplayName(context);

            long displayId = _displays.Add(new Display(name));

            context.ResponseData.Write(displayId);

            return 0;
        }

        public long CloseDisplay(ServiceCtx context)
        {
            int displayId = context.RequestData.ReadInt32();

            _displays.Delete(displayId);

            return 0;
        }

        public long GetDisplayResolution(ServiceCtx context)
        {
            long displayId = context.RequestData.ReadInt32();

            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);

            return 0;
        }

        public long OpenLayer(ServiceCtx context)
        {
            long layerId = context.RequestData.ReadInt64();
            long userId  = context.RequestData.ReadInt64();

            long parcelPtr = context.Request.ReceiveBuff[0].Position;

            byte[] parcel = MakeIGraphicsBufferProducer(parcelPtr);

            context.Memory.WriteBytes(parcelPtr, parcel);

            context.ResponseData.Write((long)parcel.Length);

            return 0;
        }

        public long CloseLayer(ServiceCtx context)
        {
            long layerId = context.RequestData.ReadInt64();

            return 0;
        }

        public long CreateStrayLayer(ServiceCtx context)
        {
            long layerFlags = context.RequestData.ReadInt64();
            long displayId  = context.RequestData.ReadInt64();

            long parcelPtr = context.Request.ReceiveBuff[0].Position;

            Display disp = _displays.GetData<Display>((int)displayId);

            byte[] parcel = MakeIGraphicsBufferProducer(parcelPtr);

            context.Memory.WriteBytes(parcelPtr, parcel);

            context.ResponseData.Write(0L);
            context.ResponseData.Write((long)parcel.Length);

            return 0;
        }

        public long DestroyStrayLayer(ServiceCtx context)
        {
            return 0;
        }

        public long SetLayerScalingMode(ServiceCtx context)
        {
            int  scalingMode = context.RequestData.ReadInt32();
            long unknown     = context.RequestData.ReadInt64();

            return 0;
        }

        public long ConvertScalingMode(ServiceCtx context)
        {
            SrcScalingMode  scalingMode     = (SrcScalingMode)context.RequestData.ReadInt32();
            DstScalingMode? destScalingMode = ConvetScalingModeImpl(scalingMode);

            if (!destScalingMode.HasValue)
            {
                return MakeError(ErrorModule.Vi, 1);
            }

            context.ResponseData.Write((ulong)destScalingMode);

            return 0;
        }

        private DstScalingMode? ConvetScalingModeImpl(SrcScalingMode srcScalingMode)
        {
            switch (srcScalingMode)
            {
                case SrcScalingMode.None:                return DstScalingMode.None;
                case SrcScalingMode.Freeze:              return DstScalingMode.Freeze;
                case SrcScalingMode.ScaleAndCrop:        return DstScalingMode.ScaleAndCrop;
                case SrcScalingMode.ScaleToWindow:       return DstScalingMode.ScaleToWindow;
                case SrcScalingMode.PreserveAspectRatio: return DstScalingMode.PreserveAspectRatio;
            }

            return null;
        }

        public long GetDisplayVSyncEvent(ServiceCtx context)
        {
            string name = GetDisplayName(context);

            if (context.Process.HandleTable.GenerateHandle(context.Device.System.VsyncEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        private byte[] MakeIGraphicsBufferProducer(long basePtr)
        {
            long id        = 0x20;
            long cookiePtr = 0L;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);

                //flat_binder_object (size is 0x28)
                writer.Write(2); //Type (BINDER_TYPE_WEAK_BINDER)
                writer.Write(0); //Flags
                writer.Write((int)(id >> 0));
                writer.Write((int)(id >> 32));
                writer.Write((int)(cookiePtr >> 0));
                writer.Write((int)(cookiePtr >> 32));
                writer.Write((byte)'d');
                writer.Write((byte)'i');
                writer.Write((byte)'s');
                writer.Write((byte)'p');
                writer.Write((byte)'d');
                writer.Write((byte)'r');
                writer.Write((byte)'v');
                writer.Write((byte)'\0');
                writer.Write(0L); //Pad

                return MakeParcel(ms.ToArray(), new byte[] { 0, 0, 0, 0 });
            }
        }

        private string GetDisplayName(ServiceCtx context)
        {
            string name = string.Empty;

            for (int index = 0; index < 8 &&
                context.RequestData.BaseStream.Position <
                context.RequestData.BaseStream.Length; index++)
            {
                byte chr = context.RequestData.ReadByte();

                if (chr >= 0x20 && chr < 0x7f)
                {
                    name += (char)chr;
                }
            }

            return name;
        }
    }
}