using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Common;

namespace Ryujinx.Profiler.UI
{
    public partial class ProfileWindow
    {
        // Colour index equal to timing flag type as int
        private Color[] _timingFlagColours = new[]
        {
            new Color(150, 25, 25, 50), // FrameSwap   = 0
            new Color(25, 25, 150, 50), // SystemFrame = 1
        };

        private TimingFlag[] _timingFlags;

        private const float GraphMoveSpeed = 40000;
        private const float GraphZoomSpeed = 50;

        private float _graphZoom      = 1;
        private float _graphPosition  = 0;

        private void DrawGraph(float xOffset, float yOffset, float width)
        {
            if (_sortedProfileData.Count != 0)
            {
                int   left, right;
                float top, bottom;

                int    verticalIndex      = 0;
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

                GL.Enable(EnableCap.ScissorTest);

                // Draw timing flags
                if (_displayFlags)
                {
                    TimingFlagType prevType = TimingFlagType.Count;

                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    GL.Begin(PrimitiveType.Lines);
                    foreach (TimingFlag timingFlag in _timingFlags)
                    {
                        if (prevType != timingFlag.FlagType)
                        {
                            prevType = timingFlag.FlagType;
                            GL.Color4(_timingFlagColours[(int)prevType]);
                        }

                        int x = (int)(graphRight - ((graphPositionTicks - timingFlag.Timestamp) / timeWidthTicks) * width);
                        GL.Vertex2(x, 0);
                        GL.Vertex2(x, Height);
                    }
                    GL.End();
                    GL.Disable(EnableCap.Blend);
                }

                // Draw bars
                GL.Begin(PrimitiveType.Triangles);
                foreach (var entry in _sortedProfileData)
                {
                    long furthest = 0;

                    bottom = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex);
                    top    = bottom + barHeight;

                    // Skip rendering out of bounds bars
                    if (top < 0 || bottom > Height)
                    {
                        verticalIndex++;
                        continue;
                    }


                    GL.Color3(Color.Green);
                    foreach (Timestamp timestamp in entry.Value.GetAllTimestamps())
                    {
                        // Skip drawing multiple timestamps on same pixel
                        if (timestamp.EndTime < furthest)
                            continue;
                        furthest = timestamp.EndTime + ticksPerPixel;

                        left  = (int)(graphRight - ((graphPositionTicks - timestamp.BeginTime) / timeWidthTicks) * width);
                        right = (int)(graphRight - ((graphPositionTicks - timestamp.EndTime)   / timeWidthTicks) * width);

                        // Make sure width is at least 1px
                        right = Math.Max(left + 1, right);

                        GL.Vertex2(left,  bottom);
                        GL.Vertex2(left,  top);
                        GL.Vertex2(right, top);

                        GL.Vertex2(right, top);
                        GL.Vertex2(right, bottom);
                        GL.Vertex2(left,  bottom);
                    }

                    // Currently capturing timestamp
                    GL.Color3(Color.Red);
                    long entryBegin = entry.Value.BeginTime;
                    if (entryBegin != -1)
                    {
                        left = (int)(graphRight - ((graphPositionTicks - entryBegin) / timeWidthTicks) * width);

                        // Make sure width is at least 1px
                        left = Math.Min(left - 1, (int)graphRight);

                        GL.Vertex2(left,       bottom);
                        GL.Vertex2(left,       top);
                        GL.Vertex2(graphRight, top);

                        GL.Vertex2(graphRight, top);
                        GL.Vertex2(graphRight, bottom);
                        GL.Vertex2(left,       bottom);
                    }

                    verticalIndex++;
                }

                GL.End();
                GL.Disable(EnableCap.ScissorTest);

                string label = $"-{MathF.Round(_graphPosition, 2)} ms";

                // Dummy draw for measure
                float labelWidth = _fontService.DrawText(label, 0, 0, LineHeight, false);
                _fontService.DrawText(label, graphRight - labelWidth - LinePadding, FilterHeight + LinePadding, LineHeight);
                
                _fontService.DrawText($"-{MathF.Round((float)((timeWidthTicks / PerformanceCounter.TicksPerMillisecond) + _graphPosition), 2)} ms", xOffset + LinePadding, FilterHeight + LinePadding, LineHeight);
            }
        }
    }
}
