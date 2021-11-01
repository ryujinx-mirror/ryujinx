using Ryujinx.HLE.Ui;
using Ryujinx.Memory;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Class that generates the graphics for the software keyboard applet during inline mode.
    /// </summary>
    internal class SoftwareKeyboardRenderer : IDisposable
    {
        const int TextBoxBlinkThreshold            = 8;
        const int TextBoxBlinkSleepMilliseconds    = 100;
        const int TextBoxBlinkJoinWaitMilliseconds = 1000;

        const string MessageText          = "Please use the keyboard to input text";
        const string AcceptText           = "Accept";
        const string CancelText           = "Cancel";
        const string ControllerToggleText = "Toggle input";

        private RenderingSurfaceInfo _surfaceInfo;
        private Bitmap               _surface    = null;
        private object               _renderLock = new object();

        private string _inputText         = "";
        private int    _cursorStart       = 0;
        private int    _cursorEnd         = 0;
        private bool   _acceptPressed     = false;
        private bool   _cancelPressed     = false;
        private bool   _overwriteMode     = false;
        private bool   _typingEnabled     = true;
        private bool   _controllerEnabled = true;

        private Image _ryujinxLogo   = null;
        private Image _padAcceptIcon = null;
        private Image _padCancelIcon = null;
        private Image _keyModeIcon   = null;

        private float _textBoxOutlineWidth;
        private float _padPressedPenWidth;

        private Brush _panelBrush;
        private Brush _disabledBrush;
        private Brush _textNormalBrush;
        private Brush _textSelectedBrush;
        private Brush _textOverCursorBrush;
        private Brush _cursorBrush;
        private Brush _selectionBoxBrush;
        private Brush _keyCapBrush;
        private Brush _keyProgressBrush;

        private Pen _gridSeparatorPen;
        private Pen _textBoxOutlinePen;
        private Pen _cursorPen;
        private Pen _selectionBoxPen;
        private Pen _padPressedPen;

        private int  _inputTextFontSize;
        private int  _padButtonFontSize;
        private Font _messageFont;
        private Font _inputTextFont;
        private Font _labelsTextFont;
        private Font _padSymbolFont;
        private Font _keyCapFont;

        private float      _inputTextCalibrationHeight;
        private float      _panelPositionY;
        private RectangleF _panelRectangle;
        private PointF     _logoPosition;
        private float      _messagePositionY;

        private TRef<int>   _textBoxBlinkCounter     = new TRef<int>(0);
        private TimedAction _textBoxBlinkTimedAction = new TimedAction();

        public SoftwareKeyboardRenderer(IHostUiTheme uiTheme)
        {
            _surfaceInfo = new RenderingSurfaceInfo(0, 0, 0, 0, 0);

            string ryujinxLogoPath = "Ryujinx.Ui.Resources.Logo_Ryujinx.png";
            int    ryujinxLogoSize = 32;

            _ryujinxLogo = LoadResource(Assembly.GetEntryAssembly(), ryujinxLogoPath, ryujinxLogoSize, ryujinxLogoSize);

            string padAcceptIconPath = "Ryujinx.HLE.HOS.Applets.SoftwareKeyboard.Resources.Icon_BtnA.png";
            string padCancelIconPath = "Ryujinx.HLE.HOS.Applets.SoftwareKeyboard.Resources.Icon_BtnB.png";
            string keyModeIconPath   = "Ryujinx.HLE.HOS.Applets.SoftwareKeyboard.Resources.Icon_KeyF6.png";

            _padAcceptIcon = LoadResource(Assembly.GetExecutingAssembly(), padAcceptIconPath  , 0, 0);
            _padCancelIcon = LoadResource(Assembly.GetExecutingAssembly(), padCancelIconPath  , 0, 0);
            _keyModeIcon   = LoadResource(Assembly.GetExecutingAssembly(), keyModeIconPath    , 0, 0);

            Color panelColor               = ToColor(uiTheme.DefaultBackgroundColor, 255);
            Color panelTransparentColor    = ToColor(uiTheme.DefaultBackgroundColor, 150);
            Color normalTextColor          = ToColor(uiTheme.DefaultForegroundColor);
            Color invertedTextColor        = ToColor(uiTheme.DefaultForegroundColor, null, true);
            Color selectedTextColor        = ToColor(uiTheme.SelectionForegroundColor);
            Color borderColor              = ToColor(uiTheme.DefaultBorderColor);
            Color selectionBackgroundColor = ToColor(uiTheme.SelectionBackgroundColor);
            Color gridSeparatorColor       = Color.FromArgb(180, 255, 255, 255);

            float cursorWidth = 2;

            _textBoxOutlineWidth = 2;
            _padPressedPenWidth  = 2;

            _panelBrush          = new SolidBrush(panelColor);
            _disabledBrush       = new SolidBrush(panelTransparentColor);
            _textNormalBrush     = new SolidBrush(normalTextColor);
            _textSelectedBrush   = new SolidBrush(selectedTextColor);
            _textOverCursorBrush = new SolidBrush(invertedTextColor);
            _cursorBrush         = new SolidBrush(normalTextColor);
            _selectionBoxBrush   = new SolidBrush(selectionBackgroundColor);
            _keyCapBrush         = Brushes.White;
            _keyProgressBrush    = new SolidBrush(borderColor);

            _gridSeparatorPen    = new Pen(gridSeparatorColor, 2);
            _textBoxOutlinePen   = new Pen(borderColor, _textBoxOutlineWidth);
            _cursorPen           = new Pen(normalTextColor, cursorWidth);
            _selectionBoxPen     = new Pen(selectionBackgroundColor, cursorWidth);
            _padPressedPen       = new Pen(borderColor, _padPressedPenWidth);

            _inputTextFontSize = 20;
            _padButtonFontSize = 24;

            string font = uiTheme.FontFamily;

            _messageFont    = new Font(font, 26,                 FontStyle.Regular, GraphicsUnit.Pixel);
            _inputTextFont  = new Font(font, _inputTextFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            _labelsTextFont = new Font(font, 24,                 FontStyle.Regular, GraphicsUnit.Pixel);
            _padSymbolFont  = new Font(font, _padButtonFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            _keyCapFont     = new Font(font, 15,                 FontStyle.Regular, GraphicsUnit.Pixel);

            // System.Drawing has serious problems measuring strings, so it requires a per-pixel calibration
            // to ensure we are rendering text inside the proper region
            _inputTextCalibrationHeight = CalibrateTextHeight(_inputTextFont);

            StartTextBoxBlinker(_textBoxBlinkTimedAction, _textBoxBlinkCounter);
        }

        private static void StartTextBoxBlinker(TimedAction timedAction, TRef<int> blinkerCounter)
        {
            timedAction.Reset(() =>
            {
                // The blinker is on falf of the time and events such as input
                // changes can reset the blinker.
                var value = Volatile.Read(ref blinkerCounter.Value);
                value = (value + 1) % (2 * TextBoxBlinkThreshold);
                Volatile.Write(ref blinkerCounter.Value, value);

            }, TextBoxBlinkSleepMilliseconds);
        }

        private Color ToColor(ThemeColor color, byte? overrideAlpha = null, bool flipRgb = false)
        {
            var a = (byte)(color.A * 255);
            var r = (byte)(color.R * 255);
            var g = (byte)(color.G * 255);
            var b = (byte)(color.B * 255);

            if (flipRgb)
            {
                r = (byte)(255 - r);
                g = (byte)(255 - g);
                b = (byte)(255 - b);
            }

            return Color.FromArgb(overrideAlpha.GetValueOrDefault(a), r, g, b);
        }

        private Image LoadResource(Assembly assembly, string resourcePath, int newWidth, int newHeight)
        {
            Stream resourceStream = assembly.GetManifestResourceStream(resourcePath);

            Debug.Assert(resourceStream != null);

            var originalImage = Image.FromStream(resourceStream);

            if (newHeight == 0 || newWidth == 0)
            {
                return originalImage;
            }

            var newSize = new Rectangle(0, 0, newWidth, newHeight);
            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = System.Drawing.Graphics.FromImage(newImage))
            using (var wrapMode = new ImageAttributes())
            {
                graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.CompositingMode    = CompositingMode.SourceCopy;
                graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode      = SmoothingMode.HighQuality;

                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(originalImage, newSize, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return newImage;
        }

#pragma warning disable CS8632
        public void UpdateTextState(string? inputText, int? cursorStart, int? cursorEnd, bool? overwriteMode, bool? typingEnabled)
#pragma warning restore CS8632
        {
            lock (_renderLock)
            {
                // Update the parameters that were provided.
                _inputText     = inputText != null ? inputText : _inputText;
                _cursorStart   = cursorStart.GetValueOrDefault(_cursorStart);
                _cursorEnd     = cursorEnd.GetValueOrDefault(_cursorEnd);
                _overwriteMode = overwriteMode.GetValueOrDefault(_overwriteMode);
                _typingEnabled = typingEnabled.GetValueOrDefault(_typingEnabled);

                // Reset the cursor blink.
                Volatile.Write(ref _textBoxBlinkCounter.Value, 0);
            }
        }

        public void UpdateCommandState(bool? acceptPressed, bool? cancelPressed, bool? controllerEnabled)
        {
            lock (_renderLock)
            {
                // Update the parameters that were provided.
                _acceptPressed     = acceptPressed.GetValueOrDefault(_acceptPressed);
                _cancelPressed     = cancelPressed.GetValueOrDefault(_cancelPressed);
                _controllerEnabled = controllerEnabled.GetValueOrDefault(_controllerEnabled);
            }
        }

        private void Redraw()
        {
            if (_surface == null)
            {
                return;
            }

            using (var graphics = CreateGraphics())
            {
                var    messageRectangle = MeasureString(graphics, MessageText, _messageFont);
                float  messagePositionX = (_panelRectangle.Width - messageRectangle.Width) / 2 - messageRectangle.X;
                float  messagePositionY = _messagePositionY - messageRectangle.Y;
                PointF messagePosition  = new PointF(messagePositionX, messagePositionY);

                graphics.Clear(Color.Transparent);
                graphics.TranslateTransform(0, _panelPositionY);
                graphics.FillRectangle(_panelBrush, _panelRectangle);
                graphics.DrawImage(_ryujinxLogo, _logoPosition);

                DrawString(graphics, MessageText, _messageFont, _textNormalBrush, messagePosition);

                if (!_typingEnabled)
                {
                    // Just draw a semi-transparent rectangle on top to fade the component with the background.
                    // TODO (caian): This will not work if one decides to add make background semi-transparent as well.
                    graphics.FillRectangle(_disabledBrush, messagePositionX, messagePositionY, messageRectangle.Width, messageRectangle.Height);
                }

                DrawTextBox(graphics);

                float halfWidth = _panelRectangle.Width / 2;

                PointF acceptButtonPosition  = new PointF(halfWidth - 180, 185);
                PointF cancelButtonPosition  = new PointF(halfWidth      , 185);
                PointF disableButtonPosition = new PointF(halfWidth + 180, 185);

                DrawPadButton       (graphics, acceptButtonPosition , _padAcceptIcon, AcceptText, _acceptPressed, _controllerEnabled);
                DrawPadButton       (graphics, cancelButtonPosition , _padCancelIcon, CancelText, _cancelPressed, _controllerEnabled);
                DrawControllerToggle(graphics, disableButtonPosition, _controllerEnabled);
            }
        }

        private void RecreateSurface()
        {
            Debug.Assert(_surfaceInfo.ColorFormat == Services.SurfaceFlinger.ColorFormat.A8B8G8R8);

            // Use the whole area of the image to draw, even the alignment, otherwise it may shear the final
            // image if the pitch is different.
            uint totalWidth  = _surfaceInfo.Pitch / 4;
            uint totalHeight = _surfaceInfo.Size / _surfaceInfo.Pitch;

            Debug.Assert(_surfaceInfo.Width <= totalWidth);
            Debug.Assert(_surfaceInfo.Height <= totalHeight);
            Debug.Assert(_surfaceInfo.Pitch * _surfaceInfo.Height <= _surfaceInfo.Size);

            _surface = new Bitmap((int)totalWidth, (int)totalHeight, PixelFormat.Format32bppArgb);
        }

        private void RecomputeConstants()
        {
            float totalWidth  = _surfaceInfo.Width;
            float totalHeight = _surfaceInfo.Height;

            float panelHeight = 240;

            _panelPositionY = totalHeight - panelHeight;
            _panelRectangle = new RectangleF(0, 0, totalWidth, panelHeight);

            _messagePositionY = 60;

            float logoPositionX = (totalWidth - _ryujinxLogo.Width) / 2;
            float logoPositionY = 18;

            _logoPosition = new PointF(logoPositionX, logoPositionY);
        }

        private StringFormat CreateStringFormat(string text)
        {
            StringFormat format = new StringFormat(StringFormat.GenericTypographic);
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });

            return format;
        }

        private RectangleF MeasureString(System.Drawing.Graphics graphics, string text, System.Drawing.Font font)
        {
            bool isEmpty = false;

            if (string.IsNullOrEmpty(text))
            {
                isEmpty = true;
                text = " ";
            }

            var format    = CreateStringFormat(text);
            var rectangle = new RectangleF(0, 0, float.PositiveInfinity, float.PositiveInfinity);
            var regions   = graphics.MeasureCharacterRanges(text, font, rectangle, format);

            Debug.Assert(regions.Length == 1);

            rectangle = regions[0].GetBounds(graphics);

            if (isEmpty)
            {
                rectangle.Width = 0;
            }
            else
            {
                rectangle.Width += 1.0f;
            }

            return rectangle;
        }

        private float CalibrateTextHeight(Font font)
        {
            // This is a pixel-wise calibration that tests the offset of a reference character because Windows text measurement
            // is horrible when compared to other frameworks like Cairo and diverge across systems and fonts.

            Debug.Assert(font.Unit == GraphicsUnit.Pixel);

            var surfaceSize = (int)Math.Ceiling(2 * font.Size);

            string calibrationText = "|";

            using (var surface = new Bitmap(surfaceSize, surfaceSize, PixelFormat.Format32bppArgb))
            using (var graphics = CreateGraphics(surface))
            {
                var measuredRectangle = MeasureString(graphics, calibrationText, font);

                Debug.Assert(measuredRectangle.Right  <= surfaceSize);
                Debug.Assert(measuredRectangle.Bottom <= surfaceSize);

                var textPosition = new PointF(0, 0);

                graphics.Clear(Color.Transparent);
                DrawString(graphics, calibrationText, font, Brushes.White, textPosition);

                var lockRectangle = new Rectangle(0, 0, surface.Width, surface.Height);
                var surfaceData   = surface.LockBits(lockRectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var surfaceBytes  = new byte[surfaceData.Stride * surfaceData.Height];

                Marshal.Copy(surfaceData.Scan0, surfaceBytes, 0, surfaceBytes.Length);

                Point topLeft    = new Point();
                Point bottomLeft = new Point();

                bool foundTopLeft = false;

                for (int y = 0; y < surfaceData.Height; y++)
                {
                    for (int x = 0; x < surfaceData.Stride; x += 4)
                    {
                        int position = y * surfaceData.Stride + x;

                        if (surfaceBytes[position] != 0)
                        {
                            if (!foundTopLeft)
                            {
                                topLeft.X    = x;
                                topLeft.Y    = y;
                                foundTopLeft = true;

                                break;
                            }
                            else
                            {
                                bottomLeft.X = x;
                                bottomLeft.Y = y;

                                break;
                            }
                        }
                    }
                }

                return bottomLeft.Y - topLeft.Y;
            }
        }

        private void DrawString(System.Drawing.Graphics graphics, string text, Font font, Brush brush, PointF point)
        {
            var format = CreateStringFormat(text);
            graphics.DrawString(text, font, brush, point, format);
        }

        private System.Drawing.Graphics CreateGraphics()
        {
            return CreateGraphics(_surface);
        }

        private System.Drawing.Graphics CreateGraphics(Image surface)
        {
            var graphics = System.Drawing.Graphics.FromImage(surface);

            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;

            return graphics;
        }

        private void DrawTextBox(System.Drawing.Graphics graphics)
        {
            var inputTextRectangle = MeasureString(graphics, _inputText, _inputTextFont);

            float boxWidth  = (int)(Math.Max(300, inputTextRectangle.Width + inputTextRectangle.X + 8));
            float boxHeight = 32;
            float boxY      = 110;
            float boxX      = (int)((_panelRectangle.Width - boxWidth) / 2);

            graphics.DrawRectangle(_textBoxOutlinePen, boxX, boxY, boxWidth, boxHeight);

            float inputTextX = (_panelRectangle.Width - inputTextRectangle.Width) / 2 - inputTextRectangle.X;
            float inputTextY = boxY + boxHeight - inputTextRectangle.Bottom - 5;

            var inputTextPosition = new PointF(inputTextX, inputTextY);

            DrawString(graphics, _inputText, _inputTextFont, _textNormalBrush, inputTextPosition);

            // Draw the cursor on top of the text and redraw the text with a different color if necessary.

            Brush cursorTextBrush;
            Brush cursorBrush;
            Pen   cursorPen;

            float cursorPositionYBottom = inputTextY + inputTextRectangle.Bottom;
            float cursorPositionYTop    = cursorPositionYBottom - _inputTextCalibrationHeight - 2;
            float cursorPositionXLeft;
            float cursorPositionXRight;

            bool cursorVisible = false;

            if (_cursorStart != _cursorEnd)
            {
                cursorTextBrush = _textSelectedBrush;
                cursorBrush     = _selectionBoxBrush;
                cursorPen       = _selectionBoxPen;

                string textUntilBegin = _inputText.Substring(0, _cursorStart);
                string textUntilEnd   = _inputText.Substring(0, _cursorEnd);

                RectangleF selectionBeginRectangle = MeasureString(graphics, textUntilBegin, _inputTextFont);
                RectangleF selectionEndRectangle   = MeasureString(graphics, textUntilEnd  , _inputTextFont);

                cursorVisible         = true;
                cursorPositionXLeft   = inputTextX + selectionBeginRectangle.Width + selectionBeginRectangle.X;
                cursorPositionXRight  = inputTextX + selectionEndRectangle.Width   + selectionEndRectangle.X;
            }
            else
            {
                cursorTextBrush = _textOverCursorBrush;
                cursorBrush     = _cursorBrush;
                cursorPen       = _cursorPen;

                if (Volatile.Read(ref _textBoxBlinkCounter.Value) < TextBoxBlinkThreshold)
                {
                    // Show the blinking cursor.

                    int        cursorStart         = Math.Min(_inputText.Length, _cursorStart);
                    string     textUntilCursor     = _inputText.Substring(0, cursorStart);
                    RectangleF cursorTextRectangle = MeasureString(graphics, textUntilCursor, _inputTextFont);

                    cursorVisible       = true;
                    cursorPositionXLeft = inputTextX + cursorTextRectangle.Width + cursorTextRectangle.X;

                    if (_overwriteMode)
                    {
                        // The blinking cursor is in overwrite mode so it takes the size of a character.

                        if (_cursorStart < _inputText.Length)
                        {
                            textUntilCursor      = _inputText.Substring(0, cursorStart + 1);
                            cursorTextRectangle  = MeasureString(graphics, textUntilCursor, _inputTextFont);
                            cursorPositionXRight = inputTextX + cursorTextRectangle.Width + cursorTextRectangle.X;
                        }
                        else
                        {
                            cursorPositionXRight = cursorPositionXLeft + _inputTextFontSize / 2;
                        }
                    }
                    else
                    {
                        // The blinking cursor is in insert mode so it is only a line.
                        cursorPositionXRight = cursorPositionXLeft;
                    }
                }
                else
                {
                    cursorPositionXLeft  = inputTextX;
                    cursorPositionXRight = inputTextX;
                }
            }

            if (_typingEnabled && cursorVisible)
            {
                float cursorWidth  = cursorPositionXRight  - cursorPositionXLeft;
                float cursorHeight = cursorPositionYBottom - cursorPositionYTop;

                if (cursorWidth == 0)
                {
                    graphics.DrawLine(cursorPen, cursorPositionXLeft, cursorPositionYTop, cursorPositionXLeft, cursorPositionYBottom);
                }
                else
                {
                    graphics.DrawRectangle(cursorPen,   cursorPositionXLeft, cursorPositionYTop, cursorWidth, cursorHeight);
                    graphics.FillRectangle(cursorBrush, cursorPositionXLeft, cursorPositionYTop, cursorWidth, cursorHeight);

                    var cursorRectangle = new RectangleF(cursorPositionXLeft, cursorPositionYTop, cursorWidth, cursorHeight);

                    var oldClip   = graphics.Clip;
                    graphics.Clip = new Region(cursorRectangle);

                    DrawString(graphics, _inputText, _inputTextFont, cursorTextBrush, inputTextPosition);

                    graphics.Clip = oldClip;
                }
            }
            else if (!_typingEnabled)
            {
                // Just draw a semi-transparent rectangle on top to fade the component with the background.
                // TODO (caian): This will not work if one decides to add make background semi-transparent as well.
                graphics.FillRectangle(_disabledBrush, boxX - _textBoxOutlineWidth, boxY - _textBoxOutlineWidth,
                    boxWidth + 2* _textBoxOutlineWidth, boxHeight + 2* _textBoxOutlineWidth);
            }
        }

        private void DrawPadButton(System.Drawing.Graphics graphics, PointF point, Image icon, string label, bool pressed, bool enabled)
        {
            // Use relative positions so we can center the the entire drawing later.

            float iconX      = 0;
            float iconY      = 0;
            float iconWidth  = icon.Width;
            float iconHeight = icon.Height;

            var labelRectangle = MeasureString(graphics, label, _labelsTextFont);

            float labelPositionX = iconWidth + 8 - labelRectangle.X;
            float labelPositionY = (iconHeight - labelRectangle.Height) / 2 - labelRectangle.Y - 1;

            float fullWidth  = labelPositionX + labelRectangle.Width + labelRectangle.X;
            float fullHeight = iconHeight;

            // Convert all relative positions into absolute.

            float originX = (int)(point.X - fullWidth  / 2);
            float originY = (int)(point.Y - fullHeight / 2);

            iconX += originX;
            iconY += originY;

            var labelPosition = new PointF(labelPositionX + originX, labelPositionY + originY);

            graphics.DrawImageUnscaled(icon, (int)iconX, (int)iconY);

            DrawString(graphics, label, _labelsTextFont, _textNormalBrush, labelPosition);

            GraphicsPath frame = new GraphicsPath();
            frame.AddRectangle(new RectangleF(originX - 2 * _padPressedPenWidth, originY - 2 * _padPressedPenWidth,
                fullWidth + 4 * _padPressedPenWidth, fullHeight + 4 * _padPressedPenWidth));

            if (enabled)
            {
                if (pressed)
                {
                    graphics.DrawPath(_padPressedPen, frame);
                }
            }
            else
            {
                // Just draw a semi-transparent rectangle on top to fade the component with the background.
                // TODO (caian): This will not work if one decides to add make background semi-transparent as well.
                graphics.FillPath(_disabledBrush, frame);
            }
        }

        private void DrawControllerToggle(System.Drawing.Graphics graphics, PointF point, bool enabled)
        {
            var labelRectangle = MeasureString(graphics, ControllerToggleText, _labelsTextFont);

            // Use relative positions so we can center the the entire drawing later.

            float keyWidth  = _keyModeIcon.Width;
            float keyHeight = _keyModeIcon.Height;

            float labelPositionX = keyWidth + 8 - labelRectangle.X;
            float labelPositionY = -labelRectangle.Y - 1;

            float keyX = 0;
            float keyY = (int)((labelPositionY + labelRectangle.Height - keyHeight) / 2);

            float fullWidth  = labelPositionX + labelRectangle.Width;
            float fullHeight = Math.Max(labelPositionY + labelRectangle.Height, keyHeight);

            // Convert all relative positions into absolute.

            float originX = (int)(point.X - fullWidth  / 2);
            float originY = (int)(point.Y - fullHeight / 2);

            keyX += originX;
            keyY += originY;

            var labelPosition   = new PointF(labelPositionX + originX, labelPositionY + originY);
            var overlayPosition = new Point((int)keyX, (int)keyY);

            graphics.DrawImageUnscaled(_keyModeIcon, overlayPosition);

            DrawString(graphics, ControllerToggleText, _labelsTextFont, _textNormalBrush, labelPosition);
        }

        private bool TryCopyTo(IVirtualMemoryManager destination, ulong position)
        {
            if (_surface == null)
            {
                return false;
            }

            Rectangle lockRectangle = new Rectangle(0, 0, _surface.Width, _surface.Height);
            BitmapData surfaceData  = _surface.LockBits(lockRectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            Debug.Assert(surfaceData.Stride                      == _surfaceInfo.Pitch);
            Debug.Assert(surfaceData.Stride * surfaceData.Height == _surfaceInfo.Size);

            // Convert the pixel format used in System.Drawing to the one required by a Switch Surface.
            int dataLength = surfaceData.Stride * surfaceData.Height;

            byte[] data = new byte[dataLength];
            Span<uint> dataConvert = MemoryMarshal.Cast<byte, uint>(data);

            Marshal.Copy(surfaceData.Scan0, data, 0, dataLength);

            for (int i = 0; i < dataConvert.Length; i++)
            {
                dataConvert[i] = BitOperations.RotateRight(BinaryPrimitives.ReverseEndianness(dataConvert[i]), 8);
            }

            try
            {
                destination.Write(position, data);
            }
            finally
            {
                _surface.UnlockBits(surfaceData);
            }

            return true;
        }

        internal bool DrawTo(RenderingSurfaceInfo surfaceInfo, IVirtualMemoryManager destination, ulong position)
        {
            lock (_renderLock)
            {
                if (!_surfaceInfo.Equals(surfaceInfo))
                {
                    _surfaceInfo = surfaceInfo;
                    RecreateSurface();
                    RecomputeConstants();
                }

                Redraw();

                return TryCopyTo(destination, position);
            }
        }

        public void Dispose()
        {
            _textBoxBlinkTimedAction.RequestCancel();
        }
    }
}
