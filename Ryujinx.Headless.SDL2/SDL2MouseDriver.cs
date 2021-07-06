using Ryujinx.Common.Logging;
using Ryujinx.Input;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using static SDL2.SDL;

namespace Ryujinx.Headless.SDL2
{
    class SDL2MouseDriver : IGamepadDriver
    {
        private bool _isDisposed;

        public bool[] PressedButtons { get; }

        public Vector2 CurrentPosition { get; private set; }
        public Vector2 Scroll { get; private set; }
        public Size _clientSize;

        public SDL2MouseDriver()
        {
            PressedButtons = new bool[(int)MouseButton.Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MouseButton DriverButtonToMouseButton(uint rawButton)
        {
            Debug.Assert(rawButton > 0 && rawButton <= (int)MouseButton.Count);

            return (MouseButton)(rawButton - 1);
        }

        public void Update(SDL_Event evnt)
        {
            if (evnt.type == SDL_EventType.SDL_MOUSEBUTTONDOWN || evnt.type == SDL_EventType.SDL_MOUSEBUTTONUP)
            {
                uint rawButton = evnt.button.button;

                if (rawButton > 0 && rawButton <= (int)MouseButton.Count)
                {
                    PressedButtons[(int)DriverButtonToMouseButton(rawButton)] = evnt.type == SDL_EventType.SDL_MOUSEBUTTONDOWN;

                    CurrentPosition = new Vector2(evnt.button.x, evnt.button.y);
                }
            }
            else if (evnt.type == SDL_EventType.SDL_MOUSEMOTION)
            {
                CurrentPosition = new Vector2(evnt.motion.x, evnt.motion.y);
            }
            else if (evnt.type == SDL_EventType.SDL_MOUSEWHEEL)
            {
                Scroll = new Vector2(evnt.wheel.x, evnt.wheel.y);
            }
        }

        public void SetClientSize(int width, int height)
        {
            _clientSize = new Size(width, height);
        }

        public bool IsButtonPressed(MouseButton button)
        {
            return PressedButtons[(int)button];
        }

        public Size GetClientSize()
        {
            return _clientSize;
        }

        public string DriverName => "SDL2";

        public event Action<string> OnGamepadConnected
        {
            add { }
            remove { }
        }

        public event Action<string> OnGamepadDisconnected
        {
            add { }
            remove { }
        }

        public ReadOnlySpan<string> GamepadsIds => new[] { "0" };

        public IGamepad GetGamepad(string id)
        {
            return new SDL2Mouse(this);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }
    }
}
