using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid.HidServer
{
    static class HidUtils
    {
        public static ControllerId GetIndexFromNpadIdType(HidNpadIdType npadIdType)
        {
            switch (npadIdType)
            {
                case HidNpadIdType.Player1:  return ControllerId.ControllerPlayer1;
                case HidNpadIdType.Player2:  return ControllerId.ControllerPlayer2;
                case HidNpadIdType.Player3:  return ControllerId.ControllerPlayer3;
                case HidNpadIdType.Player4:  return ControllerId.ControllerPlayer4;
                case HidNpadIdType.Player5:  return ControllerId.ControllerPlayer5;
                case HidNpadIdType.Player6:  return ControllerId.ControllerPlayer6;
                case HidNpadIdType.Player7:  return ControllerId.ControllerPlayer7;
                case HidNpadIdType.Player8:  return ControllerId.ControllerPlayer8;
                case HidNpadIdType.Handheld: return ControllerId.ControllerHandheld;
                case HidNpadIdType.Unknown:  return ControllerId.ControllerUnknown;

                default: throw new ArgumentOutOfRangeException(nameof(npadIdType));
            }
        }

        public static HidNpadIdType GetNpadIdTypeFromIndex(ControllerId index)
        {
            switch (index)
            {
                case ControllerId.ControllerPlayer1:  return HidNpadIdType.Player1;
                case ControllerId.ControllerPlayer2:  return HidNpadIdType.Player2;
                case ControllerId.ControllerPlayer3:  return HidNpadIdType.Player3;
                case ControllerId.ControllerPlayer4:  return HidNpadIdType.Player4;
                case ControllerId.ControllerPlayer5:  return HidNpadIdType.Player5;
                case ControllerId.ControllerPlayer6:  return HidNpadIdType.Player6;
                case ControllerId.ControllerPlayer7:  return HidNpadIdType.Player7;
                case ControllerId.ControllerPlayer8:  return HidNpadIdType.Player8;
                case ControllerId.ControllerHandheld: return HidNpadIdType.Handheld;
                case ControllerId.ControllerUnknown:  return HidNpadIdType.Unknown;

                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}