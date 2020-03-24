using Gtk;
using Ryujinx.Common;
using Ryujinx.Debugger.Profiler;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Debugger.UI
{
    public class ProfilerWidget : Box
    {
        private Thread _profilerThread;
        private double _prevTime;
        private bool   _profilerRunning;
        
        private TimingFlag[] _timingFlags;

        private bool _initComplete  = false;
        private bool _redrawPending = true;
        private bool _doStep        = false;

        // Layout
        private const int LineHeight         = 16;
        private const int MinimumColumnWidth = 200;
        private const int TitleHeight        = 24;
        private const int TitleFontHeight    = 16;
        private const int LinePadding        = 2;
        private const int ColumnSpacing      = 15;
        private const int FilterHeight       = 24;
        private const int BottomBarHeight    = FilterHeight + LineHeight;

        // Sorting
        private List<KeyValuePair<ProfileConfig, TimingInfo>>      _unsortedProfileData;
        private IComparer<KeyValuePair<ProfileConfig, TimingInfo>> _sortAction = new ProfileSorters.TagAscending();

        // Flag data
        private long[] _timingFlagsAverages;
        private long[] _timingFlagsLast;

        // Filtering
        private string _filterText   = "";
        private bool   _regexEnabled = false;

        // Scrolling
        private float _scrollPos = 0;

        // Profile data storage
        private List<KeyValuePair<ProfileConfig, TimingInfo>> _sortedProfileData;
        private long _captureTime;

        // Graph
        private SKColor[] _timingFlagColors = new[]
        {
            new SKColor(150, 25, 25, 50), // FrameSwap   = 0
            new SKColor(25, 25, 150, 50), // SystemFrame = 1
        };

        private const float GraphMoveSpeed = 40000;
        private const float GraphZoomSpeed = 50;

        private float _graphZoom = 1;
        private float _graphPosition = 0;
        private int _rendererHeight => _renderer.AllocatedHeight;
        private int _rendererWidth  => _renderer.AllocatedWidth;

        // Event management
        private long            _lastOutputUpdate;
        private long            _lastOutputDraw;
        private long            _lastOutputUpdateDuration;
        private long            _lastOutputDrawDuration;
        private double          _lastFrameTimeMs;
        private double          _updateTimer;
        private bool            _profileUpdated  = false;
        private readonly object _profileDataLock = new object();

        private SkRenderer _renderer;

        [GUI] ScrolledWindow _scrollview;
        [GUI] CheckButton    _enableCheckbutton;
        [GUI] Scrollbar      _outputScrollbar;
        [GUI] Entry          _filterBox;
        [GUI] ComboBox       _modeBox;
        [GUI] CheckButton    _showFlags;
        [GUI] CheckButton    _showInactive;
        [GUI] Button         _stepButton;
        [GUI] CheckButton    _pauseCheckbutton;

        public ProfilerWidget() : this(new Builder("Ryujinx.Debugger.UI.ProfilerWidget.glade")) { }

        public ProfilerWidget(Builder builder) : base(builder.GetObject("_profilerBox").Handle)
        {
            builder.Autoconnect(this);

            this.KeyPressEvent += ProfilerWidget_KeyPressEvent;

            this.Expand = true;

            _renderer = new SkRenderer();
            _renderer.Expand = true;

            _outputScrollbar.ValueChanged += _outputScrollbar_ValueChanged;

            _renderer.DrawGraphs += _renderer_DrawGraphs;

            _filterBox.Changed += _filterBox_Changed;

            _stepButton.Clicked += _stepButton_Clicked;

            _scrollview.Add(_renderer);

            if (Profile.UpdateRate <= 0)
            {
                // Perform step regardless of flag type
                Profile.RegisterFlagReceiver((t) =>
                {
                    if (_pauseCheckbutton.Active)
                    {
                        _doStep = true;
                    }
                });
            }
        }

        private void _stepButton_Clicked(object sender, EventArgs e)
        {
            if (_pauseCheckbutton.Active)
            {
                _doStep = true;
            }

            _profileUpdated = true;
        }

        private void _filterBox_Changed(object sender, EventArgs e)
        {
            _filterText     = _filterBox.Text;
            _profileUpdated = true;
        }

        private void _outputScrollbar_ValueChanged(object sender, EventArgs e)
        {
            _scrollPos      = -(float)Math.Max(0, _outputScrollbar.Value);
            _profileUpdated = true;
        }

        private void _renderer_DrawGraphs(object sender, EventArgs e)
        {
            if (e is SKPaintSurfaceEventArgs se)
            {
                Draw(se.Surface.Canvas);
            }
        }

        public void RegisterParentDebugger(DebuggerWidget debugger)
        {
            debugger.DebuggerEnabled  += Debugger_DebuggerAttached;
            debugger.DebuggerDisabled += Debugger_DebuggerDettached;
        }

        private void Debugger_DebuggerDettached(object sender, EventArgs e)
        {
            _profilerRunning = false;

            if (_profilerThread != null)
            {
                _profilerThread.Join();
            }
        }

        private void Debugger_DebuggerAttached(object sender, EventArgs e)
        {
            _profilerRunning = false;

            if (_profilerThread != null)
            {
                _profilerThread.Join();
            }

            _profilerRunning = true;

            _profilerThread = new Thread(UpdateLoop)
            {
                Name = "Profiler.UpdateThread"
            };
            _profilerThread.Start();
        }

        private void ProfilerWidget_KeyPressEvent(object o, Gtk.KeyPressEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.Left:
                    _graphPosition += (long)(GraphMoveSpeed * _lastFrameTimeMs);
                    break;

                case Gdk.Key.Right:
                    _graphPosition = Math.Max(_graphPosition - (long)(GraphMoveSpeed * _lastFrameTimeMs), 0);
                    break;

                case Gdk.Key.Up:
                    _graphZoom = MathF.Min(_graphZoom + (float)(GraphZoomSpeed * _lastFrameTimeMs), 100.0f);
                    break;

                case Gdk.Key.Down:
                    _graphZoom = MathF.Max(_graphZoom - (float)(GraphZoomSpeed * _lastFrameTimeMs), 1f);
                    break;
            }
            _profileUpdated = true;
        }

        public void UpdateLoop()
        {
            _lastOutputUpdate = PerformanceCounter.ElapsedTicks;
            _lastOutputDraw   = PerformanceCounter.ElapsedTicks;

            while (_profilerRunning)
            {
                _lastOutputUpdate = PerformanceCounter.ElapsedTicks;
                int timeToSleepMs = (_pauseCheckbutton.Active || !_enableCheckbutton.Active) ? 33 : 1;

                if (Profile.ProfilingEnabled() && _enableCheckbutton.Active)
                {
                    double time = (double)PerformanceCounter.ElapsedTicks / PerformanceCounter.TicksPerSecond;

                    Update(time - _prevTime);

                    _lastOutputUpdateDuration = PerformanceCounter.ElapsedTicks - _lastOutputUpdate;
                    _prevTime = time;

                    Gdk.Threads.AddIdle(1000, ()=> 
                    {
                        _renderer.QueueDraw();

                        return true;
                    });
                }

                Thread.Sleep(timeToSleepMs);
            }
        }

        public void Update(double frameTime)
        {
            _lastFrameTimeMs = frameTime;

            // Get timing data if enough time has passed
            _updateTimer += frameTime;

            if (_doStep || ((Profile.UpdateRate > 0) && (!_pauseCheckbutton.Active && (_updateTimer > Profile.UpdateRate))))
            {
                _updateTimer    = 0;
                _captureTime    = PerformanceCounter.ElapsedTicks;
                _timingFlags    = Profile.GetTimingFlags();
                _doStep         = false;
                _profileUpdated = true;

                _unsortedProfileData = Profile.GetProfilingData();

                (_timingFlagsAverages, _timingFlagsLast) = Profile.GetTimingAveragesAndLast();
            }

            // Filtering
            if (_profileUpdated)
            {
                lock (_profileDataLock)
                {
                    _sortedProfileData = _showInactive.Active ? _unsortedProfileData : _unsortedProfileData.FindAll(kvp => kvp.Value.IsActive);

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
        }

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
            return offset + lineHeight + padding + ((lineHeight + padding) * line) - ((centre) ? padding : 0);
        }

        public void Draw(SKCanvas canvas)
        {
            _lastOutputDraw = PerformanceCounter.ElapsedTicks;
            if (!Visible                   ||
                !_initComplete             ||
                !_enableCheckbutton.Active ||
                !_redrawPending)
            {
                return;
            }

            float viewTop    = TitleHeight + 5;
            float viewBottom = _rendererHeight - FilterHeight - LineHeight;

            float columnWidth;
            float maxColumnWidth = MinimumColumnWidth;
            float yOffset        = _scrollPos + viewTop;
            float xOffset        = 10;
            float timingWidth;

            float contentHeight = GetLineY(0, LineHeight, LinePadding, false, _sortedProfileData.Count - 1);

            _outputScrollbar.Adjustment.Upper    = contentHeight;
            _outputScrollbar.Adjustment.Lower    = 0;
            _outputScrollbar.Adjustment.PageSize = viewBottom - viewTop;


            SKPaint textFont = new SKPaint()
            {
                Color    = SKColors.White,
                TextSize = LineHeight
            };

            SKPaint titleFont = new SKPaint()
            {
                Color    = SKColors.White,
                TextSize = TitleFontHeight
            };

            SKPaint evenItemBackground = new SKPaint()
            {
                Color = SKColors.Gray
            };

            canvas.Save();
            canvas.ClipRect(new SKRect(0, viewTop, _rendererWidth, viewBottom), SKClipOperation.Intersect);

            for (int i = 1; i < _sortedProfileData.Count; i += 2)
            {
                float top    = GetLineY(yOffset, LineHeight, LinePadding, false, i - 1);
                float bottom = GetLineY(yOffset, LineHeight, LinePadding, false, i);

                canvas.DrawRect(new SKRect(0, top, _rendererWidth, bottom), evenItemBackground);
            }

            lock (_profileDataLock)
            {
                // Display category

                for (int verticalIndex = 0; verticalIndex < _sortedProfileData.Count; verticalIndex++)
                {
                    KeyValuePair<ProfileConfig, TimingInfo> entry = _sortedProfileData[verticalIndex];

                    if (entry.Key.Category == null)
                    {
                        continue;
                    }

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex);

                    canvas.DrawText(entry.Key.Category, new SKPoint(xOffset, y), textFont);

                    columnWidth = textFont.MeasureText(entry.Key.Category);

                    if (columnWidth > maxColumnWidth)
                    {
                        maxColumnWidth = columnWidth;
                    }
                }

                canvas.Restore();
                canvas.DrawText("Category", new SKPoint(xOffset, TitleFontHeight + 2), titleFont);

                columnWidth = titleFont.MeasureText("Category");

                if (columnWidth > maxColumnWidth)
                {
                    maxColumnWidth = columnWidth;
                }

                xOffset += maxColumnWidth + ColumnSpacing;

                canvas.DrawLine(new SKPoint(xOffset - ColumnSpacing / 2, 0), new SKPoint(xOffset - ColumnSpacing / 2, viewBottom), textFont);

                // Display session group
                maxColumnWidth = MinimumColumnWidth;

                canvas.Save();
                canvas.ClipRect(new SKRect(0, viewTop, _rendererWidth, viewBottom), SKClipOperation.Intersect);

                for (int verticalIndex = 0; verticalIndex < _sortedProfileData.Count; verticalIndex++)
                {
                    KeyValuePair<ProfileConfig, TimingInfo> entry = _sortedProfileData[verticalIndex];

                    if (entry.Key.SessionGroup == null)
                    {
                        continue;
                    }

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex);

                    canvas.DrawText(entry.Key.SessionGroup, new SKPoint(xOffset, y), textFont);

                    columnWidth = textFont.MeasureText(entry.Key.SessionGroup);

                    if (columnWidth > maxColumnWidth)
                    {
                        maxColumnWidth = columnWidth;
                    }
                }

                canvas.Restore();
                canvas.DrawText("Group", new SKPoint(xOffset, TitleFontHeight + 2), titleFont);

                columnWidth = titleFont.MeasureText("Group");

                if (columnWidth > maxColumnWidth)
                {
                    maxColumnWidth = columnWidth;
                }

                xOffset += maxColumnWidth + ColumnSpacing;

                canvas.DrawLine(new SKPoint(xOffset - ColumnSpacing / 2, 0), new SKPoint(xOffset - ColumnSpacing / 2, viewBottom), textFont);

                // Display session item
                maxColumnWidth = MinimumColumnWidth;

                canvas.Save();
                canvas.ClipRect(new SKRect(0, viewTop, _rendererWidth, viewBottom), SKClipOperation.Intersect);

                for (int verticalIndex = 0; verticalIndex < _sortedProfileData.Count; verticalIndex++)
                {
                    KeyValuePair<ProfileConfig, TimingInfo> entry = _sortedProfileData[verticalIndex];

                    if (entry.Key.SessionItem == null)
                    {
                        continue;
                    }

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex);

                    canvas.DrawText(entry.Key.SessionItem, new SKPoint(xOffset, y), textFont);

                    columnWidth = textFont.MeasureText(entry.Key.SessionItem);

                    if (columnWidth > maxColumnWidth)
                    {
                        maxColumnWidth = columnWidth;
                    }
                }

                canvas.Restore();
                canvas.DrawText("Item", new SKPoint(xOffset, TitleFontHeight + 2), titleFont);

                columnWidth = titleFont.MeasureText("Item");

                if (columnWidth > maxColumnWidth)
                {
                    maxColumnWidth = columnWidth;
                }

                xOffset += maxColumnWidth + ColumnSpacing;

                timingWidth = _rendererWidth - xOffset - 370;

                canvas.Save();
                canvas.ClipRect(new SKRect(0, viewTop, _rendererWidth, viewBottom), SKClipOperation.Intersect);
                canvas.DrawLine(new SKPoint(xOffset, 0), new SKPoint(xOffset, _rendererHeight), textFont);

                int mode = _modeBox.Active;

                canvas.Save();
                canvas.ClipRect(new SKRect(xOffset, yOffset,xOffset + timingWidth,yOffset + contentHeight), 
                                            SKClipOperation.Intersect);

                switch (mode)
                {
                    case 0: 
                        DrawGraph(xOffset, yOffset, timingWidth, canvas);
                        break;
                    case 1: 
                        DrawBars(xOffset, yOffset, timingWidth, canvas);

                        canvas.DrawText("Blue: Instant,  Green: Avg,  Red: Total", 
                                        new SKPoint(xOffset, _rendererHeight - TitleFontHeight), titleFont);
                        break;
                }

                canvas.Restore();
                canvas.DrawLine(new SKPoint(xOffset + timingWidth, 0), new SKPoint(xOffset + timingWidth, _rendererHeight), textFont);

                xOffset = _rendererWidth - 360;

                // Display timestamps
                long totalInstant = 0;
                long totalAverage = 0;
                long totalTime    = 0;
                long totalCount   = 0;

                for (int verticalIndex = 0; verticalIndex < _sortedProfileData.Count; verticalIndex++)
                {
                    KeyValuePair<ProfileConfig, TimingInfo> entry = _sortedProfileData[verticalIndex];

                    float y = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex);

                    canvas.DrawText($"{GetTimeString(entry.Value.Instant)} ({entry.Value.InstantCount})", new SKPoint(xOffset, y), textFont);
                    canvas.DrawText(GetTimeString(entry.Value.AverageTime), new SKPoint(150 + xOffset, y), textFont);
                    canvas.DrawText(GetTimeString(entry.Value.TotalTime), new SKPoint(260 + xOffset, y), textFont);

                    totalInstant += entry.Value.Instant;
                    totalAverage += entry.Value.AverageTime;
                    totalTime    += entry.Value.TotalTime;
                    totalCount   += entry.Value.InstantCount;
                }

                canvas.Restore();
                canvas.DrawLine(new SKPoint(0, viewTop), new SKPoint(_rendererWidth, viewTop), titleFont);

                float yHeight = 0 + TitleFontHeight;

                canvas.DrawText("Instant (Count)", new SKPoint(xOffset, yHeight), titleFont);
                canvas.DrawText("Average", new SKPoint(150 + xOffset, yHeight), titleFont);
                canvas.DrawText("Total (ms)", new SKPoint(260 + xOffset, yHeight), titleFont);

                // Totals
                yHeight = _rendererHeight - FilterHeight + 3;

                int textHeight = LineHeight - 2;

                SKPaint detailFont = new SKPaint()
                {
                    Color = new SKColor(100, 100, 255, 255),
                    TextSize = textHeight
                };

                canvas.DrawLine(new SkiaSharp.SKPoint(0, viewBottom), new SkiaSharp.SKPoint(_rendererWidth,viewBottom), textFont);

                string hostTimeString = $"Host {GetTimeString(_timingFlagsLast[(int)TimingFlagType.SystemFrame])} " +
                                        $"({GetTimeString(_timingFlagsAverages[(int)TimingFlagType.SystemFrame])})";

                canvas.DrawText(hostTimeString, new SKPoint(5, yHeight), detailFont);

                float tempWidth = detailFont.MeasureText(hostTimeString);

                detailFont.Color = SKColors.Red;

                string gameTimeString = $"Game {GetTimeString(_timingFlagsLast[(int)TimingFlagType.FrameSwap])} " +
                                        $"({GetTimeString(_timingFlagsAverages[(int)TimingFlagType.FrameSwap])})";

                canvas.DrawText(gameTimeString, new SKPoint(15 + tempWidth, yHeight), detailFont);

                tempWidth += detailFont.MeasureText(gameTimeString);

                detailFont.Color = SKColors.White;

                canvas.DrawText($"Profiler: Update {GetTimeString(_lastOutputUpdateDuration)} Draw {GetTimeString(_lastOutputDrawDuration)}", 
                    new SKPoint(20 + tempWidth, yHeight), detailFont);

                detailFont.Color = SKColors.White;

                canvas.DrawText($"{GetTimeString(totalInstant)} ({totalCount})", new SKPoint(xOffset, yHeight), detailFont);
                canvas.DrawText(GetTimeString(totalAverage), new SKPoint(150 + xOffset, yHeight), detailFont);
                canvas.DrawText(GetTimeString(totalTime), new SKPoint(260 + xOffset, yHeight), detailFont);

                _lastOutputDrawDuration = PerformanceCounter.ElapsedTicks - _lastOutputDraw;
            }
        }

        private void DrawGraph(float xOffset, float yOffset, float width, SKCanvas canvas)
        {
            if (_sortedProfileData.Count != 0)
            {
                int   left, right;
                float top,  bottom;

                float  graphRight         = xOffset + width;
                float  barHeight          = (LineHeight - LinePadding);
                long   history            = Profile.HistoryLength;
                double timeWidthTicks     = history / (double)_graphZoom;
                long   graphPositionTicks = (long)(_graphPosition * PerformanceCounter.TicksPerMillisecond);
                long   ticksPerPixel      = (long)(timeWidthTicks / width);

                // Reset start point if out of bounds
                if (timeWidthTicks + graphPositionTicks > history)
                {
                    graphPositionTicks = history - (long)timeWidthTicks;
                    _graphPosition     = (float)graphPositionTicks / PerformanceCounter.TicksPerMillisecond;
                }

                graphPositionTicks = _captureTime - graphPositionTicks;

                // Draw timing flags
                if (_showFlags.Active)
                {
                    TimingFlagType prevType = TimingFlagType.Count;

                    SKPaint timingPaint = new SKPaint
                    {
                        Color = _timingFlagColors.First()
                    };

                    foreach (TimingFlag timingFlag in _timingFlags)
                    {
                        if (prevType != timingFlag.FlagType)
                        {
                            prevType = timingFlag.FlagType;
                            timingPaint.Color = _timingFlagColors[(int)prevType];
                        }

                        int x = (int)(graphRight - ((graphPositionTicks - timingFlag.Timestamp) / timeWidthTicks) * width);

                        if (x > xOffset)
                        {
                            canvas.DrawLine(new SKPoint(x, yOffset), new SKPoint(x, _rendererHeight), timingPaint);
                        }
                    }
                }

                SKPaint barPaint = new SKPaint()
                {
                    Color = SKColors.Green,
                };

                // Draw bars
                for (int verticalIndex = 0; verticalIndex < _sortedProfileData.Count; verticalIndex++)
                {
                    KeyValuePair<ProfileConfig, TimingInfo> entry = _sortedProfileData[verticalIndex];
                    long furthest = 0;

                    bottom = GetLineY(yOffset, LineHeight, LinePadding, false, verticalIndex);
                    top    = bottom + barHeight;

                    // Skip rendering out of bounds bars
                    if (top < 0 || bottom > _rendererHeight)
                    {
                        continue;
                    }

                    barPaint.Color = SKColors.Green;

                    foreach (Timestamp timestamp in entry.Value.GetAllTimestamps())
                    {
                        // Skip drawing multiple timestamps on same pixel
                        if (timestamp.EndTime < furthest)
                        {
                            continue;
                        }

                        furthest = timestamp.EndTime + ticksPerPixel;

                        left  = (int)(graphRight - ((graphPositionTicks - timestamp.BeginTime) / timeWidthTicks) * width);
                        right = (int)(graphRight - ((graphPositionTicks - timestamp.EndTime)   / timeWidthTicks) * width);

                        left = (int)Math.Max(xOffset +1, left);

                        // Make sure width is at least 1px
                        right = Math.Max(left + 1, right);

                        canvas.DrawRect(new SKRect(left, top, right, bottom), barPaint);
                    }

                    // Currently capturing timestamp
                    barPaint.Color = SKColors.Red;

                    long entryBegin = entry.Value.BeginTime;

                    if (entryBegin != -1)
                    {
                        left = (int)(graphRight - ((graphPositionTicks - entryBegin) / timeWidthTicks) * width);

                        // Make sure width is at least 1px
                        left = Math.Min(left - 1, (int)graphRight);

                        left = (int)Math.Max(xOffset + 1, left);

                        canvas.DrawRect(new SKRect(left, top, graphRight, bottom), barPaint);
                    }
                }

                string label = $"-{MathF.Round(_graphPosition, 2)} ms";

                SKPaint labelPaint = new SKPaint()
                {
                    Color    = SKColors.White,
                    TextSize = LineHeight
                };

                float labelWidth = labelPaint.MeasureText(label);

                canvas.DrawText(label,new SKPoint(graphRight - labelWidth - LinePadding, FilterHeight + LinePadding) , labelPaint);

                canvas.DrawText($"-{MathF.Round((float)((timeWidthTicks / PerformanceCounter.TicksPerMillisecond) + _graphPosition), 2)} ms",
                    new SKPoint(xOffset + LinePadding, FilterHeight + LinePadding), labelPaint);
            }
        }

        private void DrawBars(float xOffset, float yOffset, float width, SKCanvas canvas)
        {
            if (_sortedProfileData.Count != 0)
            {
                long maxAverage = 0;
                long maxTotal   = 0;
                long maxInstant = 0;

                float barHeight = (LineHeight - LinePadding) / 3.0f;

                // Get max values
                foreach (KeyValuePair<ProfileConfig, TimingInfo> kvp in _sortedProfileData)
                {
                    maxInstant = Math.Max(maxInstant, kvp.Value.Instant);
                    maxAverage = Math.Max(maxAverage, kvp.Value.AverageTime);
                    maxTotal   = Math.Max(maxTotal, kvp.Value.TotalTime);
                }

                SKPaint barPaint = new SKPaint()
                {
                    Color = SKColors.Blue
                };

                for (int verticalIndex = 0; verticalIndex < _sortedProfileData.Count; verticalIndex++)
                {
                    KeyValuePair<ProfileConfig, TimingInfo> entry = _sortedProfileData[verticalIndex];
                    // Instant
                    barPaint.Color = SKColors.Blue;

                    float bottom = GetLineY(yOffset, LineHeight, LinePadding, false, verticalIndex);
                    float top    = bottom + barHeight;
                    float right  = (float)entry.Value.Instant / maxInstant * width + xOffset;

                    // Skip rendering out of bounds bars
                    if (top < 0 || bottom > _rendererHeight)
                    {
                        continue;
                    }

                    canvas.DrawRect(new SKRect(xOffset, top, right, bottom), barPaint);

                    // Average
                    barPaint.Color = SKColors.Green;

                    top    += barHeight;
                    bottom += barHeight;
                    right   = (float)entry.Value.AverageTime / maxAverage * width + xOffset;

                    canvas.DrawRect(new SKRect(xOffset, top, right, bottom), barPaint);

                    // Total
                    barPaint.Color = SKColors.Red;

                    top    += barHeight;
                    bottom += barHeight;
                    right   = (float)entry.Value.TotalTime / maxTotal * width + xOffset;

                    canvas.DrawRect(new SKRect(xOffset, top, right, bottom), barPaint);
                }
            }
        }
    }
}
