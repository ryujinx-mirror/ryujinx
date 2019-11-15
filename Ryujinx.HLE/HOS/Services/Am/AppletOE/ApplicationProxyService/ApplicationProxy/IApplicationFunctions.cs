using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.Storage;
using Ryujinx.HLE.HOS.Services.Sdb.Pdm.QueryService;
using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService.ApplicationProxy
{
    class IApplicationFunctions : IpcService
    {
        private KEvent _gpuErrorDetectedSystemEvent;

        public IApplicationFunctions(Horizon system)
        {
            _gpuErrorDetectedSystemEvent = new KEvent(system);
        }

        [Command(1)]
        // PopLaunchParameter(u32) -> object<nn::am::service::IStorage>
        public ResultCode PopLaunchParameter(ServiceCtx context)
        {
            // Only the first 0x18 bytes of the Data seems to be actually used.
            MakeObject(context, new IStorage(StorageHelper.MakeLaunchParams()));

            return ResultCode.Success;
        }

        [Command(20)]
        // EnsureSaveData(nn::account::Uid) -> u64
        public ResultCode EnsureSaveData(ServiceCtx context)
        {
            UInt128 userId = new UInt128(context.RequestData.ReadBytes(0x10));

            context.ResponseData.Write(0L);

            Logger.PrintStub(LogClass.ServiceAm, new { userId });

            return ResultCode.Success;
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
            int    errorCode = context.RequestData.ReadInt32();
            string result    = GetFormattedErrorCode(errorCode);

            Logger.PrintInfo(LogClass.ServiceAm, $"Result = 0x{errorCode:x8} ({result}).");

            return ResultCode.Success;
        }

        private string GetFormattedErrorCode(int errorCode)
        {
            int module      = (errorCode >> 0) & 0x1ff;
            int description = (errorCode >> 9) & 0x1fff;

            return $"{(2000 + module):d4}-{description:d4}";
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
    }
}