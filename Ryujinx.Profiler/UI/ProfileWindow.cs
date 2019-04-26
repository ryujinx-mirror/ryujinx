using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Ryujinx.Common;
using Ryujinx.Profiler.UI.SharpFontHelpers;

namespace Ryujinx.Profiler.UI
{
    public partial class ProfileWindow : GameWindow
    {
        // List all buttons for index in button array
        private enum ButtonIndex
        {
            TagTitle          = 0,
            InstantTitle      = 1,
            AverageTitle      = 2,
            TotalTitle        = 3,
            FilterBar         = 4,
            ShowHideInactive  = 5,
            Pause             = 6,
            ChangeDisplay     = 7,

            // Don't automatically draw after here
            ToggleFlags       = 8,
            Step              = 9,

            // Update this when new buttons are added.
            // These are indexes to the enum list
            Autodraw = 8,
            Count    = 10,
        }

        // Font service
        private FontService _fontService;

        // UI variables
        private ProfileButton[] _buttons;

        private bool _initComplete    = false;
        private bool _visible         = true;
        private bool _visibleChanged  = true;
        private bool _viewportUpdated = true;
        private bool _redrawPending   = true;
        private bool _displayGraph    = true;
        private bool _displayFlags    = true;
        private bool _showInactive    = true;
        private bool _paused          = false;
        private bool _doStep          = false;

        // Layout
        private const int LineHeight      = 16;
        private const int TitleHeight     = 24;
        private const int TitleFontHeight = 16;
        private const int LinePadding     = 2;
        private const int ColumnSpacing   = 15;
        private const int FilterHeight    = 24;
        private const int BottomBarHeight = FilterHeight + LineHeight;

        // Sorting
        private List<KeyValuePair<ProfileConfig, TimingInfo>> _unsortedProfileData;
        private IComparer<KeyValuePair<ProfileConfig, TimingInfo>> _sortAction = new ProfileSorters.TagAscending();

        // Flag data
        private long[] _timingFlagsAverages;
        private long[] _timingFlagsLast;

        // Filtering
        private string _filterText = "";
        private bool _regexEnabled = false;

        // Scrolling
        private float _scrollPos = 0;
        private float _minScroll = 0;
        private float _maxScroll = 0;

        // Profile data storage
        private List<KeyValuePair<ProfileConfig, TimingInfo>> _sortedProfileData;
        private long _captureTime;

        // Input
        private bool _backspaceDown       = false;
        private bool _prevBackspaceDown   = false;
        private double _backspaceDownTime = 0;

        // F35 used as no key
        private Key _graphControlKey = Key.F35;

        // Event management
        private double _updateTimer;
        private double _processEventTimer;
        private bool   _profileUpdated           = false;
        private readonly object _profileDataLock = new object();

        public ProfileWindow()
                               // Graphigs mode enables 2xAA
            : base(1280, 720, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 1, 1, 2))
        {
            Title    = "Profiler";
            Location = new Point(DisplayDevice.Default.Width  - 1280,
                                (DisplayDevice.Default.Height - 720) - 50);

            if (Profile.UpdateRate <= 0)
            {
                // Perform step regardless of flag type
                Profile.RegisterFlagReciever((t) =>
                {
                    if (!_paused)
                    {
                        _doStep = true;
                    }
                });
            }

            // Large number to force an update on first update
            _updateTimer = 0xFFFF;

            Init();

            // Release context for render thread
            Context.MakeCurrent(null);
        }
        
        public void ToggleVisible()
        {
            _visible = !_visible;
            _visibleChanged = true;
        }

        private void SetSort(IComparer<KeyValuePair<ProfileConfig, TimingInfo>> filter)
        {
            _sortAction = filter;
            _profileUpdated = true;
        }

#region OnLoad
        /// <summary>
        /// Setup OpenGL and load resources
        /// </summary>
        public void Init()
        {
            GL.ClearColor(Color.Black);
            _fontService = new FontService();
            _fontService.InitalizeTextures();
            _fontService.UpdateScreenHeight(Height);

            _buttons = new ProfileButton[(int)ButtonIndex.Count];
            _buttons[(int)ButtonIndex.TagTitle]      = new ProfileButton(_fontService, () => SetSort(new ProfileSorters.TagAscending()));
            _buttons[(int)ButtonIndex.InstantTitle]  = new ProfileButton(_fontService, () => SetSort(new ProfileSorters.InstantAscending()));
            _buttons[(int)ButtonIndex.AverageTitle]  = new ProfileButton(_fontService, () => SetSort(new ProfileSorters.AverageAscending()));
            _buttons[(int)ButtonIndex.TotalTitle]    = new ProfileButton(_fontService, () => SetSort(new ProfileSorters.TotalAscending()));
            _buttons[(int)ButtonIndex.Step]          = new ProfileButton(_fontService, () => _doStep = true);
            _buttons[(int)ButtonIndex.FilterBar]     = new ProfileButton(_fontService, () =>
            {
                _profileUpdated = true;
                _regexEnabled = !_regexEnabled;
            });

            _buttons[(int)ButtonIndex.ShowHideInactive] = new ProfileButton(_fontService, () =>
            {
                _profileUpdated = true;
                _showInactive = !_showInactive;
            });

            _buttons[(int)ButtonIndex.Pause] = new ProfileButton(_fontService, () =>
            {
                _profileUpdated = true;
                _paused = !_paused;
            });

            _buttons[(int)ButtonIndex.ToggleFlags] = new ProfileButton(_fontService, () =>
            {
                _displayFlags = !_displayFlags;
                _redrawPending = true;
            });

            _buttons[(int)ButtonIndex.ChangeDisplay] = new ProfileButton(_fontService, () =>
            {
                _displayGraph = !_displayGraph;
                _redrawPending = true;
            });

            Visible = _visible;
        }
#endregion

#region OnResize
        /// <summary>
        /// Respond to resize events
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize(EventArgs e)
        {
            _viewportUpdated = true;
        }
#endregion

#region OnClose
        /// <summary>
        /// Intercept close event and hide instead
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            // Hide window
            _visible        = false;
            _visibleChanged = true;

            // Cancel close
            e.Cancel = true;

            base.OnClosing(e);
        }
#endregion

#region OnUpdateFrame
        /// <summary>
        /// Profile Update Loop
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        public void Update(FrameEventArgs e)
        {
            if (_visibleChanged)
            {
                Visible = _visible;
                _visibleChanged = false;
            }

            // Backspace handling
            if (_backspaceDown)
            {
                if (!_prevBackspaceDown)
                {
                    _backspaceDownTime = 0;
                    FilterBackspace();
                }
                else
                {
                    _backspaceDownTime += e.Time;
                    if (_backspaceDownTime > 0.3)
                    {
                        _backspaceDownTime -= 0.05;
                        FilterBackspace();
                    }
                }
            }
            _prevBackspaceDown = _backspaceDown;

            // Get timing data if enough time has passed
            _updateTimer += e.Time;
            if (_doStep || ((Profile.UpdateRate > 0) && (!_paused && (_updateTimer > Profile.UpdateRate))))
            {
                _updateTimer    = 0;
                _captureTime    = PerformanceCounter.ElapsedTicks;
                _timingFlags    = Profile.GetTimingFlags();
                _doStep         = false;
                _profileUpdated = true;

                _unsortedProfileData                     = Profile.GetProfilingData();
                (_timingFlagsAverages, _timingFlagsLast) = Profile.GetTimingAveragesAndLast();
                
            }
            
            // Filtering
            if (_profileUpdated)
            {
                lock (_profileDataLock)
                {
                    _sortedProfileData = _showInactive ? _unsortedProfileData : _unsortedProfileData.FindAll(kvp => kvp.Value.IsActive);

                    if (_sortAction != null)
                    {
                        _sortedProfileData.Sort(_sortAction);
                    }

                    if (_regexEnabled)
                    {
                        try
                        {
                            Regex filterRegex = new Regex(_filterText, RegexOptions.IgnoreCase);
                            if (_filterText != "")
                            {
                                _sortedProfileData = _sortedProfileData.Where((pair => filterRegex.IsMatch(pair.Key.Search))).ToList();
                            }
                        }
                        catch (ArgumentException argException)
                        {
                            // Skip filtering for invalid regex
                        }
                    }
                    else
                    {
                        // Regular filtering
                        _sortedProfileData = _sortedProfileData.Where((pair => pair.Key.Search.ToLower().Contains(_filterText.ToLower()))).ToList();
                    }
                }

                _profileUpdated = false;
                _redrawPending  = true;
                _initComplete   = true;
            }

            // Check for events 20 times a second
            _processEventTimer += e.Time;
            if (_processEventTimer > 0.05)
            {
                ProcessEvents();

                if (_graphControlKey != Key.F35)
                {
                    switch (_graphControlKey)
                    {
                        case Key.Left:
                            _graphPosition += (long) (GraphMoveSpeed * e.Time);
                            break;

                        case Key.Right:
                            _graphPosition = Math.Max(_graphPosition - (long) (GraphMoveSpeed * e.Time), 0);
                            break;

                        case Key.Up:
                            _graphZoom = MathF.Min(_graphZoom + (float) (GraphZoomSpeed * e.Time), 100.0f);
                            break;

                        case Key.Down:
                            _graphZoom = MathF.Max(_graphZoom - (float) (GraphZoomSpeed * e.Time), 1f);
                            break;
                    }

                    _redrawPending = true;
                }

                _processEventTimer = 0;
            }
        }
#endregion

#region OnRenderFrame
        /// <summary>
        /// Profile Render Loop
        /// </summary>
        /// <remarks>There is no need to call the base implementation.</remarks>
        public void Draw()
        {
            if (!_visible || !_initComplete)
            {
                return;
            }
            
            // Update viewport
            if (_viewportUpdated)
            {
                GL.Viewport(0, 0, Width, Height);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, Width, 0, Height, 0.0, 4.0);

                _fontService.UpdateScreenHeight(Height);

                _viewportUpdated = false;
                _redrawPending   = true;
            }

            if (!_redrawPending)
            {
                return;
            }

            // Frame setup
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);

            _fontService.fontColor = Color.White;
            int verticalIndex   = 0;

            float width;
            float maxWidth = 0;
            float yOffset  = _scrollPos - TitleHeight;
            float xOffset  = 10;
            float timingDataLeft;
            float timingWidth;

            // Background lines to make reading easier
            #region Background Lines
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(0, BottomBarHeight, Width, Height - TitleHeight - BottomBarHeight);
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(0.2f, 0.2f, 0.2f);
            for (int i = 0; i < _sortedProfileData.Count; i += 2)
            {
                float top    = GetLineY(yOffset, LineHeight, LinePadding, false, i - 1);
                float bottom = GetLineY(yOffset, LineHeight, LinePadding, false, i);

                // Skip rendering out of bounds bars
                if (top < 0 || bottom > Height)
                    continue;

                GL.Vertex2(0, bottom);
                GL.Vertex2(0, top);
                GL.Vertex2(Width, top);

                GL.Vertex2(Width, top);
                GL.Vertex2(Width, bottom);
                GL.Vertex2(0, bottom);
            }
            GL.End();
            _maxScroll = (LineHeight + LinePadding) * (_sortedProfileData.Count - 1);
#endregion
            
            lock (_profileDataLock)
            {
// Display category
#region Category
                verticalIndex = 0;
                foreach (var entry in _sortedProfileData)
                {
                    if (entry.Key.Category == null)
                    {
                        verticalIndex++;
                        continue;
                    }

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex++);
                    width   = _fontService.DrawText(entry.Key.Category, xOffset, y, LineHeight);

                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
                GL.Disable(EnableCap.ScissorTest);

                width = _fontService.DrawText("Category", xOffset, Height - TitleFontHeight, TitleFontHeight);
                if (width > maxWidth)
                    maxWidth = width;

                xOffset += maxWidth + ColumnSpacing;
#endregion

// Display session group
#region Session Group
                maxWidth      = 0;
                verticalIndex = 0;

                GL.Enable(EnableCap.ScissorTest);
                foreach (var entry in _sortedProfileData)
                {
                    if (entry.Key.SessionGroup == null)
                    {
                        verticalIndex++;
                        continue;
                    }

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex++);
                    width   = _fontService.DrawText(entry.Key.SessionGroup, xOffset, y, LineHeight);

                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
                GL.Disable(EnableCap.ScissorTest);

                width = _fontService.DrawText("Group", xOffset, Height - TitleFontHeight, TitleFontHeight);
                if (width > maxWidth)
                    maxWidth = width;

                xOffset += maxWidth + ColumnSpacing;
#endregion

// Display session item
#region Session Item
                maxWidth      = 0;
                verticalIndex = 0;
                GL.Enable(EnableCap.ScissorTest);
                foreach (var entry in _sortedProfileData)
                {
                    if (entry.Key.SessionItem == null)
                    {
                        verticalIndex++;
                        continue;
                    }

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex++);
                    width   = _fontService.DrawText(entry.Key.SessionItem, xOffset, y, LineHeight);

                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
                GL.Disable(EnableCap.ScissorTest);

                width = _fontService.DrawText("Item", xOffset, Height - TitleFontHeight, TitleFontHeight);
                if (width > maxWidth)
                    maxWidth = width;

                xOffset += maxWidth + ColumnSpacing;
                _buttons[(int)ButtonIndex.TagTitle].UpdateSize(0, Height - TitleFontHeight, 0, (int)xOffset, TitleFontHeight);
#endregion

                // Timing data
                timingWidth    = Width - xOffset - 370;
                timingDataLeft = xOffset;

                GL.Scissor((int)xOffset, BottomBarHeight, (int)timingWidth, Height - TitleHeight - BottomBarHeight);

                if (_displayGraph)
                {
                    DrawGraph(xOffset, yOffset, timingWidth);
                }
                else
                {
                    DrawBars(xOffset, yOffset, timingWidth);
                }

                GL.Scissor(0, BottomBarHeight, Width, Height - TitleHeight - BottomBarHeight);

                if (!_displayGraph)
                {
                    _fontService.DrawText("Blue: Instant,  Green: Avg,  Red: Total", xOffset, Height - TitleFontHeight, TitleFontHeight);
                }

                xOffset = Width - 360;

// Display timestamps
#region Timestamps
                verticalIndex     = 0;
                long totalInstant = 0;
                long totalAverage = 0;
                long totalTime    = 0;
                long totalCount   = 0;

                GL.Enable(EnableCap.ScissorTest);
                foreach (var entry in _sortedProfileData)
                {
                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex++);

                    _fontService.DrawText($"{GetTimeString(entry.Value.Instant)} ({entry.Value.InstantCount})", xOffset, y, LineHeight);

                    _fontService.DrawText(GetTimeString(entry.Value.AverageTime), 150 + xOffset, y, LineHeight);

                    _fontService.DrawText(GetTimeString(entry.Value.TotalTime), 260 + xOffset, y, LineHeight);

                    totalInstant += entry.Value.Instant;
                    totalAverage += entry.Value.AverageTime;
                    totalTime    += entry.Value.TotalTime;
                    totalCount   += entry.Value.InstantCount;
                }
                GL.Disable(EnableCap.ScissorTest);

                float yHeight = Height - TitleFontHeight;

                _fontService.DrawText("Instant (Count)", xOffset, yHeight, TitleFontHeight);
                _buttons[(int)ButtonIndex.InstantTitle].UpdateSize((int)xOffset, (int)yHeight, 0, 130, TitleFontHeight);

                _fontService.DrawText("Average", 150 + xOffset, yHeight, TitleFontHeight);
                _buttons[(int)ButtonIndex.AverageTitle].UpdateSize((int)(150 + xOffset), (int)yHeight, 0, 130, TitleFontHeight);

                _fontService.DrawText("Total (ms)", 260 + xOffset, yHeight, TitleFontHeight);
                _buttons[(int)ButtonIndex.TotalTitle].UpdateSize((int)(260 + xOffset), (int)yHeight, 0, Width, TitleFontHeight);

                // Totals
                yHeight = FilterHeight + 3;
                int textHeight = LineHeight - 2;

                _fontService.fontColor = new Color(100, 100, 255, 255);
                float tempWidth = _fontService.DrawText($"Host {GetTimeString(_timingFlagsLast[(int)TimingFlagType.SystemFrame])} " +
                                                            $"({GetTimeString(_timingFlagsAverages[(int)TimingFlagType.SystemFrame])})", 5, yHeight, textHeight);

                _fontService.fontColor = Color.Red;
                _fontService.DrawText($"Game {GetTimeString(_timingFlagsLast[(int)TimingFlagType.FrameSwap])} " +
                                          $"({GetTimeString(_timingFlagsAverages[(int)TimingFlagType.FrameSwap])})", 15 + tempWidth, yHeight, textHeight);
                _fontService.fontColor = Color.White;
                

                _fontService.DrawText($"{GetTimeString(totalInstant)} ({totalCount})", xOffset,       yHeight, textHeight);
                _fontService.DrawText(GetTimeString(totalAverage),                     150 + xOffset, yHeight, textHeight);
                _fontService.DrawText(GetTimeString(totalTime),                        260 + xOffset, yHeight, textHeight);
#endregion
            }

#region Bottom bar
            // Show/Hide Inactive
            float widthShowHideButton = _buttons[(int)ButtonIndex.ShowHideInactive].UpdateSize($"{(_showInactive ? "Hide" : "Show")} Inactive", 5, 5, 4, 16);

            // Play/Pause
            float widthPlayPauseButton = _buttons[(int)ButtonIndex.Pause].UpdateSize(_paused ? "Play" : "Pause", 15 + (int)widthShowHideButton, 5, 4, 16) + widthShowHideButton;

            // Step
            float widthStepButton = widthPlayPauseButton;

            if (_paused)
            {
                widthStepButton += _buttons[(int)ButtonIndex.Step].UpdateSize("Step", (int)(25 + widthPlayPauseButton), 5, 4, 16) + 10;
                _buttons[(int)ButtonIndex.Step].Draw();
            }

            // Change display
            float widthChangeDisplay = _buttons[(int)ButtonIndex.ChangeDisplay].UpdateSize($"View: {(_displayGraph ? "Graph" : "Bars")}", 25 + (int)widthStepButton, 5, 4, 16) + widthStepButton;

            width = widthChangeDisplay;

            if (_displayGraph)
            {
                width += _buttons[(int) ButtonIndex.ToggleFlags].UpdateSize($"{(_displayFlags ? "Hide" : "Show")} Flags", 35 + (int)widthChangeDisplay, 5, 4, 16) + 10;
                _buttons[(int)ButtonIndex.ToggleFlags].Draw();
            }

            // Filter bar
            _fontService.DrawText($"{(_regexEnabled ? "Regex " : "Filter")}: {_filterText}", 35 + width, 7, 16);
            _buttons[(int)ButtonIndex.FilterBar].UpdateSize((int)(45 + width), 0, 0, Width, FilterHeight);
#endregion

            // Draw buttons
            for (int i = 0; i < (int)ButtonIndex.Autodraw; i++)
            {
                _buttons[i].Draw();
            }
            
// Dividing lines
#region Dividing lines
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Lines);
            // Top divider
            GL.Vertex2(0, Height -TitleHeight);
            GL.Vertex2(Width, Height - TitleHeight);

            // Bottom divider
            GL.Vertex2(0,     FilterHeight);
            GL.Vertex2(Width, FilterHeight);

            GL.Vertex2(0,     BottomBarHeight);
            GL.Vertex2(Width, BottomBarHeight);

            // Bottom vertical dividers
            GL.Vertex2(widthShowHideButton + 10, 0);
            GL.Vertex2(widthShowHideButton + 10, FilterHeight);

            GL.Vertex2(widthPlayPauseButton + 20, 0);
            GL.Vertex2(widthPlayPauseButton + 20, FilterHeight);

            if (_paused)
            {
                GL.Vertex2(widthStepButton + 20, 0);
                GL.Vertex2(widthStepButton + 20, FilterHeight);
            }

            if (_displayGraph)
            {
                GL.Vertex2(widthChangeDisplay + 30, 0);
                GL.Vertex2(widthChangeDisplay + 30, FilterHeight);
            }

            GL.Vertex2(width + 30, 0);
            GL.Vertex2(width + 30, FilterHeight);

            // Column dividers
            float timingDataTop = Height - TitleHeight;

            GL.Vertex2(timingDataLeft, FilterHeight);
            GL.Vertex2(timingDataLeft, timingDataTop);
            
            GL.Vertex2(timingWidth + timingDataLeft, FilterHeight);
            GL.Vertex2(timingWidth + timingDataLeft, timingDataTop);
            GL.End();
#endregion

            _redrawPending = false;
            SwapBuffers();
        }
#endregion

        private string GetTimeString(long timestamp)
        {
            float time = (float)timestamp / PerformanceCounter.TicksPerMillisecond;
            return (time < 1) ? $"{time * 1000:F3}us" : $"{time:F3}ms";
        }

        private void FilterBackspace()
        {
            if (_filterText.Length <= 1)
            {
                _filterText = "";
            }
            else
            {
                _filterText = _filterText.Remove(_filterText.Length - 1, 1);
            }
        }

        private float GetLineY(float offset, float lineHeight, float padding, bool centre, int line)
        {
            return Height + offset - lineHeight - padding - ((lineHeight + padding) * line) + ((centre) ? padding : 0);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            _filterText += e.KeyChar;
            _profileUpdated = true;
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.BackSpace:
                    _profileUpdated = _backspaceDown = true;
                    return;

                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    _graphControlKey = e.Key;
                    return;
        }
            base.OnKeyUp(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            // Can't go into switch as value isn't constant
            if (e.Key == Profile.Controls.Buttons.ToggleProfiler)
            {
                ToggleVisible();
                return;
            }

            switch (e.Key)
            {
                case Key.BackSpace:
                    _backspaceDown = false;
                    return;

                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    _graphControlKey = Key.F35;
                    return;
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            foreach (ProfileButton button in _buttons)
            {
                if (button.ProcessClick(e.X, Height - e.Y))
                    return;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _scrollPos += e.Delta * -30;
            if (_scrollPos < _minScroll)
                _scrollPos = _minScroll;
            if (_scrollPos > _maxScroll)
                _scrollPos = _maxScroll;

            _redrawPending = true;
        }
    }
}