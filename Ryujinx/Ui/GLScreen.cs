using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE;
using Ryujinx.HLE.Input;
using System;
using System.Threading;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Ryujinx
{
    public class GLScreen : GameWindow
    {
        private const int TouchScreenWidth  = 1280;
        private const int TouchScreenHeight = 720;

        private const float TouchScreenRatioX = (float)TouchScreenWidth  / TouchScreenHeight;
        private const float TouchScreenRatioY = (float)TouchScreenHeight / TouchScreenWidth;

        private const int TargetFPS = 60;

        private Switch Ns;

        private IGalRenderer Renderer;

        private KeyboardState? Keyboard = null;

        private MouseState? Mouse = null;

        private Thread RenderThread;

        private bool ResizeEvent;

        private bool TitleEvent;

        private string NewTitle;

        public GLScreen(Switch Ns, IGalRenderer Renderer)
            : base(1280, 720,
            new GraphicsMode(), "Ryujinx", 0,
            DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            this.Ns       = Ns;
            this.Renderer = Renderer;

            Location = new Point(
                (DisplayDevice.Default.Width  / 2) - (Width  / 2),
                (DisplayDevice.Default.Height / 2) - (Height / 2));

            ResizeEvent = false;

            TitleEvent = false;
        }

        private void RenderLoop()
        {
            MakeCurrent();

            Stopwatch Chrono = new Stopwatch();

            Chrono.Start();

            long TicksPerFrame = Stopwatch.Frequency / TargetFPS;

            long Ticks = 0;

            while (Exists && !IsExiting)
            {
                if (Ns.WaitFifo())
                {
                    Ns.ProcessFrame();
                }

                Renderer.RunActions();

                if (ResizeEvent)
                {
                    ResizeEvent = false;

                    Renderer.FrameBuffer.SetWindowSize(Width, Height);
                }

                Ticks += Chrono.ElapsedTicks;

                Chrono.Restart();

                if (Ticks >= TicksPerFrame)
                {
                    RenderFrame();

                    //Queue max. 1 vsync
                    Ticks = Math.Min(Ticks - TicksPerFrame, TicksPerFrame);
                }
            }
        }

        public void MainLoop()
        {
            VSync = VSyncMode.Off;

            Visible = true;

            Renderer.FrameBuffer.SetWindowSize(Width, Height);

            Context.MakeCurrent(null);

            //OpenTK doesn't like sleeps in its thread, to avoid this a renderer thread is created
            RenderThread = new Thread(RenderLoop);

            RenderThread.Start();

            while (Exists && !IsExiting)
            {
                ProcessEvents();

                if (!IsExiting)
                {
                    UpdateFrame();

                    if (TitleEvent)
                    {
                        TitleEvent = false;

                        Title = NewTitle;
                    }
                }
            }
        }
        
        private bool IsGamePadButtonPressedFromString(GamePadState GamePad, string Button)
        {
            if (Button.ToUpper() == "LTRIGGER" || Button.ToUpper() == "RTRIGGER")
            {
                return GetGamePadTriggerFromString(GamePad, Button) >= Config.GamePadTriggerThreshold;
            }
            else
            {
                return (GetGamePadButtonFromString(GamePad, Button) == ButtonState.Pressed);
            }
        }

        private ButtonState GetGamePadButtonFromString(GamePadState GamePad, string Button)
        {
            switch (Button.ToUpper())
            {
                case "A":         return GamePad.Buttons.A;
                case "B":         return GamePad.Buttons.B;
                case "X":         return GamePad.Buttons.X;
                case "Y":         return GamePad.Buttons.Y;
                case "LSTICK":    return GamePad.Buttons.LeftStick;
                case "RSTICK":    return GamePad.Buttons.RightStick;
                case "LSHOULDER": return GamePad.Buttons.LeftShoulder;
                case "RSHOULDER": return GamePad.Buttons.RightShoulder;
                case "DPADUP":    return GamePad.DPad.Up;
                case "DPADDOWN":  return GamePad.DPad.Down;
                case "DPADLEFT":  return GamePad.DPad.Left;
                case "DPADRIGHT": return GamePad.DPad.Right;
                case "START":     return GamePad.Buttons.Start;
                case "BACK":      return GamePad.Buttons.Back;
                default:          throw  new ArgumentException();
            }
        }

        private float GetGamePadTriggerFromString(GamePadState GamePad, string Trigger)
        {
            switch (Trigger.ToUpper())
            {
                case "LTRIGGER": return GamePad.Triggers.Left;
                case "RTRIGGER": return GamePad.Triggers.Right;
                default:         throw  new ArgumentException();
            }
        }

        private Vector2 GetJoystickAxisFromString(GamePadState GamePad, string Joystick)
        {
            switch (Joystick.ToUpper())
            {
                case "LJOYSTICK": return GamePad.ThumbSticks.Left;
                case "RJOYSTICK": return new Vector2(-GamePad.ThumbSticks.Right.Y, -GamePad.ThumbSticks.Right.X);
                default:          throw  new ArgumentException();
            }
        }

        private new void UpdateFrame()
        {
            HidControllerButtons CurrentButton = 0;
            HidJoystickPosition  LeftJoystick;
            HidJoystickPosition  RightJoystick;

            int LeftJoystickDX        = 0;
            int LeftJoystickDY        = 0;
            int RightJoystickDX       = 0;
            int RightJoystickDY       = 0;
            float AnalogStickDeadzone = Config.GamePadDeadzone;

            //Keyboard Input
            if (Keyboard.HasValue)
            {
                KeyboardState Keyboard = this.Keyboard.Value;

                if (Keyboard[Key.Escape]) this.Exit();

                //LeftJoystick
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.StickUp])    LeftJoystickDY = short.MaxValue;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.StickDown])  LeftJoystickDY = -short.MaxValue;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.StickLeft])  LeftJoystickDX = -short.MaxValue;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.StickRight]) LeftJoystickDX = short.MaxValue;

                //LeftButtons
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.StickButton]) CurrentButton |= HidControllerButtons.KEY_LSTICK;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.DPadUp])      CurrentButton |= HidControllerButtons.KEY_DUP;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.DPadDown])    CurrentButton |= HidControllerButtons.KEY_DDOWN;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.DPadLeft])    CurrentButton |= HidControllerButtons.KEY_DLEFT;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.DPadRight])   CurrentButton |= HidControllerButtons.KEY_DRIGHT;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.ButtonMinus]) CurrentButton |= HidControllerButtons.KEY_MINUS;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.ButtonL])     CurrentButton |= HidControllerButtons.KEY_L;
                if (Keyboard[(Key)Config.JoyConKeyboard.Left.ButtonZL])    CurrentButton |= HidControllerButtons.KEY_ZL;

                //RightJoystick
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.StickUp])    RightJoystickDY = short.MaxValue;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.StickDown])  RightJoystickDY = -short.MaxValue;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.StickLeft])  RightJoystickDX = -short.MaxValue;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.StickRight]) RightJoystickDX = short.MaxValue;

                //RightButtons
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.StickButton]) CurrentButton |= HidControllerButtons.KEY_RSTICK;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonA])     CurrentButton |= HidControllerButtons.KEY_A;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonB])     CurrentButton |= HidControllerButtons.KEY_B;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonX])     CurrentButton |= HidControllerButtons.KEY_X;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonY])     CurrentButton |= HidControllerButtons.KEY_Y;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonPlus])  CurrentButton |= HidControllerButtons.KEY_PLUS;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonR])     CurrentButton |= HidControllerButtons.KEY_R;
                if (Keyboard[(Key)Config.JoyConKeyboard.Right.ButtonZR])    CurrentButton |= HidControllerButtons.KEY_ZR;
            }

            //Controller Input
            if (Config.GamePadEnable)
            {
                GamePadState GamePad = OpenTK.Input.GamePad.GetState(Config.GamePadIndex);
                //LeftButtons
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.DPadUp))       CurrentButton |= HidControllerButtons.KEY_DUP;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.DPadDown))     CurrentButton |= HidControllerButtons.KEY_DDOWN;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.DPadLeft))     CurrentButton |= HidControllerButtons.KEY_DLEFT;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.DPadRight))    CurrentButton |= HidControllerButtons.KEY_DRIGHT;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.StickButton))  CurrentButton |= HidControllerButtons.KEY_LSTICK;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.ButtonMinus))  CurrentButton |= HidControllerButtons.KEY_MINUS;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.ButtonL))      CurrentButton |= HidControllerButtons.KEY_L;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Left.ButtonZL))     CurrentButton |= HidControllerButtons.KEY_ZL;

                //RightButtons
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonA))     CurrentButton |= HidControllerButtons.KEY_A;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonB))     CurrentButton |= HidControllerButtons.KEY_B;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonX))     CurrentButton |= HidControllerButtons.KEY_X;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonY))     CurrentButton |= HidControllerButtons.KEY_Y;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.StickButton)) CurrentButton |= HidControllerButtons.KEY_RSTICK;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonPlus))  CurrentButton |= HidControllerButtons.KEY_PLUS;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonR))     CurrentButton |= HidControllerButtons.KEY_R;
                if (IsGamePadButtonPressedFromString(GamePad, Config.JoyConController.Right.ButtonZR))    CurrentButton |= HidControllerButtons.KEY_ZR;

                //LeftJoystick
                if (GetJoystickAxisFromString(GamePad, Config.JoyConController.Left.Stick).X >= AnalogStickDeadzone
                 || GetJoystickAxisFromString(GamePad, Config.JoyConController.Left.Stick).X <= -AnalogStickDeadzone)
                    LeftJoystickDX = (int)(GetJoystickAxisFromString(GamePad, Config.JoyConController.Left.Stick).X * short.MaxValue);

                if (GetJoystickAxisFromString(GamePad, Config.JoyConController.Left.Stick).Y >= AnalogStickDeadzone
                 || GetJoystickAxisFromString(GamePad, Config.JoyConController.Left.Stick).Y <= -AnalogStickDeadzone)
                    LeftJoystickDY = (int)(GetJoystickAxisFromString(GamePad, Config.JoyConController.Left.Stick).Y * short.MaxValue);

                //RightJoystick
                if (GetJoystickAxisFromString(GamePad, Config.JoyConController.Right.Stick).X >= AnalogStickDeadzone
                 || GetJoystickAxisFromString(GamePad, Config.JoyConController.Right.Stick).X <= -AnalogStickDeadzone)
                    RightJoystickDX = (int)(GetJoystickAxisFromString(GamePad, Config.JoyConController.Right.Stick).X * short.MaxValue);

                if (GetJoystickAxisFromString(GamePad, Config.JoyConController.Right.Stick).Y >= AnalogStickDeadzone
                 || GetJoystickAxisFromString(GamePad, Config.JoyConController.Right.Stick).Y <= -AnalogStickDeadzone)
                    RightJoystickDY = (int)(GetJoystickAxisFromString(GamePad, Config.JoyConController.Right.Stick).Y * short.MaxValue);
            }

            LeftJoystick = new HidJoystickPosition
            {
                DX = LeftJoystickDX,
                DY = LeftJoystickDY
            };

            RightJoystick = new HidJoystickPosition
            {
                DX = RightJoystickDX,
                DY = RightJoystickDY
            };

            bool HasTouch = false;

            //Get screen touch position from left mouse click
            //OpenTK always captures mouse events, even if out of focus, so check if window is focused.
            if (Focused && Mouse?.LeftButton == ButtonState.Pressed)
            {
                MouseState Mouse = this.Mouse.Value;

                int ScrnWidth  = Width;
                int ScrnHeight = Height;

                if (Width > Height * TouchScreenRatioX)
                {
                    ScrnWidth = (int)(Height * TouchScreenRatioX);
                }
                else
                {
                    ScrnHeight = (int)(Width * TouchScreenRatioY);
                }

                int StartX = (Width  - ScrnWidth)  >> 1;
                int StartY = (Height - ScrnHeight) >> 1;

                int EndX = StartX + ScrnWidth;
                int EndY = StartY + ScrnHeight;

                if (Mouse.X >= StartX &&
                    Mouse.Y >= StartY &&
                    Mouse.X <  EndX   &&
                    Mouse.Y <  EndY)
                {
                    int ScrnMouseX = Mouse.X - StartX;
                    int ScrnMouseY = Mouse.Y - StartY;

                    int MX = (int)(((float)ScrnMouseX / ScrnWidth)  * TouchScreenWidth);
                    int MY = (int)(((float)ScrnMouseY / ScrnHeight) * TouchScreenHeight);

                    HidTouchPoint CurrentPoint = new HidTouchPoint
                    {
                        X = MX,
                        Y = MY,

                        //Placeholder values till more data is acquired
                        DiameterX = 10,
                        DiameterY = 10,
                        Angle     = 90
                    };

                    HasTouch = true;

                    Ns.Hid.SetTouchPoints(CurrentPoint);
                }
            }

            if (!HasTouch)
            {
                Ns.Hid.SetTouchPoints();
            }

            Ns.Hid.SetJoyconButton(
                HidControllerId.CONTROLLER_HANDHELD,
                HidControllerLayouts.Handheld_Joined,
                CurrentButton,
                LeftJoystick,
                RightJoystick);

            Ns.Hid.SetJoyconButton(
                HidControllerId.CONTROLLER_HANDHELD,
                HidControllerLayouts.Main,
                CurrentButton,
                LeftJoystick,
                RightJoystick);
        }

        private new void RenderFrame()
        {
            Renderer.FrameBuffer.Render();

            Ns.Statistics.RecordSystemFrameTime();

            double HostFps = Ns.Statistics.GetSystemFrameRate();
            double GameFps = Ns.Statistics.GetGameFrameRate();

            NewTitle = $"Ryujinx | Host FPS: {HostFps:0.0} | Game FPS: {GameFps:0.0}";

            TitleEvent = true;

            SwapBuffers();

            Ns.Os.SignalVsync();
        }

        protected override void OnUnload(EventArgs e)
        {
            RenderThread.Join();

            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            ResizeEvent = true;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            Keyboard = e.Keyboard;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Keyboard = e.Keyboard;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Mouse = e.Mouse;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Mouse = e.Mouse;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            Mouse = e.Mouse;
        }
    }
}