using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Ncm;
using LibHac.Ns;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.Storage;
using Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService;
using System;

using static LibHac.Fs.ApplicationSaveDataManagement;
using AccountUid = Ryujinx.HLE.HOS.Services.Account.Acc.UserId;

namespace Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService.ApplicationProxy
{
    class IApplicationFunctions : IpcService
    {
        private KEvent _gpuErrorDetectedSystemEvent;
        private KEvent _friendInvitationStorageChannelEvent;
        private KEvent _notificationStorageChannelEvent;

        public IApplicationFunctions(Horizon system)
        {
            _gpuErrorDetectedSystemEvent         = new KEvent(system.KernelContext);
            _friendInvitationStorageChannelEvent = new KEvent(system.KernelContext);
            _notificationStorageChannelEvent     = new KEvent(system.KernelContext);
        }

        [Command(1)]
        // PopLaunchParameter(u32) -> object<nn::am::service::IStorage>
        public ResultCode PopLaunchParameter(ServiceCtx context)
        {
            // Only the first 0x18 bytes of the Data seems to be actually used.
            MakeObject(context, new AppletAE.IStorage(StorageHelper.MakeLaunchParams(context.Device.System.State.Account.LastOpenedUser)));

            return ResultCode.Success;
        }

        [Command(20)]
        // EnsureSaveData(nn::account::Uid) -> u64
        public ResultCode EnsureSaveData(ServiceCtx context)
        {
            Uid     userId  = context.RequestData.ReadStruct<AccountUid>().ToLibHacUid();
            TitleId titleId = new TitleId(context.Process.TitleId);

            BlitStruct<ApplicationControlProperty> controlHolder = context.Device.Application.ControlData;

            ref ApplicationControlProperty control = ref controlHolder.Value;

            if (Util.IsEmpty(controlHolder.ByteSpan))
            {
                // If the current application doesn't have a loaded control property, create a dummy one
                // and set the savedata sizes so a user savedata will be created.
                control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                control.UserAccountSaveDataSize        = 0x4000;
                control.UserAccountSaveDataJournalSize = 0x4000;

                Logger.PrintWarning(LogClass.ServiceAm,
                    "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
            }

            Result result = EnsureApplicationSaveData(context.Device.FileSystem.FsClient, out long requiredSize, titleId,
                ref control, ref userId);

            context.ResponseData.Write(requiredSize);

            return (ResultCode)result.Value;
        }

        [Command(21)]
        // GetDesiredLanguage() -> nn::settings::LanguageCode
        public ResultCode GetDesiredLanguage(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.DesiredLanguageCode);

            return ResultCode.Success;
        }

        [Command(22)]
        // SetTerminateResult(u32)
        public ResultCode SetTerminateResult(ServiceCtx context)
        {
            Result result = new Result(context.RequestData.ReadUInt32());

            Logger.PrintInfo(LogClass.ServiceAm, $"Result = 0x{result.Value:x8} ({result.ToStringWithName()}).");

            return ResultCode.Success;
        }

        [Command(23)]
        // GetDisplayVersion() -> nn::oe::DisplayVersion
        public ResultCode GetDisplayVersion(ServiceCtx context)
        {
            // FIXME: Need to check correct version on a switch.
            context.ResponseData.Write(1L);
            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        // GetSaveDataSize(u8, nn::account::Uid) -> (u64, u64)
        [Command(26)] // 3.0.0+
        public ResultCode GetSaveDataSize(ServiceCtx context)
        {
            SaveDataType saveDataType = (SaveDataType)context.RequestData.ReadByte();
            context.RequestData.BaseStream.Seek(7, System.IO.SeekOrigin.Current);

            Uid userId = context.RequestData.ReadStruct<AccountUid>().ToLibHacUid();

            // TODO: We return a size of 2GB as we use a directory based save system. This should be enough for most of the games.
            context.ResponseData.Write(2000000000u);

            Logger.PrintStub(LogClass.ServiceAm, new { saveDataType, userId });

            return ResultCode.Success;
        }

        [Command(40)]
        // NotifyRunning() -> b8
        public ResultCode NotifyRunning(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            return ResultCode.Success;
        }

        [Command(50)] // 2.0.0+
        // GetPseudoDeviceId() -> nn::util::Uuid
        public ResultCode GetPseudoDeviceId(ServiceCtx context)
        {
            context.ResponseData.Write(0L);
            context.ResponseData.Write(0L);

            Logger.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(66)] // 3.0.0+
        // InitializeGamePlayRecording(u64, handle<copy>)
        public ResultCode InitializeGamePlayRecording(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [Command(67)] // 3.0.0+
        // SetGamePlayRecordingState(u32)
        public ResultCode SetGamePlayRecordingState(ServiceCtx context)
        {
            int state = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm, new { state });

            return ResultCode.Success;
        }

        [Command(100)] // 5.0.0+
        // InitializeApplicationCopyrightFrameBuffer(s32 width, s32 height, handle<copy, transfer_memory> transfer_memory, u64 transfer_memory_size)
        public ResultCode InitializeApplicationCopyrightFrameBuffer(ServiceCtx context)
        {
            int   width                 = context.RequestData.ReadInt32();
            int   height                = context.RequestData.ReadInt32();
            ulong transferMemorySize    = context.RequestData.ReadUInt64();
            int   transferMemoryHandle  = context.Request.HandleDesc.ToCopy[0];
            ulong transferMemoryAddress = context.Process.HandleTable.GetObject<KTransferMemory>(transferMemoryHandle).Address;

            ResultCode resultCode = ResultCode.InvalidParameters;

            if (((transferMemorySize & 0x3FFFF) == 0) && width <= 1280 && height <= 720)
            {
                resultCode = InitializeApplicationCopyrightFrameBufferImpl(transferMemoryAddress, transferMemorySize, width, height);
            }

            /*
            if (transferMemoryHandle)
            {
                svcCloseHandle(transferMemoryHandle);
            }
            */

            return resultCode;
        }

        private ResultCode InitializeApplicationCopyrightFrameBufferImpl(ulong transferMemoryAddress, ulong transferMemorySize, int width, int height)
        {
            ResultCode resultCode = ResultCode.ObjectInvalid;

            if ((transferMemorySize & 0x3FFFF) != 0)
            {
                return ResultCode.InvalidParameters;
            }

            // if (_copyrightBuffer == null)
            {
                // TODO: Initialize buffer and object.

                Logger.PrintStub(LogClass.ServiceAm, new { transferMemoryAddress, transferMemorySize, width, height });

                resultCode = ResultCode.Success;
            }

            return resultCode;
        }

        [Command(101)] // 5.0.0+
        // SetApplicationCopyrightImage(buffer<bytes, 0x45> frame_buffer, s32 x, s32 y, s32 width, s32 height, s32 window_origin_mode)
        public ResultCode SetApplicationCopyrightImage(ServiceCtx context)
        {
            long frameBufferPos   = context.Request.SendBuff[0].Position;
            long frameBufferSize  = context.Request.SendBuff[0].Size;
            int  x                = context.RequestData.ReadInt32();
            int  y                = context.RequestData.ReadInt32();
            int  width            = context.RequestData.ReadInt32();
            int  height           = context.RequestData.ReadInt32();
            uint windowOriginMode = context.RequestData.ReadUInt32();

            ResultCode resultCode = ResultCode.InvalidParameters;

            if (((y | x) >= 0) && width >= 1 && height >= 1)
            {
                ResultCode result = SetApplicationCopyrightImageImpl(x, y, width, height, frameBufferPos, frameBufferSize, windowOriginMode);

                if (resultCode != ResultCode.Success)
                {
                    resultCode = result;
                }
                else
                {
                    resultCode = ResultCode.Success;
                }
            }

            Logger.PrintStub(LogClass.ServiceAm, new { frameBufferPos, frameBufferSize, x, y, width, height, windowOriginMode });

            return resultCode;
        }

        private ResultCode SetApplicationCopyrightImageImpl(int x, int y, int width, int height, long frameBufferPos, long frameBufferSize, uint windowOriginMode)
        {
            /*
            if (_copyrightBuffer == null)
            {
                return ResultCode.NullCopyrightObject;
            }
            */

            Logger.PrintStub(LogClass.ServiceAm, new { x, y, width, height, frameBufferPos, frameBufferSize, windowOriginMode });

            return ResultCode.Success;
        }

        [Command(102)] // 5.0.0+
        // SetApplicationCopyrightVisibility(bool visible)
        public ResultCode SetApplicationCopyrightVisibility(ServiceCtx context)
        {
            bool visible = context.RequestData.ReadBoolean();

            Logger.PrintStub(LogClass.ServiceAm, new { visible });

            // NOTE: It sets an internal field and return ResultCode.Success in all case.

            return ResultCode.Success;
        }

        [Command(110)] // 5.0.0+
        // QueryApplicationPlayStatistics(buffer<bytes, 5> title_id_list) -> (buffer<bytes, 6> entries, s32 entries_count)
        public ResultCode QueryApplicationPlayStatistics(ServiceCtx context)
        {
            // TODO: Call pdm:qry cmd 13 when IPC call between services will be implemented.
            return (ResultCode)QueryPlayStatisticsManager.GetPlayStatistics(context);
        }

        [Command(111)] // 6.0.0+
        // QueryApplicationPlayStatisticsByUid(nn::account::Uid, buffer<bytes, 5> title_id_list) -> (buffer<bytes, 6> entries, s32 entries_count)
        public ResultCode QueryApplicationPlayStatisticsByUid(ServiceCtx context)
        {
            // TODO: Call pdm:qry cmd 16 when IPC call between services will be implemented.
            return (ResultCode)QueryPlayStatisticsManager.GetPlayStatistics(context, true);
        }

        [Command(130)] // 8.0.0+
        // GetGpuErrorDetectedSystemEvent() -> handle<copy>
        public ResultCode GetGpuErrorDetectedSystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_gpuErrorDetectedSystemEvent.ReadableEvent, out int gpuErrorDetectedSystemEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(gpuErrorDetectedSystemEventHandle);

            // NOTE: This is used by "sdk" NSO during applet-application initialization. 
            //       A seperate thread is setup where event-waiting is handled. 
            //       When the Event is signaled, official sw will assert.

            return ResultCode.Success;
        }

        [Command(140)] // 9.0.0+
        // GetFriendInvitationStorageChannelEvent() -> handle<copy>
        public ResultCode GetFriendInvitationStorageChannelEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_friendInvitationStorageChannelEvent.ReadableEvent, out int friendInvitationStorageChannelEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(friendInvitationStorageChannelEventHandle);

            return ResultCode.Success;
        }

        [Command(150)] // 9.0.0+
        // GetNotificationStorageChannelEvent() -> handle<copy>
        public ResultCode GetNotificationStorageChannelEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_notificationStorageChannelEvent.ReadableEvent, out int notificationStorageChannelEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(notificationStorageChannelEventHandle);

            return ResultCode.Success;
        }
    }
}