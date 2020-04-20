using Gtk;
using System;
using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Debugger.UI
{
    public class DebuggerWidget : Box
    {
        public event EventHandler DebuggerEnabled;
        public event EventHandler DebuggerDisabled;

#pragma warning disable CS0649
        [GUI] Notebook _widgetNotebook;
#pragma warning restore CS0649

        public DebuggerWidget() : this(new Builder("Ryujinx.Debugger.UI.DebuggerWidget.glade")) { }

        public DebuggerWidget(Builder builder) : base(builder.GetObject("_debuggerBox").Handle)
        {
            builder.Autoconnect(this);

            LoadProfiler();
        }

        public void LoadProfiler()
        {
            ProfilerWidget widget = new ProfilerWidget();

            widget.RegisterParentDebugger(this);

            _widgetNotebook.AppendPage(widget, new Label("Profiler"));
        }

        public void Enable()
        {
            DebuggerEnabled.Invoke(this, null);
        }

        public void Disable()
        {
            DebuggerDisabled.Invoke(this, null);
        }
    }
}
