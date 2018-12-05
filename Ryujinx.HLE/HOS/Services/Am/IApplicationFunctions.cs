using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IApplicationFunctions : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IApplicationFunctions()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
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

        public long PopLaunchParameter(ServiceCtx Context)
        {
            //Only the first 0x18 bytes of the Data seems to be actually used.
            MakeObject(Context, new IStorage(StorageHelper.MakeLaunchParams()));

            return 0;
        }

        public long EnsureSaveData(ServiceCtx Context)
        {
            long UIdLow  = Context.RequestData.ReadInt64();
            long UIdHigh = Context.RequestData.ReadInt64();

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            Context.ResponseData.Write(0L);

            return 0;
        }

        public long GetDesiredLanguage(ServiceCtx Context)
        {
            Context.ResponseData.Write(Context.Device.System.State.DesiredLanguageCode);

            return 0;
        }

        public long SetTerminateResult(ServiceCtx Context)
        {
            int ErrorCode = Context.RequestData.ReadInt32();

            string Result = GetFormattedErrorCode(ErrorCode);

            Logger.PrintInfo(LogClass.ServiceAm, $"Result = 0x{ErrorCode:x8} ({Result}).");

            return 0;
        }

        private string GetFormattedErrorCode(int ErrorCode)
        {
            int Module      = (ErrorCode >> 0) & 0x1ff;
            int Description = (ErrorCode >> 9) & 0x1fff;

            return $"{(2000 + Module):d4}-{Description:d4}";
        }

        public long GetDisplayVersion(ServiceCtx Context)
        {
            //FIXME: Need to check correct version on a switch.
            Context.ResponseData.Write(1L);
            Context.ResponseData.Write(0L);

            return 0;
        }

        public long NotifyRunning(ServiceCtx Context)
        {
            Context.ResponseData.Write(1);

            return 0;
        }

        public long GetPseudoDeviceId(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            Context.ResponseData.Write(0L);
            Context.ResponseData.Write(0L);

            return 0;
        }

        public long InitializeGamePlayRecording(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }

        public long SetGamePlayRecordingState(ServiceCtx Context)
        {
            int State = Context.RequestData.ReadInt32();

            Logger.PrintStub(LogClass.ServiceAm, "Stubbed.");

            return 0;
        }
    }
}
