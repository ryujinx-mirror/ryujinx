using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Impl
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum WsaError
    {
        /*
        * All Windows Sockets error constants are biased by WSABASEERR from
        * the "normal"
        */
        WSABASEERR = 10000,

        /*
        * Windows Sockets definitions of regular Microsoft C error constants
        */
        WSAEINTR = (WSABASEERR + 4),
        WSAEBADF = (WSABASEERR + 9),
        WSAEACCES = (WSABASEERR + 13),
        WSAEFAULT = (WSABASEERR + 14),
        WSAEINVAL = (WSABASEERR + 22),
        WSAEMFILE = (WSABASEERR + 24),

        /*
         * Windows Sockets definitions of regular Berkeley error constants
         */
        WSAEWOULDBLOCK = (WSABASEERR + 35),
        WSAEINPROGRESS = (WSABASEERR + 36),
        WSAEALREADY = (WSABASEERR + 37),
        WSAENOTSOCK = (WSABASEERR + 38),
        WSAEDESTADDRREQ = (WSABASEERR + 39),
        WSAEMSGSIZE = (WSABASEERR + 40),
        WSAEPROTOTYPE = (WSABASEERR + 41),
        WSAENOPROTOOPT = (WSABASEERR + 42),
        WSAEPROTONOSUPPORT = (WSABASEERR + 43),
        WSAESOCKTNOSUPPORT = (WSABASEERR + 44),
        WSAEOPNOTSUPP = (WSABASEERR + 45),
        WSAEPFNOSUPPORT = (WSABASEERR + 46),
        WSAEAFNOSUPPORT = (WSABASEERR + 47),
        WSAEADDRINUSE = (WSABASEERR + 48),
        WSAEADDRNOTAVAIL = (WSABASEERR + 49),
        WSAENETDOWN = (WSABASEERR + 50),
        WSAENETUNREACH = (WSABASEERR + 51),
        WSAENETRESET = (WSABASEERR + 52),
        WSAECONNABORTED = (WSABASEERR + 53),
        WSAECONNRESET = (WSABASEERR + 54),
        WSAENOBUFS = (WSABASEERR + 55),
        WSAEISCONN = (WSABASEERR + 56),
        WSAENOTCONN = (WSABASEERR + 57),
        WSAESHUTDOWN = (WSABASEERR + 58),
        WSAETOOMANYREFS = (WSABASEERR + 59),
        WSAETIMEDOUT = (WSABASEERR + 60),
        WSAECONNREFUSED = (WSABASEERR + 61),
        WSAELOOP = (WSABASEERR + 62),
        WSAENAMETOOLONG = (WSABASEERR + 63),
        WSAEHOSTDOWN = (WSABASEERR + 64),
        WSAEHOSTUNREACH = (WSABASEERR + 65),
        WSAENOTEMPTY = (WSABASEERR + 66),
        WSAEPROCLIM = (WSABASEERR + 67),
        WSAEUSERS = (WSABASEERR + 68),
        WSAEDQUOT = (WSABASEERR + 69),
        WSAESTALE = (WSABASEERR + 70),
        WSAEREMOTE = (WSABASEERR + 71),

        /*
         * Extended Windows Sockets error constant definitions
         */
        WSASYSNOTREADY = (WSABASEERR + 91),
        WSAVERNOTSUPPORTED = (WSABASEERR + 92),
        WSANOTINITIALISED = (WSABASEERR + 93),
        WSAEDISCON = (WSABASEERR + 101),
        WSAENOMORE = (WSABASEERR + 102),
        WSAECANCELLED = (WSABASEERR + 103),
        WSAEINVALIDPROCTABLE = (WSABASEERR + 104),
        WSAEINVALIDPROVIDER = (WSABASEERR + 105),
        WSAEPROVIDERFAILEDINIT = (WSABASEERR + 106),
        WSASYSCALLFAILURE = (WSABASEERR + 107),
        WSASERVICE_NOT_FOUND = (WSABASEERR + 108),
        WSATYPE_NOT_FOUND = (WSABASEERR + 109),
        WSA_E_NO_MORE = (WSABASEERR + 110),
        WSA_E_CANCELLED = (WSABASEERR + 111),
        WSAEREFUSED = (WSABASEERR + 112),

        /*
         * Error return codes from gethostbyname() and gethostbyaddr()
         * (when using the resolver). Note that these errors are
         * retrieved via WSAGetLastError() and must therefore follow
         * the rules for avoiding clashes with error numbers from
         * specific implementations or language run-time systems.
         * For this reason the codes are based at WSABASEERR+1001.
         * Note also that [WSA]NO_ADDRESS is defined only for
         * compatibility purposes.
         */

        /* Authoritative Answer: Host not found */
        WSAHOST_NOT_FOUND = (WSABASEERR + 1001),

        /* Non-Authoritative: Host not found, or SERVERFAIL */
        WSATRY_AGAIN = (WSABASEERR + 1002),

        /* Non-recoverable errors, FORMERR, REFUSED, NOTIMP */
        WSANO_RECOVERY = (WSABASEERR + 1003),

        /* Valid name, no data record of requested type */
        WSANO_DATA = (WSABASEERR + 1004),

        /*
         * Define QOS related error return codes
         *
         */
        WSA_QOS_RECEIVERS = (WSABASEERR + 1005),
        /* at least one Reserve has arrived */
        WSA_QOS_SENDERS = (WSABASEERR + 1006),
        /* at least one Path has arrived */
        WSA_QOS_NO_SENDERS = (WSABASEERR + 1007),
        /* there are no senders */
        WSA_QOS_NO_RECEIVERS = (WSABASEERR + 1008),
        /* there are no receivers */
        WSA_QOS_REQUEST_CONFIRMED = (WSABASEERR + 1009),
        /* Reserve has been confirmed */
        WSA_QOS_ADMISSION_FAILURE = (WSABASEERR + 1010),
        /* error due to lack of resources */
        WSA_QOS_POLICY_FAILURE = (WSABASEERR + 1011),
        /* rejected for administrative reasons - bad credentials */
        WSA_QOS_BAD_STYLE = (WSABASEERR + 1012),
        /* unknown or conflicting style */
        WSA_QOS_BAD_OBJECT = (WSABASEERR + 1013),
        /* problem with some part of the filterspec or providerspecific
         * buffer in general */
        WSA_QOS_TRAFFIC_CTRL_ERROR = (WSABASEERR + 1014),
        /* problem with some part of the flowspec */
        WSA_QOS_GENERIC_ERROR = (WSABASEERR + 1015),
    }
}
