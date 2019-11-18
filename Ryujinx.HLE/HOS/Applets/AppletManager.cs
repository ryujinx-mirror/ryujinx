using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Applets
{
    static class AppletManager
    {
        private static Dictionary<AppletId, Type> _appletMapping;

        static AppletManager()
        {
            _appletMapping = new Dictionary<AppletId, Type>
            {
                { AppletId.PlayerSelect,     typeof(PlayerSelectApplet)     },
                { AppletId.SoftwareKeyboard, typeof(SoftwareKeyboardApplet) }
            };
        }

        public static IApplet Create(AppletId applet, Horizon system)
        {
            if (_appletMapping.TryGetValue(applet, out Type appletClass))
            {
                return (IApplet)Activator.CreateInstance(appletClass, system);
            }

            throw new NotImplementedException($"{applet} applet is not implemented.");
        }
    }
}
