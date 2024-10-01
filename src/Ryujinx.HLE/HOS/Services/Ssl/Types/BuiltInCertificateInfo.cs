namespace Ryujinx.HLE.HOS.Services.Ssl.Types
{
    struct BuiltInCertificateInfo
    {
        public CaCertificateId Id;
        public TrustedCertStatus Status;
        public ulong CertificateDataSize;
        public ulong CertificateDataOffset;
    }
}
