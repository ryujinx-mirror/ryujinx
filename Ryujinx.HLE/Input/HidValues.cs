using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.Input
{
    public partial class Hid
    {
        /*
         * Reference:
         * https://github.com/reswitched/libtransistor/blob/development/lib/hid.c
         * https://github.com/reswitched/libtransistor/blob/development/include/libtransistor/hid.h
         * https://github.com/switchbrew/libnx/blob/master/nx/source/services/hid.c
         * https://github.com/switchbrew/libnx/blob/master/nx/include/switch/services/hid.h
         */

        internal const int HidHeaderSize            = 0x400;
        internal const int HidTouchScreenSize       = 0x3000;
        internal const int HidMouseSize             = 0x400;
        internal const int HidKeyboardSize          = 0x400;
        internal const int HidUnkSection1Size       = 0x400;
        internal const int HidUnkSection2Size       = 0x400;
        internal const int HidUnkSection3Size       = 0x400;
        internal const int HidUnkSection4Size       = 0x400;
        internal const int HidUnkSection5Size       = 0x200;
        internal const int HidUnkSection6Size       = 0x200;
        internal const int HidUnkSection7Size       = 0x200;
        internal const int HidUnkSection8Size       = 0x800;
        internal const int HidControllerSerialsSize = 0x4000;
        internal const int HidControllersSize       = 0x32000;
        internal const int HidUnkSection9Size       = 0x800;

        internal const int HidTouchHeaderSize = 0x28;
        internal const int HidTouchEntrySize  = 0x298;

        internal const int HidTouchEntryHeaderSize = 0x10;
        internal const int HidTouchEntryTouchSize  = 0x28;

        internal const int HidControllerSize        = 0x5000;
        internal const int HidControllerHeaderSize  = 0x28;
        internal const int HidControllerLayoutsSize = 0x350;

        internal const int HidControllersLayoutHeaderSize = 0x20;
        internal const int HidControllersInputEntrySize   = 0x30;

        internal const int HidHeaderOffset            = 0;
        internal const int HidTouchScreenOffset       = HidHeaderOffset            + HidHeaderSize;
        internal const int HidMouseOffset             = HidTouchScreenOffset       + HidTouchScreenSize;
        internal const int HidKeyboardOffset          = HidMouseOffset             + HidMouseSize;
        internal const int HidUnkSection1Offset       = HidKeyboardOffset          + HidKeyboardSize;
        internal const int HidUnkSection2Offset       = HidUnkSection1Offset       + HidUnkSection1Size;
        internal const int HidUnkSection3Offset       = HidUnkSection2Offset       + HidUnkSection2Size;
        internal const int HidUnkSection4Offset       = HidUnkSection3Offset       + HidUnkSection3Size;
        internal const int HidUnkSection5Offset       = HidUnkSection4Offset       + HidUnkSection4Size;
        internal const int HidUnkSection6Offset       = HidUnkSection5Offset       + HidUnkSection5Size;
        internal const int HidUnkSection7Offset       = HidUnkSection6Offset       + HidUnkSection6Size;
        internal const int HidUnkSection8Offset       = HidUnkSection7Offset       + HidUnkSection7Size;
        internal const int HidControllerSerialsOffset = HidUnkSection8Offset       + HidUnkSection8Size;
        internal const int HidControllersOffset       = HidControllerSerialsOffset + HidControllerSerialsSize;
        internal const int HidUnkSection9Offset       = HidControllersOffset       + HidControllersSize;

        internal const int HidEntryCount = 17;
    }
}
