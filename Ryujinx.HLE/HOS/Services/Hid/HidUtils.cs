using Ryujinx.HLE.Input;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    static class HidUtils
    {
        public static ControllerId GetIndexFromNpadIdType(NpadIdType npadIdType)
        {
            switch (npadIdType)
            {
                case NpadIdType.Player1:  return ControllerId.ControllerPlayer1;
                case NpadIdType.Player2:  return ControllerId.ControllerPlayer2;
                case NpadIdType.Player3:  return ControllerId.ControllerPlayer3;
                case NpadIdType.Player4:  return ControllerId.ControllerPlayer4;
                case NpadIdType.Player5:  return ControllerId.ControllerPlayer5;
                case NpadIdType.Player6:  return ControllerId.ControllerPlayer6;
                case NpadIdType.Player7:  return ControllerId.ControllerPlayer7;
                case NpadIdType.Player8:  return ControllerId.ControllerPlayer8;
                case NpadIdType.Handheld: return ControllerId.ControllerHandheld;
                case NpadIdType.Unknown:  return ControllerId.ControllerUnknown;

                default: throw new ArgumentOutOfRangeException(nameof(npadIdType));
            }
        }

        public static NpadIdType GetNpadIdTypeFromIndex(ControllerId index)
        {
            switch (index)
            {
                case ControllerId.ControllerPlayer1:  return NpadIdType.Player1;
                case ControllerId.ControllerPlayer2:  return NpadIdType.Player2;
                case ControllerId.ControllerPlayer3:  return NpadIdType.Player3;
                case ControllerId.ControllerPlayer4:  return NpadIdType.Player4;
                case ControllerId.ControllerPlayer5:  return NpadIdType.Player5;
                case ControllerId.ControllerPlayer6:  return NpadIdType.Player6;
                case ControllerId.ControllerPlayer7:  return NpadIdType.Player7;
                case ControllerId.ControllerPlayer8:  return NpadIdType.Player8;
                case ControllerId.ControllerHandheld: return NpadIdType.Handheld;
                case ControllerId.ControllerUnknown:  return NpadIdType.Unknown;

                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}