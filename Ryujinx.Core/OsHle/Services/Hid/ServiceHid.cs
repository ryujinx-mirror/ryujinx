using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;
using Ryujinx.Core.Input;

using static Ryujinx.Core.OsHle.IpcServices.ObjHelper;

namespace Ryujinx.Core.OsHle.IpcServices.Hid
{
    class ServiceHid : IIpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public ServiceHid()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                {   0, CreateAppletResource                    },
                {  11, ActivateTouchScreen                     },
                { 100, SetSupportedNpadStyleSet                },
                { 101, GetSupportedNpadStyleSet                },
                { 102, SetSupportedNpadIdType                  },
                { 103, ActivateNpad                            },
                { 120, SetNpadJoyHoldType                      },
                { 122, SetNpadJoyAssignmentModeSingleByDefault },
                { 123, SetNpadJoyAssignmentModeSingle          },
                { 124, SetNpadJoyAssignmentModeDual            },
                { 125, MergeSingleJoyAsDualJoy                 },
                { 200, GetVibrationDeviceInfo                  },
                { 203, CreateActiveVibrationDeviceList         },
                { 206, SendVibrationValues                     }
            };
        }

        public long CreateAppletResource(ServiceCtx Context)
        {
            MakeObject(Context, new IAppletResource(Context.Ns.Os.HidSharedMem));

            return 0;
        }

        public long ActivateTouchScreen(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            return 0;
        }

        public long GetSupportedNpadStyleSet(ServiceCtx Context)
        {
            Context.ResponseData.Write(0);

            return 0;
        }

        public long SetSupportedNpadStyleSet(ServiceCtx Context)
        {
            long Unknown0 = Context.RequestData.ReadInt64();
            long Unknown8 = Context.RequestData.ReadInt64();

            return 0;
        }

        public long SetSupportedNpadIdType(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            return 0;
        }

        public long ActivateNpad(ServiceCtx Context)
        {
            long Unknown = Context.RequestData.ReadInt64();

            return 0;
        }

        public long SetNpadJoyHoldType(ServiceCtx Context)
        {
            long Unknown0 = Context.RequestData.ReadInt64();
            long Unknown8 = Context.RequestData.ReadInt64();

            return 0;
        }

        public long GetNpadJoyHoldType(ServiceCtx Context)
        {
            Context.ResponseData.Write(0L);

            return 0;
        }

        public long SetNpadJoyAssignmentModeSingleByDefault(ServiceCtx Context)
        {
            HidControllerId HidControllerId = (HidControllerId)Context.RequestData.ReadInt32();
            long AppletUserResourseId = Context.RequestData.ReadInt64();

            return 0;
        }

        public long SetNpadJoyAssignmentModeSingle(ServiceCtx Context)
        {
            HidControllerId HidControllerId = (HidControllerId)Context.RequestData.ReadInt32();
            long AppletUserResourseId = Context.RequestData.ReadInt64();
            long NpadJoyDeviceType = Context.RequestData.ReadInt64();
            
            return 0;
        }

        public long SetNpadJoyAssignmentModeDual(ServiceCtx Context)
        {
            HidControllerId HidControllerId = (HidControllerId)Context.RequestData.ReadInt32();
            long AppletUserResourseId = Context.RequestData.ReadInt64();

            return 0;
        }

        public long MergeSingleJoyAsDualJoy(ServiceCtx Context)
        {
            long Unknown0 = Context.RequestData.ReadInt32();
            long Unknown8 = Context.RequestData.ReadInt32();
            long AppletUserResourseId = Context.RequestData.ReadInt64();

            return 0;
        }

        public long GetVibrationDeviceInfo(ServiceCtx Context)
        {
            int VibrationDeviceHandle = Context.RequestData.ReadInt32();

            Context.ResponseData.Write(0L); //VibrationDeviceInfoForIpc

            return 0;
        }

        public long CreateActiveVibrationDeviceList(ServiceCtx Context)
        {
            MakeObject(Context, new IActiveApplicationDeviceList());

            return 0;
        }

        public long SendVibrationValues(ServiceCtx Context)
        {
            return 0;
        }
    }
}