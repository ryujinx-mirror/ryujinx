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
        private const int CursorHideIdleTime = 8; // seconds

        private bool _isDisposed;
        private HideCursor _hideCursor;
        private bool _isHidden;
        private long _lastCursorMoveTime;

        public bool[] PressedButtons { get; }

        public Vector2 CurrentPosition { get; private set; }
        public Vector2 Scroll { get; private set; }
        public Size _clientSize;

        public SDL2MouseDriver(HideCursor hideCursor)
        {
            PressedButtons = new bool[(int)MouseButton.Count];
            _hideCursor = hideCursor;

            if (_hideCursor == HideCursor.Always)
            {
                SDL_ShowCursor(SDL_DISABLE);
                _isHidden = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MouseButton DriverButtonToMouseButton(uint rawButton)
        {
            Debug.Assert(rawButton > 0 && rawButton <= (int)MouseButton.Count);

            return (MouseButton)(rawButton - 1);
        }

        public void UpdatePosition()
        {
            SDL_GetMouseState(out int posX, out int posY);
            Vector2 position = new(posX, posY);

            if (CurrentPosition != position)
            {
                CurrentPosition = position;
                _lastCursorMoveTime = Stopwatch.GetTimestamp();
            }

            CheckIdle();
        }

        private void CheckIdle()
        {
            if (_hideCursor != HideCursor.OnIdle)
            {
                return;
            }

            long cursorMoveDelta = Stopwatch.GetTimestamp() - _lastCursorMoveTime;

            if (cursorMoveDelta >= CursorHideIdleTime * Stopwatch.Frequency)
            {
                if (!_isHidden)
                {
                    SDL_ShowCursor(SDL_DISABLE);
                    _isHidden = true;
                }
            }
            else
            {
                if (_isHidden)
                {
                    SDL_ShowCursor(SDL_ENABLE);
                    _isHidden = false;
                }
            }
        }

        public void Update(SDL_Event evnt)
        {
            switch (evnt.type)
            {
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    uint rawButton = evnt.button.button;

                    if (rawButton > 0 && rawButton <= (int)MouseButton.Count)
                    {
                        PressedButtons[(int)DriverButtonToMouseButton(rawButton)] = evnt.type == SDL_EventType.SDL_MOUSEBUTTONDOWN;

                        CurrentPosition = new Vector2(evnt.button.x, evnt.button.y);
                    }

                    break;

                // NOTE: On Linux using Wayland mouse motion events won't be received at all.
                case SDL_EventType.SDL_MOUSEMOTION:
                    CurrentPosition = new Vector2(evnt.motion.x, evnt.motion.y);
                    _lastCursorMoveTime = Stopwatch.GetTimestamp();

                    break;

                case SDL_EventType.SDL_MOUSEWHEEL:
                    Scroll = new Vector2(evnt.wheel.x, evnt.wheel.y);

                    break;
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