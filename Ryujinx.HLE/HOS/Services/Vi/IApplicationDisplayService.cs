using ChocolArm64.Memory;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using System;
using System.IO;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;
using static Ryujinx.HLE.HOS.Services.Android.Parcel;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    class IApplicationDisplayService : IpcService
    {
        private IdDictionary _displays;

        public IApplicationDisplayService()
        {
            _displays = new IdDictionary();
        }

        [Command(100)]
        // GetRelayService() -> object<nns::hosbinder::IHOSBinderDriver>
        public long GetRelayService(ServiceCtx context)
        {
            MakeObject(context, new IHOSBinderDriver(
                context.Device.System,
                context.Device.Gpu.Renderer));

            return 0;
        }

        [Command(101)]
        // GetSystemDisplayService() -> object<nn::visrv::sf::ISystemDisplayService>
        public long GetSystemDisplayService(ServiceCtx context)
        {
            MakeObject(context, new ISystemDisplayService(this));

            return 0;
        }

        [Command(102)]
        // GetManagerDisplayService() -> object<nn::visrv::sf::IManagerDisplayService>
        public long GetManagerDisplayService(ServiceCtx context)
        {
            MakeObject(context, new IManagerDisplayService(this));

            return 0;
        }

        [Command(103)] // 2.0.0+
        // GetIndirectDisplayTransactionService() -> object<nns::hosbinder::IHOSBinderDriver>
        public long GetIndirectDisplayTransactionService(ServiceCtx context)
        {
            MakeObject(context, new IHOSBinderDriver(
                context.Device.System,
                context.Device.Gpu.Renderer));

            return 0;
        }

        [Command(1000)]
        // ListDisplays() -> (u64, buffer<nn::vi::DisplayInfo, 6>)
        public long ListDisplays(ServiceCtx context)
        {
            long recBuffPtr = context.Request.ReceiveBuff[0].Position;

            MemoryHelper.FillWithZeros(context.Memory, recBuffPtr, 0x60);

            // Add only the default display to buffer
            context.Memory.WriteBytes(recBuffPtr, Encoding.ASCII.GetBytes("Default"));
            context.Memory.WriteInt64(recBuffPtr + 0x40, 0x1L);
            context.Memory.WriteInt64(recBuffPtr + 0x48, 0x1L);
            context.Memory.WriteInt64(recBuffPtr + 0x50, 1920L);
            context.Memory.WriteInt64(recBuffPtr + 0x58, 1080L);

            context.ResponseData.Write(1L);

            return 0;
        }

        [Command(1010)]
        // OpenDisplay(nn::vi::DisplayName) -> u64
        public long OpenDisplay(ServiceCtx context)
        {
            string name = GetDisplayName(context);

            long displayId = _displays.Add(new Display(name));

            context.ResponseData.Write(displayId);

            return 0;
        }

        [Command(1020)]
        // CloseDisplay(u64)
        public long CloseDisplay(ServiceCtx context)
        {
            int displayId = context.RequestData.ReadInt32();

            _displays.Delete(displayId);

            return 0;
        }

        [Command(1102)]
        // GetDisplayResolution(u64) -> (u64, u64)
        public long GetDisplayResolution(ServiceCtx context)
        {
            long displayId = context.RequestData.ReadInt32();

            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);

            return 0;
        }

        [Command(2020)]
        // OpenLayer(nn::vi::DisplayName, u64, nn::applet::AppletResourceUserId, pid) -> (u64, buffer<bytes, 6>)
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

        [Command(2021)]
        // CloseLayer(u64)
        public long CloseLayer(ServiceCtx context)
        {
            long layerId = context.RequestData.ReadInt64();

            return 0;
        }

        [Command(2030)]
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
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

        [Command(2031)]
        // DestroyStrayLayer(u64)
        public long DestroyStrayLayer(ServiceCtx context)
        {
            return 0;
        }

        [Command(2101)]
        // SetLayerScalingMode(u32, u64)
        public long SetLayerScalingMode(ServiceCtx context)
        {
            int  scalingMode = context.RequestData.ReadInt32();
            long unknown     = context.RequestData.ReadInt64();

            return 0;
        }

        [Command(2102)] // 5.0.0+
        // ConvertScalingMode(unknown) -> unknown
        public long ConvertScalingMode(ServiceCtx context)
        {
            SrcScalingMode scalingMode = (SrcScalingMode)context.RequestData.ReadInt32();

            DstScalingMode? convertedScalingMode = ConvertScalingMode(scalingMode);

            if (!convertedScalingMode.HasValue)
            {
                // Scaling mode out of the range of valid values.
                return MakeError(ErrorModule.Vi, 1);
            }

            if (scalingMode != SrcScalingMode.ScaleToWindow &&
                scalingMode != SrcScalingMode.PreserveAspectRatio)
            {
                // Invalid scaling mode specified.
                return MakeError(ErrorModule.Vi, 6);
            }

            context.ResponseData.Write((ulong)convertedScalingMode);

            return 0;
        }

        private DstScalingMode? ConvertScalingMode(SrcScalingMode source)
        {
            switch (source)
            {
                case SrcScalingMode.None:                return DstScalingMode.None;
                case SrcScalingMode.Freeze:              return DstScalingMode.Freeze;
                case SrcScalingMode.ScaleAndCrop:        return DstScalingMode.ScaleAndCrop;
                case SrcScalingMode.ScaleToWindow:       return DstScalingMode.ScaleToWindow;
                case SrcScalingMode.PreserveAspectRatio: return DstScalingMode.PreserveAspectRatio;
            }

            return null;
        }

        [Command(5202)]
        // GetDisplayVsyncEvent(u64) -> handle<copy>
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

                // flat_binder_object (size is 0x28)
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