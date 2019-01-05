using LibHac;
using LibHac.IO;
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
        private Switch _device;

        private long _physicalAddress;

        private string _fontsPath;

        private struct FontInfo
        {
            public int Offset;
            public int Size;

            public FontInfo(int offset, int size)
            {
                Offset = offset;
                Size   = size;
            }
        }

        private Dictionary<SharedFontType, FontInfo> _fontData;

        public SharedFontManager(Switch device, long physicalAddress)
        {
            _physicalAddress = physicalAddress;

            _device = device;

            _fontsPath = Path.Combine(device.FileSystem.GetSystemPath(), "fonts");
        }

        public void EnsureInitialized(ContentManager contentManager)
        {
            if (_fontData == null)
            {
                _device.Memory.FillWithZeros(_physicalAddress, Horizon.FontSize);

                uint fontOffset = 0;

                FontInfo CreateFont(string name)
                {
                    if (contentManager.TryGetFontTitle(name, out long fontTitle))
                    {
                        string contentPath = contentManager.GetInstalledContentPath(fontTitle, StorageId.NandSystem, ContentType.Data);
                        string fontPath    = _device.FileSystem.SwitchPathToSystemPath(contentPath);

                        if (!string.IsNullOrWhiteSpace(fontPath))
                        {
                            int fileIndex = 0;

                            //Use second file in Chinese Font title for standard
                            if(name == "FontChineseSimplified")
                            {
                                fileIndex = 1;
                            }

                            byte[] data;
                            
                            using (FileStream ncaFileStream = new FileStream(fontPath, FileMode.Open, FileAccess.Read))
                            {
                                Nca        nca          = new Nca(_device.System.KeySet, ncaFileStream.AsStorage(), false);
                                NcaSection romfsSection = nca.Sections.FirstOrDefault(x => x?.Type == SectionType.Romfs);
                                Romfs      romfs        = new Romfs(nca.OpenSection(romfsSection.SectionNum, false, _device.System.FsIntegrityCheckLevel, false));
                                Stream     fontFile     = romfs.OpenFile(romfs.Files[fileIndex]).AsStream();

                                data = DecryptFont(fontFile);
                            }
                                
                            FontInfo info = new FontInfo((int)fontOffset, data.Length);

                            WriteMagicAndSize(_physicalAddress + fontOffset, data.Length);

                            fontOffset += 8;

                            uint start = fontOffset;

                            for (; fontOffset - start < data.Length; fontOffset++)
                            {
                                _device.Memory.WriteByte(_physicalAddress + fontOffset, data[fontOffset - start]);
                            }

                            return info;
                        }
                    }

                    string fontFilePath = Path.Combine(_fontsPath, name + ".ttf");

                    if (File.Exists(fontFilePath))
                    {
                        byte[] data = File.ReadAllBytes(fontFilePath);

                        FontInfo info = new FontInfo((int)fontOffset, data.Length);

                        WriteMagicAndSize(_physicalAddress + fontOffset, data.Length);

                        fontOffset += 8;

                        uint start = fontOffset;

                        for (; fontOffset - start < data.Length; fontOffset++)
                        {
                            _device.Memory.WriteByte(_physicalAddress + fontOffset, data[fontOffset - start]);
                        }

                        return info;
                    }
                    else
                    {
                        throw new InvalidSystemResourceException($"Font \"{name}.ttf\" not found. Please provide it in \"{_fontsPath}\".");
                    }
                }

                _fontData = new Dictionary<SharedFontType, FontInfo>
                {
                    { SharedFontType.JapanUsEurope,       CreateFont("FontStandard")                  },
                    { SharedFontType.SimplifiedChinese,   CreateFont("FontChineseSimplified")         },
                    { SharedFontType.SimplifiedChineseEx, CreateFont("FontExtendedChineseSimplified") },
                    { SharedFontType.TraditionalChinese,  CreateFont("FontChineseTraditional")        },
                    { SharedFontType.Korean,              CreateFont("FontKorean")                    },
                    { SharedFontType.NintendoEx,          CreateFont("FontNintendoExtended")          }
                };

                if (fontOffset > Horizon.FontSize)
                {
                    throw new InvalidSystemResourceException(
                        $"The sum of all fonts size exceed the shared memory size. " +
                        $"Please make sure that the fonts don't exceed {Horizon.FontSize} bytes in total. " +
                        $"(actual size: {fontOffset} bytes).");
                }
            }
        }

        private void WriteMagicAndSize(long position, int size)
        {
            const int decMagic = 0x18029a7f;
            const int key      = 0x49621806;

            int encryptedSize = EndianSwap.Swap32(size ^ key);

            _device.Memory.WriteInt32(position + 0, decMagic);
            _device.Memory.WriteInt32(position + 4, encryptedSize);
        }

        public int GetFontSize(SharedFontType fontType)
        {
            EnsureInitialized(_device.System.ContentManager);

            return _fontData[fontType].Size;
        }

        public int GetSharedMemoryAddressOffset(SharedFontType fontType)
        {
            EnsureInitialized(_device.System.ContentManager);

            return _fontData[fontType].Offset + 8;
        }
    }
}
