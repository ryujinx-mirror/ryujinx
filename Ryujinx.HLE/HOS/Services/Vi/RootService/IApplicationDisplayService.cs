using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.SurfaceFlinger;
using Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Vi.RootService
{
    class IApplicationDisplayService : IpcService
    {
        private readonly IdDictionary _displays;

        private int _vsyncEventHandle;

        public IApplicationDisplayService()
        {
            _displays = new IdDictionary();
        }

        [Command(100)]
        // GetRelayService() -> object<nns::hosbinder::IHOSBinderDriver>
        public ResultCode GetRelayService(ServiceCtx context)
        {
            MakeObject(context, new HOSBinderDriverServer());

            return ResultCode.Success;
        }

        [Command(101)]
        // GetSystemDisplayService() -> object<nn::visrv::sf::ISystemDisplayService>
        public ResultCode GetSystemDisplayService(ServiceCtx context)
        {
            MakeObject(context, new ISystemDisplayService(this));

            return ResultCode.Success;
        }

        [Command(102)]
        // GetManagerDisplayService() -> object<nn::visrv::sf::IManagerDisplayService>
        public ResultCode GetManagerDisplayService(ServiceCtx context)
        {
            MakeObject(context, new IManagerDisplayService(this));

            return ResultCode.Success;
        }

        [Command(103)] // 2.0.0+
        // GetIndirectDisplayTransactionService() -> object<nns::hosbinder::IHOSBinderDriver>
        public ResultCode GetIndirectDisplayTransactionService(ServiceCtx context)
        {
            MakeObject(context, new HOSBinderDriverServer());

            return ResultCode.Success;
        }

        [Command(1000)]
        // ListDisplays() -> (u64, buffer<nn::vi::DisplayInfo, 6>)
        public ResultCode ListDisplays(ServiceCtx context)
        {
            long recBuffPtr = context.Request.ReceiveBuff[0].Position;

            MemoryHelper.FillWithZeros(context.Memory, recBuffPtr, 0x60);

            // Add only the default display to buffer
            context.Memory.Write((ulong)recBuffPtr, Encoding.ASCII.GetBytes("Default"));
            context.Memory.Write((ulong)recBuffPtr + 0x40, 0x1L);
            context.Memory.Write((ulong)recBuffPtr + 0x48, 0x1L);
            context.Memory.Write((ulong)recBuffPtr + 0x50, 1280L);
            context.Memory.Write((ulong)recBuffPtr + 0x58, 720L);

            context.ResponseData.Write(1L);

            return ResultCode.Success;
        }

        [Command(1010)]
        // OpenDisplay(nn::vi::DisplayName) -> u64
        public ResultCode OpenDisplay(ServiceCtx context)
        {
            string name = GetDisplayName(context);

            long displayId = _displays.Add(new Display(name));

            context.ResponseData.Write(displayId);

            return ResultCode.Success;
        }

        [Command(1020)]
        // CloseDisplay(u64)
        public ResultCode CloseDisplay(ServiceCtx context)
        {
            int displayId = context.RequestData.ReadInt32();

            _displays.Delete(displayId);

            return ResultCode.Success;
        }

        [Command(1102)]
        // GetDisplayResolution(u64) -> (u64, u64)
        public ResultCode GetDisplayResolution(ServiceCtx context)
        {
            long displayId = context.RequestData.ReadInt32();

            context.ResponseData.Write(1280);
            context.ResponseData.Write(720);

            return ResultCode.Success;
        }

        [Command(2020)]
        // OpenLayer(nn::vi::DisplayName, u64, nn::applet::AppletResourceUserId, pid) -> (u64, buffer<bytes, 6>)
        public ResultCode OpenLayer(ServiceCtx context)
        {
            // TODO: support multi display.
            byte[] displayName = context.RequestData.ReadBytes(0x40);

            long layerId   = context.RequestData.ReadInt64();
            long userId    = context.RequestData.ReadInt64();
            long parcelPtr = context.Request.ReceiveBuff[0].Position;

            IBinder producer = context.Device.System.SurfaceFlinger.OpenLayer(context.Request.HandleDesc.PId, layerId);

            Parcel parcel = new Parcel(0x28, 0x4);

            parcel.WriteObject(producer, "dispdrv\0");

            ReadOnlySpan<byte> parcelData = parcel.Finish();

            context.Memory.Write((ulong)parcelPtr, parcelData);

            context.ResponseData.Write((long)parcelData.Length);

            return ResultCode.Success;
        }

        [Command(2021)]
        // CloseLayer(u64)
        public ResultCode CloseLayer(ServiceCtx context)
        {
            long layerId = context.RequestData.ReadInt64();

            context.Device.System.SurfaceFlinger.CloseLayer(layerId);

            return ResultCode.Success;
        }

        [Command(2030)]
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public ResultCode CreateStrayLayer(ServiceCtx context)
        {
            long layerFlags = context.RequestData.ReadInt64();
            long displayId  = context.RequestData.ReadInt64();

            long parcelPtr = context.Request.ReceiveBuff[0].Position;

            // TODO: support multi display.
            Display disp = _displays.GetData<Display>((int)displayId);

            IBinder producer = context.Device.System.SurfaceFlinger.CreateLayer(0, out long layerId);

            Parcel parcel = new Parcel(0x28, 0x4);

            parcel.WriteObject(producer, "dispdrv\0");

            ReadOnlySpan<byte> parcelData = parcel.Finish();

            context.Memory.Write((ulong)parcelPtr, parcelData);

            context.ResponseData.Write(layerId);
            context.ResponseData.Write((long)parcelData.Length);

            return ResultCode.Success;
        }

        [Command(2031)]
        // DestroyStrayLayer(u64)
        public ResultCode DestroyStrayLayer(ServiceCtx context)
        {
            long layerId = context.RequestData.ReadInt64();

            context.Device.System.SurfaceFlinger.CloseLayer(layerId);

            return ResultCode.Success;
        }

        [Command(2101)]
        // SetLayerScalingMode(u32, u64)
        public ResultCode SetLayerScalingMode(ServiceCtx context)
        {
            int  scalingMode = context.RequestData.ReadInt32();
            long layerId     = context.RequestData.ReadInt64();

            return ResultCode.Success;
        }

        [Command(2102)] // 5.0.0+
        // ConvertScalingMode(unknown) -> unknown
        public ResultCode ConvertScalingMode(ServiceCtx context)
        {
            SourceScalingMode scalingMode = (SourceScalingMode)context.RequestData.ReadInt32();

            DestinationScalingMode? convertedScalingMode = ConvertScalingMode(scalingMode);

            if (!convertedScalingMode.HasValue)
            {
                // Scaling mode out of the range of valid values.
                return ResultCode.InvalidArguments;
            }

            if (scalingMode != SourceScalingMode.ScaleToWindow &&
                scalingMode != SourceScalingMode.PreserveAspectRatio)
            {
                // Invalid scaling mode specified.
                return ResultCode.InvalidScalingMode;
            }

            context.ResponseData.Write((ulong)convertedScalingMode);

            return ResultCode.Success;
        }

        private DestinationScalingMode? ConvertScalingMode(SourceScalingMode source)
        {
            switch (source)
            {
                case SourceScalingMode.None:                return DestinationScalingMode.None;
                case SourceScalingMode.Freeze:              return DestinationScalingMode.Freeze;
                case SourceScalingMode.ScaleAndCrop:        return DestinationScalingMode.ScaleAndCrop;
                case SourceScalingMode.ScaleToWindow:       return DestinationScalingMode.ScaleToWindow;
                case SourceScalingMode.PreserveAspectRatio: return DestinationScalingMode.PreserveAspectRatio;
            }

            return null;
        }

        [Command(2450)]
        // GetIndirectLayerImageMap(s64 width, s64 height, u64 handle, nn::applet::AppletResourceUserId, pid) -> (s64, s64, buffer<bytes, 0x46>)
        public ResultCode GetIndirectLayerImageMap(ServiceCtx context)
        {
            // The size of the layer buffer should be an aligned multiple of width * height
            // because it was created using GetIndirectLayerImageRequiredMemoryInfo as a guide.

            long layerBuffPosition = context.Request.ReceiveBuff[0].Position;
            long layerBuffSize     = context.Request.ReceiveBuff[0].Size;

            // Fill the layer with zeros.
            context.Memory.Fill((ulong)layerBuffPosition, (ulong)layerBuffSize, 0x00);

            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [Command(2460)]
        // GetIndirectLayerImageRequiredMemoryInfo(u64 width, u64 height) -> (u64 size, u64 alignment)
        public ResultCode GetIndirectLayerImageRequiredMemoryInfo(ServiceCtx context)
        {
            /*
            // Doesn't occur in our case.
            if (sizePtr == null || address_alignmentPtr == null)
            {
                return ResultCode.InvalidArguments;
            }
            */

            int width  = (int)context.RequestData.ReadUInt64();
            int height = (int)context.RequestData.ReadUInt64();

            if (height < 0 || width < 0)
            {
                return ResultCode.InvalidLayerSize;
            }
            else
            {
                /*
                // Doesn't occur in our case.
                if (!service_initialized)
                {
                    return ResultCode.InvalidArguments;
                }
                */

                const ulong defaultAlignment = 0x1000;
                const ulong defaultSize      = 0x20000;

                // NOTE: The official service setup a A8B8G8R8 texture with a linear layout and then query its size.
                //       As we don't need this texture on the emulator, we can just simplify this logic and directly
                //       do a linear layout size calculation. (stride * height * bytePerPixel)
                int   pitch              = BitUtils.AlignUp(BitUtils.DivRoundUp(width * 32, 8), 64);
                int   memorySize         = pitch * BitUtils.AlignUp(height, 64);
                ulong requiredMemorySize = (ulong)BitUtils.AlignUp(memorySize, (int)defaultAlignment);
                ulong size               = (requiredMemorySize + defaultSize - 1) / defaultSize * defaultSize;

                context.ResponseData.Write(size);
                context.ResponseData.Write(defaultAlignment);
            }

            return ResultCode.Success;
        }

        [Command(5202)]
        // GetDisplayVsyncEvent(u64) -> handle<copy>
        public ResultCode GetDisplayVSyncEvent(ServiceCtx context)
        {
            string name = GetDisplayName(context);

            if (_vsyncEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.System.VsyncEvent.ReadableEvent, out _vsyncEventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_vsyncEventHandle);

            return ResultCode.Success;
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