using System.Text.RegularExpressions;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres.Proxy
{
    static class DnsBlacklist
    {
        const RegexOptions RegexOpts = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled;

        private static readonly Regex[] BlockedHosts = new Regex[]
        {
            new Regex(@"^(.*)\-lp1\.(n|s)\.n\.srv\.nintendo\.net$", RegexOpts),
            new Regex(@"^(.*)\-lp1\.lp1\.t\.npln\.srv\.nintendo\.net$", RegexOpts),
            new Regex(@"^(.*)\-lp1\.(znc|p)\.srv\.nintendo\.net$", RegexOpts),
            new Regex(@"^(.*)\-sb\-api\.accounts\.nintendo\.com$", RegexOpts),
            new Regex(@"^(.*)\-sb\.accounts\.nintendo\.com$", RegexOpts),
            new Regex(@"^accounts\.nintendo\.com$", RegexOpts)
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