using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Core;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx
{
    public class GLScreen : GameWindow
    {
        private Switch Ns;

        private IGalRenderer Renderer;

        public GLScreen(Switch Ns, IGalRenderer Renderer)
            : base(1280, 720,
            new GraphicsMode(), "Ryujinx", 0,
            DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            this.Ns       = Ns;
            this.Renderer = Renderer;
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;

            Renderer.InitializeFrameBuffer();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            HidControllerKeys CurrentButton = 0;
            JoystickPosition LeftJoystick;
            JoystickPosition RightJoystick;


            if (Keyboard[OpenTK.Input.Key.Escape]) this.Exit();

            //RightJoystick
            int LeftJoystickDX = 0;
            int LeftJoystickDY = 0;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.StickUp]) LeftJoystickDY = short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.StickDown]) LeftJoystickDY = -short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.StickLeft]) LeftJoystickDX = -short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.StickRight]) LeftJoystickDX = short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.StickButton]) CurrentButton |= HidControllerKeys.KEY_LSTICK;

            //LeftButtons
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.DPadUp]) CurrentButton |= HidControllerKeys.KEY_DUP;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.DPadDown]) CurrentButton |= HidControllerKeys.KEY_DDOWN;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.DPadLeft]) CurrentButton |= HidControllerKeys.KEY_DLEFT;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.DPadRight]) CurrentButton |= HidControllerKeys.KEY_DRIGHT;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.ButtonMinus]) CurrentButton |= HidControllerKeys.KEY_MINUS;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.ButtonL]) CurrentButton |= HidControllerKeys.KEY_L;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Left.ButtonZL]) CurrentButton |= HidControllerKeys.KEY_ZL;

            //RightJoystick
            int RightJoystickDX = 0;
            int RightJoystickDY = 0;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.StickUp]) RightJoystickDY = short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.StickDown]) RightJoystickDY = -short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.StickLeft]) RightJoystickDX = -short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.StickRight]) RightJoystickDX = short.MaxValue;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.StickButton]) CurrentButton |= HidControllerKeys.KEY_RSTICK;

            //RightButtons
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonA]) CurrentButton |= HidControllerKeys.KEY_A;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonB]) CurrentButton |= HidControllerKeys.KEY_B;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonX]) CurrentButton |= HidControllerKeys.KEY_X;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonY]) CurrentButton |= HidControllerKeys.KEY_Y;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonPlus]) CurrentButton |= HidControllerKeys.KEY_PLUS;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonR]) CurrentButton |= HidControllerKeys.KEY_R;
            if (Keyboard[(OpenTK.Input.Key)Config.FakeJoyCon.Right.ButtonZR]) CurrentButton |= HidControllerKeys.KEY_ZR;

            LeftJoystick = new JoystickPosition
            {
                DX = LeftJoystickDX,
                DY = LeftJoystickDY
            };

            RightJoystick = new JoystickPosition
            {
                DX = RightJoystickDX,
                DY = RightJoystickDY
            };

            //Get screen touch position from left mouse click
            //Opentk always captures mouse events, even if out of focus, so check if window is focused.
            if (Mouse != null && Focused)
                if (Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed)
                {
                    HidTouchScreenEntryTouch CurrentPoint = new HidTouchScreenEntryTouch
                    {
                        Timestamp = (uint)Environment.TickCount,
                        X = (uint)Mouse.X,
                        Y = (uint)Mouse.Y,

                        //Placeholder values till more data is acquired
                        DiameterX = 10,
                        DiameterY = 10,
                        Angle = 90,

                        //Only support single touch
                        TouchIndex = 0,
                    };
                    if (Mouse.X > -1 && Mouse.Y > -1)
                        Ns.SendTouchScreenEntry(CurrentPoint);
                }

            //We just need one pair of JoyCon because it's emulate by the keyboard.
            Ns.SendControllerButtons(HidControllerID.CONTROLLER_HANDHELD, HidControllerLayouts.Main, CurrentButton, LeftJoystick, RightJoystick);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            Title = $"Ryujinx Screen - (Vsync: {VSync} - FPS: {1f / e.Time:0})";

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Renderer.RunActions();
            Renderer.Render();

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            Renderer.SetWindowSize(Width, Height);
        }
    }
}