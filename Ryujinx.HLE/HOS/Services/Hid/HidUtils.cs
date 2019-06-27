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
    }
}