using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.UI.Input
{
    public struct JoyConControllerLeft
    {
        public string Stick;
        public string StickButton;
        public string DPadUp;
        public string DPadDown;
        public string DPadLeft;
        public string DPadRight;
        public string ButtonMinus;
        public string ButtonL;
        public string ButtonZL;
    }

    public struct JoyConControllerRight
    {
        public string Stick;
        public string StickButton;
        public string ButtonA;
        public string ButtonB;
        public string ButtonX;
        public string ButtonY;
        public string ButtonPlus;
        public string ButtonR;
        public string ButtonZR;
    }

    public struct JoyConController
    {
        public JoyConControllerLeft Left;
        public JoyConControllerRight Right;
    }
}
