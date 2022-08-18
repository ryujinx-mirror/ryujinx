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

        [CommandHipc(0)]
        // CreateContext(nn::ssl::sf::SslVersion, u64, pid) -> object<nn::ssl::sf::ISslContext>
        public ResultCode CreateContext(ServiceCtx context)
        {
            SslVersion sslVersion     = (SslVersion)context.RequestData.ReadUInt32();
            ulong      pidPlaceholder = context.RequestData.ReadUInt64();

            MakeObject(context, new ISslContext(context.Request.HandleDesc.PId, sslVersion));

            Logger.Stub?.PrintStub(LogClass.ServiceSsl, new { sslVersion });

            return ResultCode.Success;
        }

        private uint ComputeCertificateBufferSizeRequired(ReadOnlySpan<BuiltInCertificateManager.CertStoreEntry> entries)
        {
            uint totalSize = 0;

            for (int i = 0; i < entries.Length; i++)
            {
                totalSize += (uint)Unsafe.SizeOf<BuiltInCertificateInfo>();
                totalSize += (uint)entries[i].Data.Length;
            }

            return totalSize;
        }

        [CommandHipc(2)]
        // GetCertificates(buffer<CaCertificateId, 5> ids) -> (u32 certificates_count, buffer<bytes, 6> certificates)
        public ResultCode GetCertificates(ServiceCtx context)
        {
            ReadOnlySpan<CaCertificateId> ids = MemoryMarshal.Cast<byte, CaCertificateId>(context.Memory.GetSpan(context.Request.SendBuff[0].Position, (int)context.Request.SendBuff[0].Size));

            if (!BuiltInCertificateManager.Instance.TryGetCertificates(ids, out BuiltInCertificateManager.CertStoreEntry[] entries))
            {
                throw new InvalidOperationException();
            }

            if (ComputeCertificateBufferSizeRequired(entries) > context.Request.ReceiveBuff[0].Size)
            {
                return ResultCode.InvalidCertBufSize;
            }

            using (WritableRegion region = context.Memory.GetWritableRegion(context.Request.ReceiveBuff[0].Position, (int)context.Request.ReceiveBuff[0].Size))
            {
                Span<byte> rawData = region.Memory.Span;
                Span<BuiltInCertificateInfo> infos = MemoryMarshal.Cast<byte, BuiltInCertificateInfo>(rawData)[..entries.Length];
                Span<byte> certificatesData = rawData[(Unsafe.SizeOf<BuiltInCertificateInfo>() * entries.Length)..];

                for (int i = 0; i < infos.Length; i++)
                {
                    entries[i].Data.CopyTo(certificatesData);

                    infos[i] = new BuiltInCertificateInfo
                    {
                        Id = entries[i].Id,
                        Status = entries[i].Status,
                        CertificateDataSize = (ulong)entries[i].Data.Length,
                        CertificateDataOffset = (ulong)(rawData.Length - certificatesData.Length)
                    };

                    certificatesData = certificatesData[entries[i].Data.Length..];
                }
            }

            context.ResponseData.Write(entries.Length);

            return ResultCode.Success;
        }

        [CommandHipc(3)]
        // GetCertificateBufSize(buffer<CaCertificateId, 5> ids) -> u32 buffer_size;
        public ResultCode GetCertificateBufSize(ServiceCtx context)
        {
            ReadOnlySpan<CaCertificateId> ids = MemoryMarshal.Cast<byte, CaCertificateId>(context.Memory.GetSpan(context.Request.SendBuff[0].Position, (int)context.Request.SendBuff[0].Size));

            if (!BuiltInCertificateManager.Instance.TryGetCertificates(ids, out BuiltInCertificateManager.CertStoreEntry[] entries))
            {
                throw new InvalidOperationException();
            }

            context.ResponseData.Write(ComputeCertificateBufferSizeRequired(entries));

            return ResultCode.Success;
        }

        [CommandHipc(5)]
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