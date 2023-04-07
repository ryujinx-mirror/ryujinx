using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.Core;
using Ryujinx.Input;
using System;
using System.Numerics;
using MouseButton = Ryujinx.Input.MouseButton;
using Size = System.Drawing.Size;

namespace Ryujinx.Ava.Input
{
    internal class AvaloniaMouseDriver : IGamepadDriver
    {
        private Control           _widget;
        private bool              _isDisposed;
        private Size              _size;
        private readonly TopLevel _window;

        public bool[]  PressedButtons  { get; }
        public Vector2 CurrentPosition { get; private set; }
        public Vector2 Scroll          { get; private set; }

        public string               DriverName  => "AvaloniaMouseDriver";
        public ReadOnlySpan<string> GamepadsIds => new[] { "0" };

        public AvaloniaMouseDriver(TopLevel window, Control parent)
        {
            _widget = parent;
            _window = window;

            _widget.PointerMoved        += Parent_PointerMovedEvent;
            _widget.PointerPressed      += Parent_PointerPressEvent;
            _widget.PointerReleased     += Parent_PointerReleaseEvent;
            _widget.PointerWheelChanged += Parent_ScrollEvent;
            
            _window.PointerMoved        += Parent_PointerMovedEvent;
            _window.PointerPressed      += Parent_PointerPressEvent;
            _window.PointerReleased     += Parent_PointerReleaseEvent;
            _window.PointerWheelChanged += Parent_ScrollEvent;

            PressedButtons = new bool[(int)MouseButton.Count];

            _size = new Size((int)parent.Bounds.Width, (int)parent.Bounds.Height);

            parent.GetObservable(Visual.BoundsProperty).Subscribe(Resized);
        }

        public event Action<string> OnGamepadConnected
        {
            add    { }
            remove { }
        }

        public event Action<string> OnGamepadDisconnected
        {
            add    { }
            remove { }
        }

        private void Resized(Rect rect)
        {
            _size = new Size((int)rect.Width, (int)rect.Height);
        }

        private void Parent_ScrollEvent(object o, PointerWheelEventArgs args)
        {
            Scroll = new Vector2((float)args.Delta.X, (float)args.Delta.Y);
        }

        private void Parent_PointerReleaseEvent(object o, PointerReleasedEventArgs args)
        {
            int button = (int)args.InitialPressMouseButton - 1;

            if (PressedButtons.Count() >= button)
            {
                PressedButtons[button] = false;
            }
        }

        private void Parent_PointerPressEvent(object o, PointerPressedEventArgs args)
        {
            int button = (int)args.GetCurrentPoint(_widget).Properties.PointerUpdateKind;

            if (PressedButtons.Count() >= button)
            {
                PressedButtons[button] = true;
            }
        }

        private void Parent_PointerMovedEvent(object o, PointerEventArgs args)
        {
            Point position = args.GetPosition(_widget);

            CurrentPosition = new Vector2((float)position.X, (float)position.Y);
        }

        public void SetMousePressed(MouseButton button)
        {
            if (PressedButtons.Count() >= (int)button)
            {
                PressedButtons[(int)button] = true;
            }
        }

        public void SetMouseReleased(MouseButton button)
        {
            if (PressedButtons.Count() >= (int)button)
            {
                PressedButtons[(int)button] = false;
            }
        }

        public void SetPosition(double x, double y)
        {
            CurrentPosition = new Vector2((float)x, (float)y);
        }

        public bool IsButtonPressed(MouseButton button)
        {
            if (PressedButtons.Count() >= (int)button)
            {
                return PressedButtons[(int)button];
            }

            return false;
        }

        public Size GetClientSize()
        {
            return _size;
        }

        public IGamepad GetGamepad(string id)
        {
            return new AvaloniaMouse(this);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _widget.PointerMoved        -= Parent_PointerMovedEvent;
            _widget.PointerPressed      -= Parent_PointerPressEvent;
            _widget.PointerReleased     -= Parent_PointerReleaseEvent;
            _widget.PointerWheelChanged -= Parent_ScrollEvent;

            _window.PointerMoved        -= Parent_PointerMovedEvent;
            _window.PointerPressed      -= Parent_PointerPressEvent;
            _window.PointerReleased     -= Parent_PointerReleaseEvent;
            _window.PointerWheelChanged -= Parent_ScrollEvent;

            _widget = null;
        }
    }
}