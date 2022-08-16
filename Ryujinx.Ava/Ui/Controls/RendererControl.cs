using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Ryujinx.Common.Configuration;
using SPB.Windowing;
using System;

namespace Ryujinx.Ava.Ui.Controls
{
    internal abstract class RendererControl : Control
    {
        protected object Image { get; set; }

        public event EventHandler<EventArgs> RendererInitialized;
        public event EventHandler<Size> SizeChanged;

        protected Size RenderSize { get; private set; }
        public bool IsStarted { get; private set; }

        public GraphicsDebugLevel DebugLevel { get; }

        private bool _isInitialized;

        protected ICustomDrawOperation DrawOperation { get; private set; }

        public RendererControl(GraphicsDebugLevel graphicsDebugLevel)
        {
            DebugLevel = graphicsDebugLevel;
            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);

            Focusable = true;
        }

        protected void Resized(Rect rect)
        {
            SizeChanged?.Invoke(this, rect.Size);

            if (!rect.IsEmpty)
            {
                RenderSize = rect.Size * VisualRoot.RenderScaling;
                DrawOperation = CreateDrawOperation();
            }
        }

        protected abstract ICustomDrawOperation CreateDrawOperation();
        protected abstract void CreateWindow();

        public override void Render(DrawingContext context)
        {
            if (!_isInitialized)
            {
                CreateWindow();

                OnRendererInitialized();
                _isInitialized = true;
            }

            if (!IsStarted || Image == null)
            {
                return;
            }

            if (DrawOperation != null)
            {
                context.Custom(DrawOperation);
            }

            base.Render(context);
        }

        protected void OnRendererInitialized()
        {
            RendererInitialized?.Invoke(this, EventArgs.Empty);
        }

        internal abstract void Present(object image);

        internal void Start()
        {
            IsStarted = true;
        }

        internal void Stop()
        {
            IsStarted = false;
        }

        public abstract void DestroyBackgroundContext();
        internal abstract void MakeCurrent();
        internal abstract void MakeCurrent(SwappableNativeWindowBase window);
    }
}