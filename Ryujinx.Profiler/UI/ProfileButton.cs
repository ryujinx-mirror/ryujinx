using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Profiler.UI.SharpFontHelpers;

namespace Ryujinx.Profiler.UI
{
    public class ProfileButton
    {
        // Store font service
        private FontService _fontService;

        // Layout information
        private int _left, _right;
        private int _bottom, _top;
        private int _height;
        private int _padding;

        // Label information
        private int    _labelX, _labelY;
        private string _label;

        // Misc
        private Action _clicked;
        private bool   _visible;

        public ProfileButton(FontService fontService, Action clicked)
            : this(fontService, clicked, 0, 0, 0, 0, 0)
        {
            _visible = false;
        }

        public ProfileButton(FontService fontService, Action clicked, int x, int y, int padding, int height, int width)
            : this(fontService, "", clicked, x, y, padding, height, width)
        {
            _visible = false;
        }

        public ProfileButton(FontService fontService, string label, Action clicked, int x, int y, int padding, int height, int width = -1)
        {
            _fontService = fontService;
            _clicked     = clicked;

            UpdateSize(label, x, y, padding, height, width);
        }

        public int UpdateSize(string label, int x, int y, int padding, int height, int width = -1)
        {
            _visible = true;
            _label   = label;

            if (width == -1)
            {
                // Dummy draw to measure size
                width = (int)_fontService.DrawText(label, 0, 0, height, false);
            }

            UpdateSize(x, y, padding, width, height);

            return _right - _left;
        }

        public void UpdateSize(int x, int y, int padding, int width, int height)
        {
            _height = height;
            _left   = x;
            _bottom = y;
            _labelX = x + padding / 2;
            _labelY = y + padding / 2;
            _top    = y + height + padding;
            _right  = x + width + padding;
        }

        public void Draw()
        {
            if (!_visible)
            {
                return;
            }

            // Draw backing rectangle
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.Black);
            GL.Vertex2(_left,  _bottom);
            GL.Vertex2(_left,  _top);
            GL.Vertex2(_right, _top);

            GL.Vertex2(_right, _top);
            GL.Vertex2(_right, _bottom);
            GL.Vertex2(_left,  _bottom);
            GL.End();

            // Use font service to draw label
            _fontService.DrawText(_label, _labelX, _labelY, _height);
        }

        public bool ProcessClick(int x, int y)
        {
            // If button contains x, y
            if (x > _left   && x < _right &&
                y > _bottom && y < _top)
            {
                _clicked();
                return true;
            }

            return false;
        }
    }
}
