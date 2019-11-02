using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl.Types
{
    class GetConfigurationArguments
    {
        public string Domain;
        public string Parameter;
        public byte[] Configuration;

        public static GetConfigurationArguments FromSpan(Span<byte> span)
        {
            string domain    = Encoding.ASCII.GetString(span.Slice(0, 0x41));
            string parameter = Encoding.ASCII.GetString(span.Slice(0x41, 0x41));

            GetConfigurationArguments result = new GetConfigurationArguments
            {
                Domain        = domain.Substring(0, domain.IndexOf('\0')),
                Parameter     = parameter.Substring(0, parameter.IndexOf('\0')),
                Configuration = span.Slice(0x82, 0x101).ToArray()
            };

            return result;
        }

        public void CopyTo(Span<byte> span)
        {
            Encoding.ASCII.GetBytes(Domain + '\0').CopyTo(span.Slice(0, 0x41));
            Encoding.ASCII.GetBytes(Parameter + '\0').CopyTo(span.Slice(0x41, 0x41));
            Configuration.CopyTo(span.Slice(0x82, 0x101));
        }
    }
}
