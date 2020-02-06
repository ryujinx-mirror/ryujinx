using SkiaSharp;
using SkiaSharp.Views.Gtk;
using System;

namespace Ryujinx.Debugger.UI
{
    public class SkRenderer : SKDrawingArea
    {
        public event EventHandler DrawGraphs;

        public SkRenderer()
        {
            this.PaintSurface += SkRenderer_PaintSurface;
        }

        private void SkRenderer_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.Black);

            DrawGraphs.Invoke(this, e);
        }
    }
}
