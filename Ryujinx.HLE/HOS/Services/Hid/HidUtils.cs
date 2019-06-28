using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    static class HidUtils
    {
        public static HidControllerId GetIndexFromNpadIdType(NpadIdType npadIdType)
        {
            switch (npadIdType)
            {
                case NpadIdType.Player1:  return HidControllerId.ControllerPlayer1;
                case NpadIdType.Player2:  return HidControllerId.ControllerPlayer2;
                case NpadIdType.Player3:  return HidControllerId.ControllerPlayer3;
                case NpadIdType.Player4:  return HidControllerId.ControllerPlayer4;
                case NpadIdType.Player5:  return HidControllerId.ControllerPlayer5;
                case NpadIdType.Player6:  return HidControllerId.ControllerPlayer6;
                case NpadIdType.Player7:  return HidControllerId.ControllerPlayer7;
                case NpadIdType.Player8:  return HidControllerId.ControllerPlayer8;
                case NpadIdType.Handheld: return HidControllerId.ControllerHandheld;
                case NpadIdType.Unknown:  return HidControllerId.ControllerUnknown;

                default: throw new ArgumentOutOfRangeException(nameof(npadIdType));
            }
        }

        public static NpadIdType GetNpadIdTypeFromIndex(HidControllerId index)
        {
            switch (index)
            {
                case HidControllerId.ControllerPlayer1:  return NpadIdType.Player1;
                case HidControllerId.ControllerPlayer2:  return NpadIdType.Player2;
                case HidControllerId.ControllerPlayer3:  return NpadIdType.Player3;
                case HidControllerId.ControllerPlayer4:  return NpadIdType.Player4;
                case HidControllerId.ControllerPlayer5:  return NpadIdType.Player5;
                case HidControllerId.ControllerPlayer6:  return NpadIdType.Player6;
                case HidControllerId.ControllerPlayer7:  return NpadIdType.Player7;
                case HidControllerId.ControllerPlayer8:  return NpadIdType.Player8;
                case HidControllerId.ControllerHandheld: return NpadIdType.Handheld;
                case HidControllerId.ControllerUnknown:  return NpadIdType.Unknown;

                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}