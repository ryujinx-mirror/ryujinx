using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using System.Collections.Generic;

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
                {   0, CreateAppletResource            },
                {  11, ActivateTouchScreen             },
                { 100, SetSupportedNpadStyleSet        },
                { 101, GetSupportedNpadStyleSet        },
                { 102, SetSupportedNpadIdType          },
                { 103, ActivateNpad                    },
                { 120, SetNpadJoyHoldType              },
                { 121, GetNpadJoyHoldType              },
                { 200, GetVibrationDeviceInfo          },
                { 203, CreateActiveVibrationDeviceList },
                { 206, SendVibrationValues             }
            };
        }

        public long CreateAppletResource(ServiceCtx Context)
        {
            HSharedMem HidHndData = Context.Ns.Os.Handles.GetData<HSharedMem>(Context.Ns.Os.HidHandle);

            MakeObject(Context, new IAppletResource(HidHndData));

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