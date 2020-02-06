using System;
using Ryujinx.Debugger.UI;

namespace Ryujinx.Debugger
{
    public class Debugger : IDisposable
    {
        public DebuggerWidget Widget { get; set; }

        public Debugger()
        {
            Widget = new DebuggerWidget();
        }

        public void Enable()
        {
            Widget.Enable();
        }

        public void Disable()
        {
            Widget.Disable();
        }

        public void Dispose()
        {
            Disable();

            Widget.Dispose();
        }
    }
}
