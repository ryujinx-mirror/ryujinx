using System.Text.RegularExpressions;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres.Proxy
{
    static class DnsBlacklist
    {
        private static readonly Regex[] BlockedHosts = new Regex[]
        {
            new Regex(@"^g(.*)\-lp1\.s\.n\.srv\.nintendo\.net$"),
            new Regex(@"^(.*)\-sb\-api\.accounts\.nintendo\.com$"),
            new Regex(@"^(.*)\-sb\.accounts\.nintendo\.com$"),
            new Regex(@"^accounts\.nintendo\.com$")
        };

        public static bool IsHostBlocked(string host)
        {
            foreach (Regex regex in BlockedHosts)
            {
                if (regex.IsMatch(host))
                {
                    return true;
                }
            }

            return false;
        }
    }
}