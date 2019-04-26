using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Profiler.UI
{
    public partial class ProfileWindow
    {
        private void DrawBars(float xOffset, float yOffset, float width)
        {
            if (_sortedProfileData.Count != 0)
            {
                long maxAverage;
                long maxTotal;

                int verticalIndex = 0;
                float barHeight   = (LineHeight - LinePadding) / 3.0f;

                // Get max values
                var maxInstant = maxAverage = maxTotal = 0;
                foreach (KeyValuePair<ProfileConfig, TimingInfo> kvp in _sortedProfileData)
                {
                    maxInstant = Math.Max(maxInstant, kvp.Value.Instant);
                    maxAverage = Math.Max(maxAverage, kvp.Value.AverageTime);
                    maxTotal   = Math.Max(maxTotal,   kvp.Value.TotalTime);
                }

                GL.Enable(EnableCap.ScissorTest);
                GL.Begin(PrimitiveType.Triangles);
                foreach (var entry in _sortedProfileData)
                {
                    // Instant
                    GL.Color3(Color.Blue);
                    float bottom = GetLineY(yOffset, LineHeight, LinePadding, true, verticalIndex++);
                    float top    = bottom + barHeight;
                    float right  = (float)entry.Value.Instant / maxInstant * width + xOffset;

                    // Skip rendering out of bounds bars
                    if (top < 0 || bottom > Height)
                        continue;

                    GL.Vertex2(xOffset, bottom);
                    GL.Vertex2(xOffset, top);
                    GL.Vertex2(right,   top);

                    GL.Vertex2(right,   top);
                    GL.Vertex2(right,   bottom);
                    GL.Vertex2(xOffset, bottom);

                    // Average
                    GL.Color3(Color.Green);
                    top    += barHeight;
                    bottom += barHeight;
                    right   = (float)entry.Value.AverageTime / maxAverage * width + xOffset;

                    GL.Vertex2(xOffset, bottom);
                    GL.Vertex2(xOffset, top);
                    GL.Vertex2(right, top);

                    GL.Vertex2(right, top);
                    GL.Vertex2(right, bottom);
                    GL.Vertex2(xOffset, bottom);

                    // Total
                    GL.Color3(Color.Red);
                    top    += barHeight;
                    bottom += barHeight;
                    right   = (float)entry.Value.TotalTime / maxTotal * width + xOffset;

                    GL.Vertex2(xOffset, bottom);
                    GL.Vertex2(xOffset, top);
                    GL.Vertex2(right, top);

                    GL.Vertex2(right, top);
                    GL.Vertex2(right, bottom);
                    GL.Vertex2(xOffset, bottom);
                }

                GL.End();
                GL.Disable(EnableCap.ScissorTest);
            }
        }
    }
}
