using Ryujinx.Core.Input;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Hid
{
    class IHidServer : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IHidServer()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {   0, CreateAppletResource                    },
                {   1, ActivateDebugPad                        },
                {  11, ActivateTouchScreen                     },
                {  21, ActivateMouse                           },
                {  31, ActivateKeyboard                        },
                {  66, StartSixAxisSensor                      },
                {  79, SetGyroscopeZeroDriftMode               },
                { 100, SetSupportedNpadStyleSet                },
                { 101, GetSupportedNpadStyleSet                },
                { 102, SetSupportedNpadIdType                  },
                { 103, ActivateNpad                            },
                { 108, GetPlayerLedPattern                     },
                { 120, SetNpadJoyHoldType                      },
                { 121, GetNpadJoyHoldType                      },
                { 122, SetNpadJoyAssignmentModeSingleByDefault },
                { 123, SetNpadJoyAssignmentModeSingle          },
                { 124, SetNpadJoyAssignmentModeDual            },
                { 125, MergeSingleJoyAsDualJoy                 },
                { 128, SetNpadHandheldActivationMode           },
                { 200, GetVibrationDeviceInfo                  },
                { 201, SendVibrationValue                      },
                { 203, CreateActiveVibrationDeviceList         },
                { 206, SendVibrationValues                     }
            };
        }

        public long CreateAppletResource(ServiceCtx Context)
        {
            MakeObject(Context, new IAppletResource(Context.Ns.Os.HidSharedMem));

            return 0;
        }

        public long ActivateDebugPad(ServiceCtx Context)
        {
            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long ActivateTouchScreen(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long ActivateMouse(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long ActivateKeyboard(ServiceCtx Context)
        {
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long StartSixAxisSensor(ServiceCtx Context)
        {
            int Handle = Context.RequestData.ReadInt32();

            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetGyroscopeZeroDriftMode(ServiceCtx Context)
        {
            int Handle = Context.RequestData.ReadInt32();
            int Unknown = Context.RequestData.ReadInt32();
            long AppletResourceUserId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long GetSupportedNpadStyleSet(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetSupportedNpadStyleSet(ServiceCtx Context)
        {
            long Unknown0 = Context.RequestData.ReadInt64();
            long Unknown8 = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetSupportedNpadIdType(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long ActivateNpad(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long GetPlayerLedPattern(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt32();

            Context.ResponseData.Write(0L);

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetNpadJoyHoldType(ServiceCtx Context)
        {
            long Unknown0 = Context.RequestData.ReadInt64();
            long Unknown8 = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long GetNpadJoyHoldType(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L);

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetNpadJoyAssignmentModeSingleByDefault(ServiceCtx Context)
        {
            HidControllerId HidControllerId = (HidControllerId)Context.RequestData.ReadInt32();
            long AppletUserResourceId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetNpadJoyAssignmentModeSingle(ServiceCtx Context)
        {
            HidControllerId HidControllerId = (HidControllerId)Context.RequestData.ReadInt32();
            long AppletUserResourceId = Context.RequestData.ReadInt64();
            long NpadJoyDeviceType = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetNpadJoyAssignmentModeDual(ServiceCtx Context)
        {
            HidControllerId HidControllerId = (HidControllerId)Context.RequestData.ReadInt32();
            long AppletUserResourceId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long MergeSingleJoyAsDualJoy(ServiceCtx Context)
        {
            long Unknown0 = Context.RequestData.ReadInt32();
            long Unknown8 = Context.RequestData.ReadInt32();
            long AppletUserResourceId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long SetNpadHandheldActivationMode(ServiceCtx Context)
        {
            long AppletUserResourceId = Context.RequestData.ReadInt64();
            long Unknown = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long GetVibrationDeviceInfo(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();

            Logging.Stub(LogClass.ServiceHid, $"VibrationDeviceHandle = {VibrationDeviceHandle}, VibrationDeviceInfo = 0");

            Context.ResponseData.Write(0L); //VibrationDeviceInfoForIpc

            return 0;
        }

        public long SendVibrationValue(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();

            int VibrationValue1 = Context.RequestData.ReadInt32();
            int VibrationValue2 = Context.RequestData.ReadInt32();
            int VibrationValue3 = Context.RequestData.ReadInt32();
            int VibrationValue4 = Context.RequestData.ReadInt32();

            long AppletUserResourceId = Context.RequestData.ReadInt64();

            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }

        public long CreateActiveVibrationDeviceList(ServiceCtx Context)
        {
            MakeObject(Context, new IActiveApplicationDeviceList());

            return 0;
        }

        public long SendVibrationValues(ServiceCtx Context)
        {
            Logging.Stub(LogClass.ServiceHid, "Stubbed");

            return 0;
        }
    }
}
