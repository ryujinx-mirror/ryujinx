using Ryujinx.Common.Logging;
using System;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.Loaders.Mods
{
    class IpsPatcher
    {
        MemPatch _patches;

        public IpsPatcher(BinaryReader reader)
        {
            _patches = ParseIps(reader);
            if (_patches != null)
            {
                Logger.Info?.Print(LogClass.ModLoader, "IPS patch loaded successfully");
            }
        }

        private static MemPatch ParseIps(BinaryReader reader)
        {
            Span<byte> IpsHeaderMagic = Encoding.ASCII.GetBytes("PATCH").AsSpan();
            Span<byte> IpsTailMagic = Encoding.ASCII.GetBytes("EOF").AsSpan();
            Span<byte> Ips32HeaderMagic = Encoding.ASCII.GetBytes("IPS32").AsSpan();
            Span<byte> Ips32TailMagic = Encoding.ASCII.GetBytes("EEOF").AsSpan();

            MemPatch patches = new MemPatch();
            var header = reader.ReadBytes(IpsHeaderMagic.Length).AsSpan();

            if (header.Length != IpsHeaderMagic.Length)
            {
                return null;
            }

            bool is32;
            Span<byte> tailSpan;

            if (header.SequenceEqual(IpsHeaderMagic))
            {
                is32 = false;
                tailSpan = IpsTailMagic;
            }
            else if (header.SequenceEqual(Ips32HeaderMagic))
            {
                is32 = true;
                tailSpan = Ips32TailMagic;
            }
            else
            {
                return null;
            }

            byte[] buf = new byte[tailSpan.Length];

            bool ReadNext(int size) => reader.Read(buf, 0, size) != size;

            while (true)
            {
                if (ReadNext(buf.Length))
                {
                    return null;
                }

                if (buf.AsSpan().SequenceEqual(tailSpan))
                {
                    break;
                }

                int patchOffset = is32 ? buf[0] << 24 | buf[1] << 16 | buf[2] << 8 | buf[3]
                                  : buf[0] << 16 | buf[1] << 8 | buf[2];

                if (ReadNext(2))
                {
                    return null;
                }

                int patchSize = buf[0] << 8 | buf[1];

                if (patchSize == 0) // RLE/Fill mode
                {
                    if (ReadNext(2))
                    {
                        return null;
                    }

                    int fillLength = buf[0] << 8 | buf[1];

                    if (ReadNext(1))
                    {
                        return null;
                    }

                    patches.AddFill((uint)patchOffset, fillLength, buf[0]);
                }
                else // Copy mode
                {
                    var patch = reader.ReadBytes(patchSize);

                    if (patch.Length != patchSize)
                    {
                        return null;
                    }

                    patches.Add((uint)patchOffset, patch);
                }
            }

            return patches;
        }

        public void AddPatches(MemPatch patches)
        {
            patches.AddFrom(_patches);
        }
    }
}