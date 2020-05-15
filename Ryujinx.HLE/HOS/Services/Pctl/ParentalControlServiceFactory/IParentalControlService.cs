using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Arp;
using System;

namespace Ryujinx.HLE.HOS.Services.Pctl.ParentalControlServiceFactory
{
    class IParentalControlService : IpcService
    {
        private int   _permissionFlag;
        private ulong _titleId;
        private bool  _freeCommunicationEnabled;
        private int[] _ratingAge;

        public IParentalControlService(ServiceCtx context, bool withInitialize, int permissionFlag)
        {
            _permissionFlag = permissionFlag;

            if (withInitialize)
            {
                Initialize(context);
            }
        }

        [Command(1)] // 4.0.0+
        // Initialize()
        public ResultCode Initialize(ServiceCtx context)
        {
            if ((_permissionFlag & 0x8001) == 0)
            {
                return ResultCode.PermissionDenied;
            }

            ResultCode resultCode = ResultCode.InvalidPid;

            if (context.Process.Pid != 0)
            {
                if ((_permissionFlag & 0x40) == 0)
                {
                    ulong titleId = ApplicationLaunchProperty.GetByPid(context).TitleId;

                    if (titleId != 0)
                    {
                        _titleId = titleId;

                        // TODO: Call nn::arp::GetApplicationControlProperty here when implemented, if it return ResultCode.Success we assign fields.
                        _ratingAge                = Array.ConvertAll(context.Device.System.ControlData.Value.RatingAge.ToArray(), Convert.ToInt32);
                        _freeCommunicationEnabled = context.Device.System.ControlData.Value.ParentalControl == LibHac.Ns.ParentalControlFlagValue.FreeCommunication;
                    }
                }

                if (_titleId != 0)
                {
                    // TODO: Service store some private fields in another static object.

                    if ((_permissionFlag & 0x8040) == 0)
                    {
                        // TODO: Service store TitleId and FreeCommunicationEnabled in another static object.
                        //       When it's done it signal an event in this static object.
                        Logger.PrintStub(LogClass.ServicePctl);
                    }
                }

                resultCode = ResultCode.Success;
            }

            return resultCode;
        }

        [Command(1001)]
        // CheckFreeCommunicationPermission()
        public ResultCode CheckFreeCommunicationPermission(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServicePctl);

            if (!_freeCommunicationEnabled)
            {
                return ResultCode.FreeCommunicationDisabled;
            }

            return ResultCode.Success;
        }
    }
}