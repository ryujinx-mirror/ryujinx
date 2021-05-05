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

        public ResultCode GetEnvironmentIdentifier(out string identifier)
        {
            if (_nsdSettings.TestMode)
            {
                identifier = "err";

                return ResultCode.InvalidSettingsValue;
            }
            else
            {
                identifier = _nsdSettings.Environment;
            }

            return ResultCode.Success;
        }

        public ResultCode Resolve(ServiceCtx context, string address, out string resolvedAddress)
        {
            if (address == "api.sect.srv.nintendo.net"     ||
                address == "ctest.cdn.nintendo.net"        ||
                address == "ctest.cdn.n.nintendoswitch.cn" ||
                address == "unknown.dummy.nintendo.net")
            {
                resolvedAddress = address;
            }
            else
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

                resolvedAddress = address switch
                {
                    "e97b8a9d672e4ce4845ec6947cd66ef6-sb-api.accounts.nintendo.com" => "e97b8a9d672e4ce4845ec6947cd66ef6-sb.baas.nintendo.com", // dp1 environment
                    "api.accounts.nintendo.com"                                     => "e0d67c509fb203858ebcb2fe3f88c2aa.baas.nintendo.com",    // dp1 environment
                    "e97b8a9d672e4ce4845ec6947cd66ef6-sb.accounts.nintendo.com"     => "e97b8a9d672e4ce4845ec6947cd66ef6-sb.baas.nintendo.com", // lp1 environment
                    "accounts.nintendo.com"                                         => "e0d67c509fb203858ebcb2fe3f88c2aa.baas.nintendo.com",    // lp1 environment
                    /*
                        // TODO: Determine fields of the struct.
                        this + 0xEB8  => this + 0xEB8 + 0x300
                        this + 0x2BE8 => this + 0x2BE8 + 0x300
                    */
                    _ => address,
                };
            }

            return ResultCode.Success;
        }

        public ResultCode ResolveEx(ServiceCtx context, out ResultCode resultCode, out string resolvedAddress)
        {
            ulong inputPosition = context.Request.SendBuff[0].Position;
            ulong inputSize     = context.Request.SendBuff[0].Size;

            byte[] addressBuffer = new byte[inputSize];

            context.Memory.Read(inputPosition, addressBuffer);

            string address = Encoding.UTF8.GetString(addressBuffer).TrimEnd('\0');

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