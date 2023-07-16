using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ssl.SslService;
using Ryujinx.HLE.HOS.Services.Ssl.Types;
using Ryujinx.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ssl
{
    [Service("ssl")]
    class ISslService : IpcService
    {
        // NOTE: The SSL service is used by games to connect it to various official online services, which we do not intend to support.
        //       In this case it is acceptable to stub all calls of the service.
        public ISslService(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateContext(nn::ssl::sf::SslVersion, u64, pid) -> object<nn::ssl::sf::ISslContext>
        public ResultCode CreateContext(ServiceCtx context)
        {
            SslVersion sslVersion = (SslVersion)context.RequestData.ReadUInt32();
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong pidPlaceholder = context.RequestData.ReadUInt64();
#pragma warning restore IDE0059

            MakeObject(context, new ISslContext(context.Request.HandleDesc.PId, sslVersion));

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { sslVersion });

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetCertificates(buffer<CaCertificateId, 5> ids) -> (u32 certificates_count, buffer<bytes, 6> certificates)
        public ResultCode GetCertificates(ServiceCtx context)
        {
            ReadOnlySpan<CaCertificateId> ids = MemoryMarshal.Cast<byte, CaCertificateId>(context.Memory.GetSpan(context.Request.SendBuff[0].Position, (int)context.Request.SendBuff[0].Size));

            if (!BuiltInCertificateManager.Instance.TryGetCertificates(
                ids,
                out BuiltInCertificateManager.CertStoreEntry[] entries,
                out bool hasAllCertificates,
                out int requiredSize))
            {
                throw new InvalidOperationException();
            }

            if ((uint)requiredSize > (uint)context.Request.ReceiveBuff[0].Size)
            {
                return ResultCode.InvalidCertBufSize;
            }

            int infosCount = entries.Length;

            if (hasAllCertificates)
            {
                infosCount++;
            }

            using (WritableRegion region = context.Memory.GetWritableRegion(context.Request.ReceiveBuff[0].Position, (int)context.Request.ReceiveBuff[0].Size))
            {
                Span<byte> rawData = region.Memory.Span;
                Span<BuiltInCertificateInfo> infos = MemoryMarshal.Cast<byte, BuiltInCertificateInfo>(rawData)[..infosCount];
                Span<byte> certificatesData = rawData[(Unsafe.SizeOf<BuiltInCertificateInfo>() * infosCount)..];

                for (int i = 0; i < entries.Length; i++)
                {
                    entries[i].Data.CopyTo(certificatesData);

                    infos[i] = new BuiltInCertificateInfo
                    {
                        Id = entries[i].Id,
                        Status = entries[i].Status,
                        CertificateDataSize = (ulong)entries[i].Data.Length,
                        CertificateDataOffset = (ulong)(rawData.Length - certificatesData.Length),
                    };

                    certificatesData = certificatesData[entries[i].Data.Length..];
                }

                if (hasAllCertificates)
                {
                    infos[entries.Length] = new BuiltInCertificateInfo
                    {
                        Id = CaCertificateId.All,
                        Status = TrustedCertStatus.Invalid,
                        CertificateDataSize = 0,
                        CertificateDataOffset = 0,
                    };
                }
            }

            context.ResponseData.Write(entries.Length);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetCertificateBufSize(buffer<CaCertificateId, 5> ids) -> u32 buffer_size;
        public ResultCode GetCertificateBufSize(ServiceCtx context)
        {
            ReadOnlySpan<CaCertificateId> ids = MemoryMarshal.Cast<byte, CaCertificateId>(context.Memory.GetSpan(context.Request.SendBuff[0].Position, (int)context.Request.SendBuff[0].Size));

            if (!BuiltInCertificateManager.Instance.TryGetCertificates(ids, out _, out _, out int requiredSize))
            {
                throw new InvalidOperationException();
            }

            context.ResponseData.Write(requiredSize);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // SetInterfaceVersion(u32)
        public ResultCode SetInterfaceVersion(ServiceCtx context)
        {
            // 1 = 3.0.0+, 2 = 5.0.0+, 3 = 6.0.0+
            uint interfaceVersion = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { interfaceVersion });

            return ResultCode.Success;
        }
    }
}
