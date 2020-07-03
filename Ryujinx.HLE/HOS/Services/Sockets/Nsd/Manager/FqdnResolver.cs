using System.Text;

namespace Ryujinx.HLE.HOS.Services.Sockets.Nsd.Manager
{
    class FqdnResolver
    {
        private const string _dummyAddress = "unknown.dummy.nintendo.net";

        private NsdSettings _nsdSettings;

        public FqdnResolver(NsdSettings nsdSettings)
        {
            _nsdSettings = nsdSettings;
        }

        public ResultCode GetSettingName(ServiceCtx context, out string settingName)
        {
            if (_nsdSettings.TestMode)
            {
                settingName = "";

                return ResultCode.NotImplemented;
            }
            else
            {
                settingName = "";

                if (true) // TODO: Determine field (struct + 0x2C)
                {
                    settingName = _nsdSettings.Environment;

                    return ResultCode.Success;
                }

#pragma warning disable CS0162
                return ResultCode.NullOutputObject;
#pragma warning restore CS0162
            }
        }

        public ResultCode GetEnvironmentIdentifier(ServiceCtx context, out string identifier)
        {
            if (_nsdSettings.TestMode)
            {
                identifier = "rre";

                return ResultCode.NotImplemented;
            }
            else
            {
                identifier = _nsdSettings.Environment;
            }

            return ResultCode.Success;
        }

        public ResultCode Resolve(ServiceCtx context, string address, out string resolvedAddress)
        {
            if (address != "api.sect.srv.nintendo.net" || address != "conntest.nintendowifi.net")
            {
                // TODO: Load Environment from the savedata.
                address = address.Replace("%", _nsdSettings.Environment);

                resolvedAddress = "";

                if (_nsdSettings == null)
                {
                    return ResultCode.SettingsNotInitialized;
                }

                if (!_nsdSettings.Initialized)
                {
                    return ResultCode.SettingsNotLoaded;
                }

                switch (address)
                {
                    case "e97b8a9d672e4ce4845ec6947cd66ef6-sb-api.accounts.nintendo.com": // dp1 environment
                        resolvedAddress = "e97b8a9d672e4ce4845ec6947cd66ef6-sb.baas.nintendo.com";
                        break;
                    case "api.accounts.nintendo.com": // dp1 environment
                        resolvedAddress = "e0d67c509fb203858ebcb2fe3f88c2aa.baas.nintendo.com";
                        break;
                    case "e97b8a9d672e4ce4845ec6947cd66ef6-sb.accounts.nintendo.com": // lp1 environment
                        resolvedAddress = "e97b8a9d672e4ce4845ec6947cd66ef6-sb.baas.nintendo.com";
                        break;
                    case "accounts.nintendo.com": // lp1 environment
                        resolvedAddress = "e0d67c509fb203858ebcb2fe3f88c2aa.baas.nintendo.com";
                        break;
                    /*
                    // TODO: Determine fields of the struct.
                    case "": // + 0xEB8 || + 0x2BE8
                        resolvedAddress = ""; // + 0xEB8 + 0x300 || + 0x2BE8 + 0x300
                        break;
                    */
                    default:
                        resolvedAddress = address;
                        break;
                }
            }
            else
            {
                resolvedAddress = address;
            }

            return ResultCode.Success;
        }

        public ResultCode ResolveEx(ServiceCtx context, out ResultCode resultCode, out string resolvedAddress)
        {
            (long inputPosition, long inputSize)  = context.Request.GetBufferType0x21();

            byte[] addressBuffer = new byte[inputSize];

            context.Memory.Read((ulong)inputPosition, addressBuffer);

            string address = Encoding.UTF8.GetString(addressBuffer);

            resultCode = Resolve(context, address, out resolvedAddress);

            if (resultCode != ResultCode.Success)
            {
                resolvedAddress = _dummyAddress;
            }

            if (_nsdSettings.TestMode)
            {
                return ResultCode.Success;
            }
            else
            {
                return resultCode;
            }
        }
    }
}