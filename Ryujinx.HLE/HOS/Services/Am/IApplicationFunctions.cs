using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IApplicationFunctions : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IApplicationFunctions()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 1,  PopLaunchParameter          },
                { 20, EnsureSaveData              },
                { 21, GetDesiredLanguage          },
                { 22, SetTerminateResult          },
                { 23, GetDisplayVersion           },
                { 40, NotifyRunning               },
                { 50, GetPseudoDeviceId           },
                { 66, InitializeGamePlayRecording },
                { 67, SetGamePlayRecordingState   }
            };
        }

        public long PopLaunchParameter(ServiceCtx context)
        {
            // Only the first 0x18 bytes of the Data seems to be actually used.
            MakeObject(context, new IStorage(StorageHelper.MakeLaunchParams()));

            return 0;
        }

        public long EnsureSaveData(ServiceCtx context)
        {
            long uIdLow  = context.RequestData.ReadInt64();
            long uIdHigh = context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(0L);

            return 0;
        }

        public long GetDesiredLanguage(ServiceCtx context)
        {
            context.ResponseData.Write(context.Device.System.State.DesiredLanguageCode);

            return 0;
        }

        public long SetTerminateResult(ServiceCtx context)
        {
            int errorCode = context.RequestData.ReadInt32();

            string result = GetFormattedErrorCode(errorCode);

            Logger.PrintInfo(LogClass.ServiceAm, $"Result = 0x{errorCode:x8} ({result}).");

            return 0;
        }

        private string GetFormattedErrorCode(int errorCode)
        {
            int module      = (errorCode >> 0) & 0x1ff;
            int description = (errorCode >> 9) & 0x1fff;

            return $"{(2000 + module):d4}-{description:d4}";
        }

        public long GetDisplayVersion(ServiceCtx context)
        {
            // FIXME: Need to check correct version on a switch.
            context.ResponseData.Write(1L);
            context.ResponseData.Write(0L);

            return 0;
        }

        public long NotifyRunning(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            return 0;
        }

        public long GetPseudoDeviceId(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(0L);
            context.ResponseData.Write(0L);

            return 0;
        }

        public long InitializeGamePlayRecording(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }

        public long SetGamePlayRecordingState(ServiceCtx context)
        {
            int state = context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm);

            return 0;
        }
    }
}
