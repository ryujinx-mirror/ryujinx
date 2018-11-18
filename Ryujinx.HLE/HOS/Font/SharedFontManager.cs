using LibHac;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.Resource;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Ryujinx.HLE.Utilities.FontUtils;

namespace Ryujinx.HLE.HOS.Font
{
    class SharedFontManager
    {
        private Switch Device;

        private long PhysicalAddress;

        private string FontsPath;

        private struct FontInfo
        {
            public int Offset;
            public int Size;

            public FontInfo(int Offset, int Size)
            {
                this.Offset = Offset;
                this.Size   = Size;
            }
        }

        private Dictionary<SharedFontType, FontInfo> FontData;

        public SharedFontManager(Switch Device, long PhysicalAddress)
        {
            this.PhysicalAddress = PhysicalAddress;

            this.Device = Device;

            FontsPath = Path.Combine(Device.FileSystem.GetSystemPath(), "fonts");
        }

        public void EnsureInitialized(ContentManager ContentManager)
        {
            if (FontData == null)
            {
                Device.Memory.FillWithZeros(PhysicalAddress, Horizon.FontSize);

                uint FontOffset = 0;

                FontInfo CreateFont(string Name)
                {
                    if (ContentManager.TryGetFontTitle(Name, out long FontTitle))
                    {
                        string ContentPath = ContentManager.GetInstalledContentPath(FontTitle, StorageId.NandSystem, ContentType.Data);
                        string FontPath    = Device.FileSystem.SwitchPathToSystemPath(ContentPath);

                        if (!string.IsNullOrWhiteSpace(FontPath))
                        {
                            int FileIndex = 0;

                            //Use second file in Chinese Font title for standard
                            if(Name == "FontChineseSimplified")
                            {
                                FileIndex = 1;
                            }

                            FileStream NcaFileStream = new FileStream(FontPath, FileMode.Open, FileAccess.Read);
                            Nca        Nca           = new Nca(Device.System.KeySet, NcaFileStream, false);
                            NcaSection RomfsSection  = Nca.Sections.FirstOrDefault(x => x?.Type == SectionType.Romfs);
                            Romfs      Romfs         = new Romfs(Nca.OpenSection(RomfsSection.SectionNum, false, Device.System.FsIntegrityCheckLevel));
                            Stream     FontFile      = Romfs.OpenFile(Romfs.Files[FileIndex]);

                            byte[] Data = DecryptFont(FontFile);

                            FontInfo Info = new FontInfo((int)FontOffset, Data.Length);

                            WriteMagicAndSize(PhysicalAddress + FontOffset, Data.Length);

                            FontOffset += 8;

                            uint Start = FontOffset;

                            for (; FontOffset - Start < Data.Length; FontOffset++)
                            {
                                Device.Memory.WriteByte(PhysicalAddress + FontOffset, Data[FontOffset - Start]);
                            }

                            NcaFileStream.Dispose();
                            Nca.Dispose();

                            return Info;
                        }
                    }

                    string FontFilePath = Path.Combine(FontsPath, Name + ".ttf");

                    if (File.Exists(FontFilePath))
                    {
                        byte[] Data = File.ReadAllBytes(FontFilePath);

                        FontInfo Info = new FontInfo((int)FontOffset, Data.Length);

                        WriteMagicAndSize(PhysicalAddress + FontOffset, Data.Length);

                        FontOffset += 8;

                        uint Start = FontOffset;

                        for (; FontOffset - Start < Data.Length; FontOffset++)
                        {
                            Device.Memory.WriteByte(PhysicalAddress + FontOffset, Data[FontOffset - Start]);
                        }

                        return Info;
                    }
                    else
                    {
                        throw new InvalidSystemResourceException($"Font \"{Name}.ttf\" not found. Please provide it in \"{FontsPath}\".");
                    }
                }

                FontData = new Dictionary<SharedFontType, FontInfo>()
                {
                    { SharedFontType.JapanUsEurope,       CreateFont("FontStandard")                  },
                    { SharedFontType.SimplifiedChinese,   CreateFont("FontChineseSimplified")         },
                    { SharedFontType.SimplifiedChineseEx, CreateFont("FontExtendedChineseSimplified") },
                    { SharedFontType.TraditionalChinese,  CreateFont("FontChineseTraditional")        },
                    { SharedFontType.Korean,              CreateFont("FontKorean")                    },
                    { SharedFontType.NintendoEx,          CreateFont("FontNintendoExtended")          }
                };

                if (FontOffset > Horizon.FontSize)
                {
                    throw new InvalidSystemResourceException(
                        $"The sum of all fonts size exceed the shared memory size. " +
                        $"Please make sure that the fonts don't exceed {Horizon.FontSize} bytes in total. " +
                        $"(actual size: {FontOffset} bytes).");
                }
            }
        }

        private void WriteMagicAndSize(long Position, int Size)
        {
            const int DecMagic = 0x18029a7f;
            const int Key      = 0x49621806;

            int EncryptedSize = EndianSwap.Swap32(Size ^ Key);

            Device.Memory.WriteInt32(Position + 0, DecMagic);
            Device.Memory.WriteInt32(Position + 4, EncryptedSize);
        }

        public int GetFontSize(SharedFontType FontType)
        {
            EnsureInitialized(Device.System.ContentManager);

            return FontData[FontType].Size;
        }

        public int GetSharedMemoryAddressOffset(SharedFontType FontType)
        {
            EnsureInitialized(Device.System.ContentManager);

            return FontData[FontType].Offset + 8;
        }
    }
}
