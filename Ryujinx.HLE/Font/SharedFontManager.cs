using ChocolArm64.Exceptions;
using ChocolArm64.Memory;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.Resource;
using System;
using System.Collections.Generic;
using System.IO;


namespace Ryujinx.HLE.Font
{
    public class SharedFontManager
    {
        private const uint SharedMemorySize = 0x1100000;
        private Logger Log;

        private string FontsPath;

        private object ShMemLock;

        private (AMemory, long, long)[] ShMemPositions;

        private Dictionary<SharedFontType, byte[]> FontData;

        private uint[] LoadedFonts;

        public SharedFontManager(Logger Log, string SystemPath)
        {
            this.Log          = Log;
            this.FontsPath    = Path.Combine(SystemPath, "fonts");

            ShMemLock         = new object();

            ShMemPositions    = new(AMemory, long, long)[0];

            FontData          = new Dictionary<SharedFontType, byte[]>()
            {
                { SharedFontType.JapanUsEurope,       GetData("FontStandard")                  },
                { SharedFontType.SimplifiedChinese,   GetData("FontChineseSimplified")         },
                { SharedFontType.SimplifiedChineseEx, GetData("FontExtendedChineseSimplified") },
                { SharedFontType.TraditionalChinese,  GetData("FontChineseTraditional")        },
                { SharedFontType.Korean,              GetData("FontKorean")                    },
                { SharedFontType.NintendoEx,          GetData("FontNintendoExtended")          }
            };

            int FontMemoryUsage = 0;
            foreach (byte[] data in FontData.Values)
            {
                FontMemoryUsage += data.Length;
                FontMemoryUsage += 0x8;
            }

            if (FontMemoryUsage > SharedMemorySize)
            {
                throw new InvalidSystemResourceException($"The sum of all fonts size exceed the shared memory size. Please make sure that the fonts don't exceed {SharedMemorySize} bytes in total. (actual size: {FontMemoryUsage} bytes)");
            }

            LoadedFonts       = new uint[FontData.Count];
        }

        public byte[] GetData(string FontName)
        {
            string FontFilePath = Path.Combine(FontsPath, $"{FontName}.ttf");
            if (File.Exists(FontFilePath))
            {
                return File.ReadAllBytes(FontFilePath);
            }
            else
            {
                throw new InvalidSystemResourceException($"Font \"{FontName}.ttf\" not found. Please provide it in \"{FontsPath}\".");
            }
        }

        public void MapFont(SharedFontType FontType, AMemory Memory, long Position)
        {
            uint SharedMemoryAddressOffset = GetSharedMemoryAddressOffset(FontType);
            // TODO: find what are the 8 bytes before the font
            Memory.WriteUInt64(Position + SharedMemoryAddressOffset - 8, 0);
            Memory.WriteBytes(Position + SharedMemoryAddressOffset, FontData[FontType]);
        }

        public void PropagateNewMapFont(SharedFontType Type)
        {
            lock (ShMemLock)
            {
                foreach ((AMemory Memory, long Position, long Size) in ShMemPositions)
                {
                    AMemoryMapInfo MemoryInfo = Memory.Manager.GetMapInfo(Position);

                    if (MemoryInfo == null)
                    {
                        throw new VmmPageFaultException(Position);
                    }

                    // The memory is read only, we need to changes that to add the new font
                    AMemoryPerm originalPerms = MemoryInfo.Perm;
                    Memory.Manager.Reprotect(Position, Size, AMemoryPerm.RW);
                    MapFont(Type, Memory, Position);
                    Memory.Manager.Reprotect(Position, Size, originalPerms);
                }
            }
        }

        internal void ShMemMap(object sender, EventArgs e)
        {
            HSharedMem SharedMem = (HSharedMem)sender;

            lock (ShMemLock)
            {
                ShMemPositions = SharedMem.GetVirtualPositions();

                (AMemory Memory, long Position, long Size) = ShMemPositions[ShMemPositions.Length - 1];

                for (int Type = 0; Type < LoadedFonts.Length; Type++)
                {
                    if (LoadedFonts[(int)Type] == 1)
                    {
                        MapFont((SharedFontType)Type, Memory, Position);
                    }
                }
            }
        }

        internal void ShMemUnmap(object sender, EventArgs e)
        {
            HSharedMem SharedMem = (HSharedMem)sender;

            lock (ShMemLock)
            {
                ShMemPositions = SharedMem.GetVirtualPositions();
            }
        }

        public void Load(SharedFontType FontType)
        {
            if (LoadedFonts[(int)FontType] == 0)
            {
                PropagateNewMapFont(FontType);
            }

            LoadedFonts[(int)FontType] = 1;
        }

        public uint GetLoadState(SharedFontType FontType)
        {
            if (LoadedFonts[(int)FontType] != 1)
            {
                // Some games don't request a load, so we need to load it here.
                Load(FontType);
                return 0;
            }
            return LoadedFonts[(int)FontType];
        }

        public uint GetFontSize(SharedFontType FontType)
        {
            return (uint)FontData[FontType].Length;
        }

        public uint GetSharedMemoryAddressOffset(SharedFontType FontType)
        {
            uint Pos = 0x8;

            for (SharedFontType Type = SharedFontType.JapanUsEurope; Type < FontType; Type++)
            {
                Pos += GetFontSize(Type);
                Pos += 0x8;
            }

            return Pos;
        }

        public int Count => FontData.Count;
    }
}
