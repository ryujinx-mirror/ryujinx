using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr
    {
        public ConcurrentQueue<MessageInfo> Messages { get; }

        public FocusState FocusState { get; private set; }

        public KEvent MessageEvent { get; }

        public IdDictionary AppletResourceUserIds { get; }

        public AppletStateMgr(Horizon system)
        {
            Messages     = new ConcurrentQueue<MessageInfo>();
            MessageEvent = new KEvent(system.KernelContext);

            AppletResourceUserIds = new IdDictionary();
        }

        public void SetFocus(bool isFocused)
        {
            FocusState = isFocused ? FocusState.InFocus : FocusState.OutOfFocus;

            Messages.Enqueue(MessageInfo.FocusStateChanged);
            MessageEvent.ReadableEvent.Signal();
        }
    }
}